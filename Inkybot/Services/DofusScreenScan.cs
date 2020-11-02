using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Inkybot.Events;
using Inkybot.Contracts;
using Inkybot.Exceptions;
using Tesseract;
using ImageFormat = System.Drawing.Imaging.ImageFormat;

namespace Inkybot
{
    public struct StatLineScanResult
    {
        public string name;
        public string value;
        public string min;
        public string max;

        public StatLineScanResult(string name, string value, string min, string max) {
            this.name = name;
            this.value = value;
            this.min = min;
            this.max = max;
        }
    }

    public class DofusScreenScan
    {
        public static readonly Rectangle HistoryBounds = new Rectangle(352, 137, 590-352, 835-137);
        public static readonly Rectangle StatBounds = new Rectangle(645, 300, 980-645, 39*12);
        
        private static ScreenCapture screen;
        private static TesseractEngine engine;
        private static bool init;
        private readonly IntPtr handle;
        private readonly Bitmap screenshot;
        private TextInfo text;

        public DofusScreenScan(IntPtr hwnd) {
            Init();
            handle = hwnd;
            screenshot = TakeScreenshot();
        }

        private void Init() {
            if (init) return;
            engine = new TesseractEngine(
                "./tessdata",
                "eng",
                EngineMode.TesseractOnly,
                null, new Dictionary<string, object> {
                    {"tessedit_char_whitelist", "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789%-,+() "}
                }, false);
            
            screen = (ScreenCapture) Program.Services.GetService(typeof(ScreenCapture));
            init = true;
        }

        public StatLineScanResult[] Stats() {
            var Results = new List<StatLineScanResult>();
            var (x, y) = (645, 301);

            var isResultValid = true;
            for (var yOffset = 0; isResultValid; yOffset += 39) {
                var result = ScanLine(new Rectangle(x, y + yOffset+5, 980 - x, 39-10), "stats" + yOffset);
                
                var separated = Regex.Match(result, @"^(\d+) (\d+) (\d*) ?(%? ?[A-z ]+)$").Groups;

                isResultValid = separated.Count == 5;
                if (!isResultValid) continue;

                var (min, max, value, name) =
                    (separated[1].Value, separated[2].Value, separated[3].Value, separated[4].Value);
                Results.Add(new StatLineScanResult(name, value, min, max));
            }

            return Results.ToArray();
        }

        public string[] History() {
            var scanned = ScanRegion(HistoryBounds, "history");

            return scanned;
        }

        private string[] ScanRegion(Rectangle bounds, string name) {
            var bitmap = screen.cropAtRect(screenshot, bounds);
            // Todo: add minimum rect
            bitmap = screen.ResizeImage(bitmap, bitmap.Width * 2, bitmap.Height * 2);
            bitmap = screen.Sharpen(bitmap);

            //var fstream = File.Create(@"C:\Users\Klemen\Desktop\" + name + ".bmp");
            //bitmap.Save(fstream, ImageFormat.Bmp);
            //fstream.Dispose();

            var ocrResult = ProcessImage(bitmap, PageSegMode.SingleBlock);

            var results = Regex
                .Split(ocrResult.GetText(), "(?<!(?:[,+-] ?[0-9]*))(?:\\n)+(?=(?:[-+]?(?:[0-9]|sink)))")
                .Select(result => result.Replace("\n", " "));

            ocrResult.Dispose();

            return results.ToArray();
        }

        private Page ProcessImage(Bitmap image, PageSegMode? pageSegMode = null) {
            try {
                return engine.Process(image, pageSegMode);
            } catch (InvalidOperationException exception) {
                Console.WriteLine(exception.Message);
                throw new OcrEngineNotReadyYetException("OCR engine is unavailable, try again in a moment.", exception);
            }
        }

        private string ScanLine(Rectangle bounds, string name) {
            var scannedLine = "";
            var bitmap = screen.cropAtRect(screenshot, bounds);
            bitmap = screen.ResizeImage(bitmap, bitmap.Width * 2, bitmap.Height * 2);


            //var fstream = File.Create(@"C:\Users\Klemen\Desktop\"+name+".bmp");
            //bitmap.Save(fstream, ImageFormat.Bmp);
            //fstream.Dispose();

            var ocrResult = ProcessImage(bitmap, PageSegMode.SingleBlock);

            using (var iter = ocrResult.GetIterator()) {
                iter.Begin();

                var text = "" + iter.GetText(PageIteratorLevel.TextLine);
                ocrResult.Dispose();
                scannedLine = Regex.Replace(text, @"\t|\n|\r", "");
            }

            return scannedLine;
        }

        public Bitmap TakeScreenshot() {
            //var bitmap = Image.FromFile(@"C:\Users\Klemen\Desktop\ex.bmp");
            var bitmap = (Bitmap) screen.CaptureWindow(handle);

            //var fstream = File.Create(@"C:\Users\Klemen\Desktop\example.bmp");
            //bitmap.Save(fstream, ImageFormat.Bmp);
            //fstream.Dispose();

            return (Bitmap) bitmap;
        }
    }
}
