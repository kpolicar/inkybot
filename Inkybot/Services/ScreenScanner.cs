using System;
using System.Diagnostics;
using System.Drawing;
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
using ImageFormat = System.Drawing.Imaging.ImageFormat;

namespace Inkybot.Services
{
    public class ScreenScanner
    {
        public event EventHandler<TesseractPageProcessed> PageProcessed;

        private TesseractEngine engine;
        private Responsive.Measurement regionOfInterest;
        private Func<string, string[]> split;
        private ImagePreprocessor preprocessor;
        private PageSegMode segMode = PageSegMode.SingleBlock;

        public ScreenScanner(Responsive.Measurement regionOfInterest, Func<string, string[]> split, ImagePreprocessor preprocessor = null) {
            engine = new TesseractEngine(
                "./Resources/Tesseract",
                Program.Lang.ThreeLetterISOLanguageName,
                EngineMode.Default);
            this.preprocessor = preprocessor ?? new ImagePreprocessor();
            this.regionOfInterest = regionOfInterest;
            this.split = split;
        }
        
        public void SetVariables(Action<TesseractEngine> callback) {
            callback(engine);
        }
        
        public Rectangle CalculateBounds(Image image) {
            return Responsive.ResponsiveRectangle(regionOfInterest, image.Width, image.Height);
        }
        public async Task<string[]> ScanRegionAsync(Image screenshot, bool saveToDisk = false) {
            var result = new string[] { };
            var task = Task.Run(() => {
                result = ScanRegion(screenshot, saveToDisk);
            });
            task.Wait();

            return result;
        }

        public string[] ScanRegion(Image screenshot, bool saveToDisk = false) {
            var bounds = CalculateBounds(screenshot);
            
            var image = (Bitmap) preprocessor.PreprocessImage(screenshot, bounds);

            if (saveToDisk) {
                var folderPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"/debug/images/";
                Directory.CreateDirectory(folderPath);
                var fileName = Path.GetRandomFileName() + ".bmp";
                
                PixConverter.ToPix(image).Save(folderPath+"/"+fileName);
            }

            using (var ocrPage = ProcessImage(engine, image)) {
                PageProcessed?.Invoke(this, new TesseractPageProcessed(image, ocrPage));
                
                var scanned = ocrPage.GetText();
                Trace.WriteLine(Regex.Escape(scanned));
                var textLines = split(scanned);

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
                throw new OcrEngineNotReadyYetException("OCR engine is unavailable, try again in a moment.", exception);
            }
        }
    }
    
    public class TextScreenScanner : ScreenScanner
    {
        public TextScreenScanner(Responsive.Measurement regionOfInterest,
            Func<string, string[]> split,
            ImagePreprocessor preprocessor = null) : base(regionOfInterest, split, preprocessor) {
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
            Func<string, string[]> split,
            ImagePreprocessor preprocessor = null) : base(regionOfInterest, split, preprocessor) {
            SetVariables(engine => {
                engine.SetVariable("tessedit_char_whitelist", "01234567890-");
            });
        }
    }
}
