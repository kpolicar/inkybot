using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using Inkybot;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ImageMagick;
using ImageMagick.Factories;
using Inkybot.Events;
using Inkybot.Exceptions;
using Inkybot.Helpers;
using Tesseract;
using Debug = System.Diagnostics.Debug;
using ImageFormat = Tesseract.ImageFormat;

namespace Inkybot.Services
{
    public partial class ScreenReaderDataProvider
    {
        public class MinMaxScreenScanner : ScreenScanner
        {
            private readonly RowSpacingDetector _rowDetector;

            public MinMaxScreenScanner(RowSpacingDetector rowDetector, Responsive.Measurement regionOfInterest, Func<string, string[]>? split = null, ImagePreprocessor? preprocessor = null, PageSegMode segMode = PageSegMode.SingleBlock)
                : base(regionOfInterest, split, preprocessor, segMode) {
                _rowDetector = rowDetector;
                SetVariables(engine => {
                    engine.SetVariable("tessedit_char_whitelist", "0123456789-%");
                    engine.SetVariable("load_system_dawg", "0");
                    engine.SetVariable("load_freq_dawg", "0");
                    engine.SetVariable("load_unambig_dawg", "0");
                    engine.SetVariable("load_punc_dawg", "0");
                    engine.SetVariable("load_number_dawg", "0");
                    engine.SetVariable("classify_bln_numeric_mode", "0");
                    engine.SetVariable("classify_enable_learning", "0");
                    engine.SetVariable("classify_enable_adaptive_matcher", "0");
                    engine.SetVariable("tessedit_enable_doc_dict", "0");
                });
            }

            protected override TesseractEngine CreateEngine() {
                return CreateTesseractEngine("eng-numbers", EngineMode.TesseractOnly);
            }

            public override event EventHandler<TesseractPageProcessed>? PageProcessed;
            public override event EventHandler<FileSystemEventArgs>? Saved;
            
            public override string[] ScanRegion(Image screenshot, int screenshotHeight, TesseractEngine engine, bool saveToDisk = false) {
                var totalSw = System.Diagnostics.Stopwatch.StartNew();

                var bounds = CalculateBounds(screenshot);

                var image = /*preprocessor is ResizeImagePreprocessor resizeImagePreprocessor
                    ? (Bitmap) resizeImagePreprocessor.PreprocessImage(screenshot, bounds, ratioFromOptimalScreenshotHeight(1080))
                    : */(Bitmap) preprocessor.PreprocessImage(screenshot, bounds);

                if (saveToDisk) {
                    var folderPath = Path.Combine(AppContext.BaseDirectory, @"debug\images");
                    Directory.CreateDirectory(folderPath);
                    var fileName = Path.GetRandomFileName() + ".bmp";

                    PixConverter.ToPix(image).Save(folderPath + "/" + fileName);
                    Saved?.Invoke(this, new FileSystemEventArgs(
                        WatcherChangeTypes.Created, folderPath, fileName));
                }

                var m = new MagickFactory();
                MagickImage magickImage = new MagickImage(m.Image.Create(image));

                // Trigger detection (updates ReferenceRowHeight if dimensions changed)
                _rowDetector.GetRowCount(image, bounds.Height);

                // Derive slice height from the same ReferenceRowHeight the indicators use,
                // scaled from reference space (1694x1009) to preprocessed image space
                const double ReferenceColumnHeight = 846 - 314; // 532
                double sliceHeight = _rowDetector.ReferenceRowHeight * image.Height / ReferenceColumnHeight;
                int sliceCount = 13;

                // 2px inset in reference space, scaled to preprocessed image space
                double insetY = 2.0 * image.Height / ReferenceColumnHeight;
                double refColumnWidth = regionOfInterest.Rectangle.Width;
                double insetX = 2.0 * image.Width / refColumnWidth;

                IEnumerable<string> textLines = new string[] {};

                for (var i = 0; i < sliceCount; i++) {
                    int y = (int)Math.Round(i * sliceHeight + insetY);
                    int h = (int)Math.Round((i + 1) * sliceHeight - insetY) - y;
                    int x = (int)Math.Round(insetX);
                    int w = (int)(magickImage.Width - insetX * 2);
                    if (h < 1) h = 1;
                    if (w < 1) w = 1;

                    var slice = (MagickImage)magickImage.Clone();
                    slice.Crop(new MagickGeometry(x, y, (uint)w, (uint)h));
                    slice.ResetPage();

                    using var canvas = new MagickImage(MagickColors.White, slice.Width + 150, slice.Height + 150);
                    canvas.Composite(slice, 75, 75, CompositeOperator.Over);

                    var sliceBmp = canvas.ToBitmap();
                    sliceBmp.Save(Path.Combine(AppContext.BaseDirectory, @"debug\images", $"slice_{i}.bmp"), System.Drawing.Imaging.ImageFormat.Bmp);

                    var ocrSw = System.Diagnostics.Stopwatch.StartNew();
                    var ocrPage = ProcessImage(engine, sliceBmp);
                    var ocrMs = ocrSw.ElapsedMilliseconds;
                    var scanned = ocrPage.GetText().Replace(Environment.NewLine, "").Trim();
                    PageProcessed?.Invoke(this, new TesseractPageProcessed(image, ocrPage, scanned));

                    if (scanned == "") {
                        slice.Resize(new Percentage(130));
                        ocrPage.Dispose();
                        ocrSw.Restart();
                        using var ocrPage2 = ProcessImage(engine, slice.ToBitmap());
                        ocrMs += ocrSw.ElapsedMilliseconds;
                        scanned = ocrPage2.GetText().Replace(Environment.NewLine, "").Trim();
                    }

                    Profiler.Record("OCR", "MinMax.slice", ocrMs);

                    if (scanned == "") scanned = "-";
                    if (scanned != "-" || !textLines.Any(s => s != "-")) {
                        textLines = textLines.Append(scanned);
                    }

                    ocrPage.Dispose();
                }

                Profiler.Record("OCR", "MinMax.total", totalSw.ElapsedMilliseconds);
                return textLines.ToArray();
            }
        }

