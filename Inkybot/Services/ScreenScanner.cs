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
            public MinMaxScreenScanner(Responsive.Measurement regionOfInterest, Func<string, string[]>? split = null, ImagePreprocessor? preprocessor = null, PageSegMode segMode = PageSegMode.SingleBlock)
                : base(regionOfInterest, split, preprocessor, segMode) {
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
                var eng = new TesseractEngine(
                    Path.Combine(AppContext.BaseDirectory, @"Resources\Tesseract"),
                    "eng",
                    EngineMode.TesseractOnly);
                eng.SetVariable("debug", "0");
                return eng;
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

                var slices = magickImage.CropToTiles(magickImage.Width, magickImage.Height/13);

                IEnumerable<string> textLines = new string[] {};

                var i = 0;
                foreach (var slice in slices) {
                    slice.ResetPage();
                    slice.Crop(new MagickGeometry(0, (int)slice.Height/6, slice.Width, slice.Height/2+slice.Height/10), Gravity.North);

                    using var canvas = new MagickImage(MagickColors.White, slice.Width + 150, slice.Height + 150);
                    canvas.Composite(slice, 75, 75, CompositeOperator.Over);

                    var sliceBmp = canvas.ToBitmap();

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
                    i++;
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
            private Responsive.Measurement regionOfInterest;
            private Func<string, string[]>? split;

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
                var eng = new TesseractEngine(
                    Path.Combine(AppContext.BaseDirectory, @"Resources\Tesseract"),
                    CultureInfo.CurrentUICulture.ThreeLetterISOLanguageName,
                    EngineMode.Default);
                eng.SetVariable("debug", "0");
                return eng;
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
                    engine.SetVariable("load_system_dawg", "0");
                    engine.SetVariable("load_freq_dawg", "0");
                    engine.SetVariable("load_unambig_dawg", "0");
                    engine.SetVariable("classify_enable_learning", "0");
                    engine.SetVariable("classify_enable_adaptive_matcher", "0");
                });
            }

            protected override TesseractEngine CreateEngine() {
                var eng = new TesseractEngine(
                    Path.Combine(AppContext.BaseDirectory, @"Resources\Tesseract"),
                    "eng-fine-tuned",
                    EngineMode.Default);
                eng.SetVariable("debug", "0");
                return eng;
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

            protected override TesseractEngine CreateEngine() =>
                new TesseractEngine(
                    Path.Combine(AppContext.BaseDirectory, @"Resources\Tesseract"),
                    "digits",
                    EngineMode.Default);
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
            
            protected override TesseractEngine CreateEngine() =>
                new TesseractEngine(
                    Path.Combine(AppContext.BaseDirectory, @"Resources\Tesseract"),
                    "digits",
                    EngineMode.Default);
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
            
            protected override TesseractEngine CreateEngine() =>
                new TesseractEngine(
                    Path.Combine(AppContext.BaseDirectory, @"Resources\Tesseract"),
                    "digits",
                    EngineMode.Default);
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
