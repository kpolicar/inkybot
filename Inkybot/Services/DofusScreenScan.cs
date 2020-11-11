using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ImageMagick;
using Inkybot.Events;
using Inkybot.Contracts;
using Inkybot.Exceptions;
using Inkybot.Helpers;
using Tesseract;
using Debug = System.Diagnostics.Debug;
using ImageFormat = System.Drawing.Imaging.ImageFormat;

namespace Inkybot
{
    public class DofusScreenScan
    {
        public static readonly Responsive.Measurement HistoryBoundsMeasurement = new Responsive.Measurement {
            Rectangle = Rect.FromCoords(346, 117, 590, 835),
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
        private bool saveToDisk;

        public DofusScreenScan(IntPtr hwnd, bool saveToDisk = false) {
            Init();
            handle = hwnd;
            screenshot = TakeScreenshot();
            this.saveToDisk = saveToDisk;
            
            if (saveToDisk) {
                var folderPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)+@"/debug/images/";
                Directory.CreateDirectory(folderPath);
                screenshot.Save(folderPath+Path.GetRandomFileName()+".png");
            }
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
            var scanned = ScanRegion(StatBounds,
            text => {
                return text.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            });
            Debug.WriteLine(string.Join("\n", scanned));

            return scanned;
        }

        public string[] History() {
            var scanned = ScanRegion(
                HistoryBounds,
                text => {
                    return Regex.Split(text, "(?<!(?:[,+-] ?[0-9]*(?:\\n)*))(?:\\n)+(?=(?:[-+]?(?:[0-9]|sink|Failure)))")
                        .Where(s => s != string.Empty)
                        .Select(result => result.Replace("\n", " "))
                        .ToArray();
                });

            foreach (var s in scanned) {
                Debug.WriteLine(Regex.Escape(s));
            }

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

                image.Sharpen();
                image.Alpha(AlphaOption.Remove);
                image.BlackThreshold(new Percentage(30));
                image.WhiteThreshold(new Percentage(35));
                image.Negate();
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
                    newImage.ColorSpace = ColorSpace.Gray;
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

        private string[] ScanRegion(Rectangle bounds, Func<string, string[]> split) {
            var image = PreprocessImage(screenshot, bounds);

            //var fstream = File.Create(@"C:\Users\Klemen\Desktop\" + name + ".bmp");
            //bitmap.Save(fstream, ImageFormat.Bmp);
            //fstream.Dispose();
            
            using (var ocrPage = ProcessImage((Bitmap) image, PageSegMode.SingleBlock)) {

                var scanned = ocrPage.GetText();
                Debug.WriteLine(Regex.Escape(scanned));
                var textLines = split(scanned);

                return textLines
                    .Select(text => text.Replace("\n", " "))
                    .ToArray();
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

        //private static int times = 0;

        public Image TakeScreenshot() {

            //times = times >= 3 ? times : ++times;
            //var bitmapp = Image.FromFile(@"C:\Users\Klemen\Desktop\debug.png"); 
            //return bitmapp;
            var bitmap = screen.CaptureWindow(handle);

            //var fstream = File.Create(@"C:\Users\Klemen\Desktop\example.bmp");
            //bitmap.Save(fstream, ImageFormat.Bmp);
            //fstream.Dispose();

            return bitmap;
        }
    }
}