        public class ScreenScanner : IDisposable
        {
            public virtual event EventHandler<TesseractPageProcessed>? PageProcessed;
            public virtual event EventHandler<FileSystemEventArgs>? Saved;

            private readonly ConcurrentBag<TesseractEngine> _enginePool = new();
            private readonly SemaphoreSlim _engineAvailable = new(2, 2);
            protected Responsive.Measurement regionOfInterest;
            protected Func<string, string[]>? split;

            public ImagePreprocessor preprocessor {
                private set;
                get;
            }
            private PageSegMode segMode;


            public ScreenScanner(Responsive.Measurement regionOfInterest,
                Func<string, string[]>? split = null,
                ImagePreprocessor? preprocessor = null,
                PageSegMode segMode = PageSegMode.SingleBlock) {
                _enginePool.Add(CreateEngine());
                _enginePool.Add(CreateEngine());
                this.preprocessor = preprocessor ?? new ImagePreprocessor();
                this.regionOfInterest = regionOfInterest;
                this.split = split;
                this.segMode = segMode;
            }

            protected virtual TesseractEngine CreateEngine() {
                return CreateTesseractEngine(CultureInfo.CurrentUICulture.ThreeLetterISOLanguageName, EngineMode.Default);
            }

            protected TesseractEngine CreateTesseractEngine(string lang, EngineMode mode) {
                var tessdataPath = Path.Combine(AppContext.BaseDirectory, @"Resources\Tesseract");
                try {
                    var eng = new TesseractEngine(tessdataPath, lang, mode);
                    FileEventLogger.SystemLogger.Info("Tesseract engine initialized: " + lang + " from " + tessdataPath);
                    eng.SetVariable("debug", "0");
                    return eng;
                } catch (Exception ex) {
                    FileEventLogger.SystemLogger.Error(ex, "Failed to initialize Tesseract engine (" + lang + ") from " + tessdataPath + ": " + ex.Message);
                    throw;
                }
            }

            protected TesseractEngine AcquireEngine() {
                _engineAvailable.Wait();
                _enginePool.TryTake(out var eng);
                return eng!;
            }

