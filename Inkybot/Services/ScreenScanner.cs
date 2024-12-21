using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Inkybot.Events;
using Inkybot.Exceptions;
using Inkybot.Helpers;
using Tesseract;
using Debug = System.Diagnostics.Debug;

namespace Inkybot.Services
{
    public partial class ScreenReaderDataProvider
    {
        public class ScreenScanner : IDisposable
        {
            public event EventHandler<TesseractPageProcessed>? PageProcessed;
            public event EventHandler<FileSystemEventArgs>? Saved;

            private TesseractEngine engine;
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
                engine = CreateEngine();
                this.preprocessor = preprocessor ?? new ImagePreprocessor();
                this.regionOfInterest = regionOfInterest;
                this.split = split;
                this.segMode = segMode;
            }

            protected virtual TesseractEngine CreateEngine() {
                return new TesseractEngine(
                    Path.Combine(AppContext.BaseDirectory, @"Resources\Tesseract"),
                    CultureInfo.CurrentUICulture.ThreeLetterISOLanguageName,
                    EngineMode.Default);
            }

            public void SetVariables(Action<TesseractEngine> callback) {
                callback(engine);
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
                return Task.Run(() => ScanRegion(screenshot, screenshotHeight, saveToDisk));
            }
            
            public double ratioFromOptimalScreenshotHeight(int screenshotHeight) =>
                (1d*optimalScreenshotHeight)/(1d*screenshotHeight);
            private readonly int optimalScreenshotHeight = 1080; //1920x1080

            public string[] ScanRegion(Image screenshot, int screenshotHeight, bool saveToDisk = false) {
                var bounds = CalculateBounds(screenshot);

                var image = preprocessor is ResizeImagePreprocessor resizeImagePreprocessor
                    ? (Bitmap) resizeImagePreprocessor.PreprocessImage(screenshot, bounds, ratioFromOptimalScreenshotHeight(screenshotHeight))
                    : (Bitmap) preprocessor.PreprocessImage(screenshot, bounds);

                if (saveToDisk) {
                    var folderPath = Path.Combine(AppContext.BaseDirectory, @"debug\images");
                    Directory.CreateDirectory(folderPath);
                    var fileName = Path.GetRandomFileName() + ".bmp";

                    PixConverter.ToPix(image).Save(folderPath + "/" + fileName);
                    Saved?.Invoke(this, new FileSystemEventArgs(
                        WatcherChangeTypes.Created, folderPath, fileName));
                }

                using (var ocrPage = ProcessImage(engine, image)) {
                    var scanned = ocrPage.GetText();
                    PageProcessed?.Invoke(this, new TesseractPageProcessed(image, ocrPage, scanned));

                    Debug.WriteLine(scanned);
                    var textLines = split?.Invoke(scanned) ?? new[] {scanned};

                    return textLines
                        .Select(text => text.Replace("\n", " "))
                        .ToArray();
                }
            }

            private Page ProcessImage(TesseractEngine engine, Bitmap image) {
                try {
                    return engine.Process(image, segMode);
                } catch (InvalidOperationException exception) {
                    Console.WriteLine(exception.Message);
                    throw new OcrEngineNotReadyYetException("OCR engine is unavailable, try again in a moment.",
                        exception);
                }
            }

            public void Dispose() {
                engine?.Dispose();
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
                    engine.SetVariable("tessedit_enable_dict_correction", 1);
                    engine.SetVariable("language_model_penalty_non_freq_dict_word", 1);
                    engine.SetVariable("language_model_penalty_non_dict_word", 1);
                });
            }
        }

        public class NumberScreenScanner : ScreenScanner
        {
            public NumberScreenScanner(Responsive.Measurement regionOfInterest,
                Func<string, string[]>? split = null,
                ImagePreprocessor? preprocessor = null,
                PageSegMode segMode = PageSegMode.SingleBlock) : base(regionOfInterest, split, preprocessor, segMode) {
                SetVariables(engine => {
                    engine.SetVariable("tessedit_char_whitelist", "0123456789-%");
                    engine.SetVariable("classify_bln_numeric_mode", 1);
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
