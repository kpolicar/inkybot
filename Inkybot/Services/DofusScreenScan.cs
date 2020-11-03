using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using ImageMagick;
using Inkybot.Events;
using Inkybot.Contracts;
using Inkybot.Exceptions;
using Inkybot.Helpers;
using Tesseract;
using ImageFormat = System.Drawing.Imaging.ImageFormat;

namespace Inkybot
{
    public class DofusScreenScan
    {
        public static readonly Responsive.Measurement HistoryBoundsMeasurement = new Responsive.Measurement {
            Rectangle = Rect.FromCoords(346, 120, 590, 835),
            Width = 1920,
            Height = 1017
        };

        public static readonly Responsive.Measurement StatBoundsMeasurement = new Responsive.Measurement {
            Rectangle = Rect.FromCoords(630, 307, 973, 836),
            Width = 1920,
            Height = 1017
        };
        
        public Rectangle HistoryBounds =>
            Responsive.ResponsiveRectangle(HistoryBoundsMeasurement, screenshot.Width, screenshot.Height);

        public Rectangle StatBounds =>
            Responsive.ResponsiveRectangle(StatBoundsMeasurement, screenshot.Width, screenshot.Height);
        
        private static ScreenCapture screen;
        private static TesseractEngine engine;
        private static bool init;
        private readonly IntPtr handle;
        private readonly Image screenshot;
        private TextInfo text;
        private bool saveToDisk;

        public DofusScreenScan(IntPtr hwnd, bool saveToDisk = false) {
            Init();
            handle = hwnd;
            screenshot = TakeScreenshot();
            this.saveToDisk = saveToDisk;
        }

        private void Init() {
            if (init) return;
            engine = new TesseractEngine(
                "./Resources/Tesseract",
                "eng",
                EngineMode.TesseractOnly,
                null, new Dictionary<string, object> {
                    {"tessedit_char_whitelist", "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789%-,+() "}
                }, false);
            
            screen = (ScreenCapture) Program.Services.GetService(typeof(ScreenCapture));
            init = true;
        }

        public string[] Stats() {
            var scanned = ScanRegion(StatBounds, "\n");
            Debug.WriteLine(string.Join("\n", scanned));

            return scanned;
        }

        public string[] History() {
            var scanned = ScanRegion(HistoryBounds, "\n\n");

            return scanned;
        }

        private Image PreprocessRunesImage(Image image, Rectangle bounds) {
            return DoPreprocess(image, bounds, image => {
                image.Resize(new Percentage(300));
                image.ColorThreshold(new MagickColor(230, 230, 230), new MagickColor(255, 255, 255));
            });
        }

        private Image PreprocessImage(Image image, Rectangle bounds) {
            return DoPreprocess(image, bounds, image => {
                image.ColorSpace = ColorSpace.Gray;

                image.Sharpen();
                image.BlackThreshold(new Percentage(30));
                image.WhiteThreshold(new Percentage(30));
            });
        }

        private Image DoPreprocess(Image image, Rectangle bounds, Action<MagickImage> steps) {
            using (var ms = new MemoryStream()) {
                image.Save(ms, ImageFormat.Bmp);
                ms.Position = 0;
                    
                using (var newImage = new MagickImage(ms)) {
                    var b = bounds;
                    
                    // Resize each image in the collection to a width of 200. When zero is specified for the height
                    // the height will be calculated with the aspect ratio.
                    newImage.Crop(new MagickGeometry(b.X, b.Y, b.Width, b.Height));
                    newImage.Resize(new Percentage(300));

                    steps(newImage);
                    
                    newImage.Write(ms);
                    if (saveToDisk) {
                        var folderPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)+@"/debug/images/";
                        Directory.CreateDirectory(folderPath);
                        newImage.Write(folderPath+Path.GetRandomFileName()+".png");
                    }
                    
                    var outImage = Image.FromStream(ms);

                    return outImage;
                }
            }

        }

        private string[] ScanRegion(Rectangle bounds, string delimiter) {
            var image = PreprocessImage(screenshot, bounds);

            //var fstream = File.Create(@"C:\Users\Klemen\Desktop\" + name + ".bmp");
            //bitmap.Save(fstream, ImageFormat.Bmp);
            //fstream.Dispose();
            

            using (var ocrResult = ProcessImage((Bitmap) image, PageSegMode.SingleBlock)) {
                return ocrResult.GetText().Split(new[] { delimiter }, StringSplitOptions.RemoveEmptyEntries);
                var results = Regex
                    .Split(ocrResult.GetText(), "(?<!(?:[,+-] ?[0-9]*))(?:\\n)+(?=(?:[-+]?(?:[0-9]|sink)))")
                    .Select(result => result.Replace("\n", " "));

                ocrResult.Dispose();

                return results.ToArray();
            }
        }

        private Page ProcessImage(Bitmap image, PageSegMode? pageSegMode = null) {
            try {
                return engine.Process(image, pageSegMode);
            } catch (InvalidOperationException exception) {
                Console.WriteLine(exception.Message);
                throw new OcrEngineNotReadyYetException("OCR engine is unavailable, try again in a moment.", exception);
            }
        }

        public Image TakeScreenshot() {
            //var bitmap = Image.FromFile(@"C:\Users\Klemen\Desktop\ex.bmp");
            var bitmap = screen.CaptureWindow(handle);

            //var fstream = File.Create(@"C:\Users\Klemen\Desktop\example.bmp");
            //bitmap.Save(fstream, ImageFormat.Bmp);
            //fstream.Dispose();

            return bitmap;
        }
    }
}