            protected void ReleaseEngine(TesseractEngine eng) {
                _enginePool.Add(eng);
                _engineAvailable.Release();
            }

            public void SetVariables(Action<TesseractEngine> callback) {
                foreach (var eng in _enginePool) callback(eng);
            }

            public void SetRegion(Responsive.Measurement regionOfInterest) {
                lock (this) {
                    this.regionOfInterest = regionOfInterest;
                }
            }

            public Rectangle CalculateBounds(Image image) {
                lock (image) {
                    return Responsive.ResponsiveRectangle(regionOfInterest, image.Width, image.Height);
                }
            }

            public Task<string[]> ScanRegionAsync(Image screenshot, int screenshotHeight, bool saveToDisk = false) {
                return Task.Run(() => {
                    var eng = AcquireEngine();
                    try {
                        return ScanRegion(screenshot, screenshotHeight, eng, saveToDisk);
                    } finally {
                        ReleaseEngine(eng);
                    }
                });
            }
            
            public double ratioFromOptimalScreenshotHeight(int screenshotHeight) =>
                (1d*optimalScreenshotHeight)/(1d*screenshotHeight);
            private readonly int optimalScreenshotHeight = 1080; //1920x1080

            public virtual string[] ScanRegion(Image screenshot, int screenshotHeight, TesseractEngine engine, bool saveToDisk = false) {
                var totalSw = System.Diagnostics.Stopwatch.StartNew();
                var bounds = CalculateBounds(screenshot);

                var image = /*preprocessor is ResizeImagePreprocessor resizeImagePreprocessor
                    ? (Bitmap) resizeImagePreprocessor.PreprocessImage(screenshot, bounds, ratioFromOptimalScreenshotHeight(1080))
                    :*/ (Bitmap) preprocessor.PreprocessImage(screenshot, bounds);

                if (saveToDisk) {
                    var folderPath = Path.Combine(AppContext.BaseDirectory, @"debug\images");
                    Directory.CreateDirectory(folderPath);
                    var fileName = Path.GetRandomFileName() + ".bmp";

                    PixConverter.ToPix(image).Save(folderPath + "/" + fileName);
                    Saved?.Invoke(this, new FileSystemEventArgs(
                        WatcherChangeTypes.Created, folderPath, fileName));
                }

                var ocrSw = System.Diagnostics.Stopwatch.StartNew();
                using (var ocrPage = ProcessImage(engine, image)) {
                    Profiler.Record("OCR", $"{GetType().Name}.process", ocrSw.ElapsedMilliseconds);
                    var scanned = ocrPage.GetText();
                    PageProcessed?.Invoke(this, new TesseractPageProcessed(image, ocrPage, scanned));

                    var textLines = split?.Invoke(scanned) ?? new[] {scanned};

                    Profiler.Record("OCR", $"{GetType().Name}.total", totalSw.ElapsedMilliseconds);
                    return textLines
                        .Select(text => text.Replace("\n", " "))
                        .ToArray();
                }
            }

            protected Page ProcessImage(TesseractEngine engine, Bitmap image) {
                try {
                    return engine.Process(image, segMode);
                } catch (InvalidOperationException exception) {
                    Console.WriteLine(exception.Message);
                    throw new OcrEngineNotReadyYetException("OCR engine is unavailable, try again in a moment.",
                        exception);
                }
            }

            public void Dispose() {
                while (_enginePool.TryTake(out var eng)) eng?.Dispose();
            }
        }

        public class TextScreenScanner : ScreenScanner
        {
            public TextScreenScanner(Responsive.Measurement regionOfInterest,
                Func<string, string[]>? split = null,
                ImagePreprocessor? preprocessor = null,
                PageSegMode segMode = PageSegMode.SingleBlock) : base(regionOfInterest, split, preprocessor, segMode) {
                SetVariables(engine => {
                    engine.SetVariable("tessedit_char_whitelist", Properties.Resources.OcrCharWhitelist);
                    engine.SetVariable("load_system_dawg", "0");
                    engine.SetVariable("load_freq_dawg", "0");
                    engine.SetVariable("load_unambig_dawg", "0");
                    engine.SetVariable("classify_enable_learning", "0");
                    engine.SetVariable("classify_enable_adaptive_matcher", "0");
                });
            }

            protected override TesseractEngine CreateEngine() {
                return CreateTesseractEngine(CultureInfo.CurrentUICulture.ThreeLetterISOLanguageName, EngineMode.Default);
            }
        }

        public class StatValuesScreenScanner : TextScreenScanner
        {
            private readonly RowSpacingDetector _rowDetector;
            private readonly Responsive.Measurement _fullBounds;

            public StatValuesScreenScanner(RowSpacingDetector rowDetector, Responsive.Measurement regionOfInterest,
                Func<string, string[]>? split = null,
                ImagePreprocessor? preprocessor = null,
                PageSegMode segMode = PageSegMode.SingleBlock)
                : base(regionOfInterest, split, preprocessor, segMode) {
                _rowDetector = rowDetector;
                _fullBounds = regionOfInterest;
            }

            public override string[] ScanRegion(Image screenshot, int screenshotHeight, TesseractEngine engine, bool saveToDisk = false) {
                _rowDetector.WaitForDetection();
                SetRegion(Measurements.StatColumnBounds(_fullBounds));
                return base.ScanRegion(screenshot, screenshotHeight, engine, saveToDisk);
            }
        }

        public class NumberScreenScanner : ScreenScanner
        {
            public NumberScreenScanner(Responsive.Measurement regionOfInterest,
                Func<string, string[]>? split = null,
                ImagePreprocessor? preprocessor = null,
                PageSegMode segMode = PageSegMode.SingleBlock) : base(regionOfInterest, split, preprocessor, segMode) {
                SetVariables(engine => {
                    engine.SetVariable("tessedit_char_whitelist", "0123456789-");
                    engine.SetVariable("classify_bln_numeric_mode", 1);
                    engine.SetVariable("debug", 0);
                });
            }

            protected override TesseractEngine CreateEngine() {
                return CreateTesseractEngine("digits", EngineMode.Default);
            }
        }

        public class PositiveNumberScreenScanner : ScreenScanner
        {
            public PositiveNumberScreenScanner(Responsive.Measurement regionOfInterest,
                Func<string, string[]>? split = null,
                ImagePreprocessor? preprocessor = null,
                PageSegMode segMode = PageSegMode.SingleBlock) : base(regionOfInterest, split, preprocessor, segMode) {
                SetVariables(engine => { 
                    engine.SetVariable("tessedit_char_whitelist", "0123456789");
                    engine.SetVariable("classify_bln_numeric_mode", 1);
                    engine.SetVariable("debug", 0);
                });
            }
            
            protected override TesseractEngine CreateEngine() {
                return CreateTesseractEngine("digits", EngineMode.Default);
            }
        }

        public class SinkScanner : ScreenScanner
        {
            public SinkScanner(Responsive.Measurement regionOfInterest,
                Func<string, string[]>? split = null,
                ImagePreprocessor? preprocessor = null,
                PageSegMode segMode = PageSegMode.SingleWord) : base(regionOfInterest, split, preprocessor, segMode) {
                SetVariables(engine => { 
                    engine.SetVariable("tessedit_char_whitelist", "0123456789.,");
                    engine.SetVariable("debug", "0");
                    //engine.SetVariable("classify_bln_numeric_mode", 1);
                });
            }
            
            protected override TesseractEngine CreateEngine() {
                return CreateTesseractEngine("digits", EngineMode.Default);
            }
        }

        public class KamasScanner : ScreenScanner
        {
            public KamasScanner(Responsive.Measurement regionOfInterest,
                Func<string, string[]>? split = null,
                ImagePreprocessor? preprocessor = null,
                PageSegMode segMode = PageSegMode.SingleBlock) : base(regionOfInterest, split, preprocessor, segMode) {
                SetVariables(engine => { engine.SetVariable("tessedit_char_whitelist", "01234567890k,"); });
            }
        }
    }
}
