using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Globalization;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;
using Windows.Storage.Streams;
using Tesseract;
using ImageFormat = System.Drawing.Imaging.ImageFormat;
using ScreenCapture = WindowsFormsApp.Contracts.ScreenCapture;

namespace WindowsFormsApp
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
        private static ScreenCapture screen;
        private static InMemoryRandomAccessStream stream;
        private static TesseractEngine engine;
        private static bool init = false;
        private readonly IntPtr handle;
        private readonly Bitmap screenshot;

        public DofusScreenScan(IntPtr hwnd) {
            Init();
            handle = hwnd;
            stream = new InMemoryRandomAccessStream();
            screenshot = TakeScreenshot();
        }

        private void Init() {
            if (init) return;
            engine = new TesseractEngine(
                "C:\\Users\\Klemen\\RiderProjects\\WindowsFormsApp\\WindowsFormsApp\\tessdata", 
                "eng",
                EngineMode.Default,
                "C:\\Users\\Klemen\\RiderProjects\\WindowsFormsApp\\WindowsFormsApp\\tessdata\\config\\config");
            
            screen = (ScreenCapture) Program.Services.GetService(typeof(ScreenCapture));
            init = true;
        }
        
        ~DofusScreenScan() {
            stream.Dispose();
        }

        public StatLineScanResult[] Stats() {
            var Results = new List<StatLineScanResult>();
            var (x, y) = (626, 300);

            bool isResultValid = true;
            for (int yOffset = 0; isResultValid; yOffset += 39) {
                var result = ScanLine(new Rectangle(x, y+yOffset, 980-x, 39), "stats"+yOffset);

                var separated = Regex.Match(result, @"(\d+) (\d+) (\d*) ?(%? ?[A-z ]+)").Groups;

                isResultValid = separated.Count == 5;
                if (!isResultValid) continue;
                
                var (min, max, value, name) =
                    (separated[1].Value, separated[2].Value, separated[3].Value, separated[4].Value);
                Results.Add(new StatLineScanResult(name, value, min, max));
            }

            return Results.ToArray();
        }

        public string[] History() {
            var (x, y) = (352, 137);
            var (xMax, yMax) = (590, 835);

            var scanned = ScanRegion(new Rectangle(x, y, xMax-x, yMax-y), "history");
            
            Debug.WriteLine("-------read------");
            foreach (var s in scanned) {
                Debug.WriteLine(s);
            }
            Debug.WriteLine("-------------");

            return scanned;
        }

        private string[] ScanRegion(Rectangle bounds, string name) {
            var bitmap = screen.cropAtRect(screenshot, bounds);
            bitmap = screen.ResizeImage(bitmap, bitmap.Width*2, bitmap.Height*2);
            // bitmap = Sharpen(bitmap);
            
            
            // var fstream = File.Create("A:/Desktop/"+name+".bmp");
            // bitmap.Save(fstream, ImageFormat.Bmp);
            // fstream.Dispose();
            
            var ocrResult = engine.Process(bitmap, PageSegMode.SingleBlock);
            
            var results = Regex
                .Split(ocrResult.GetText(), "\n\n")
                .Select(result => result.Replace("\n", " "));
            
            ocrResult.Dispose();

            return results.ToArray();
        }
        
        private string ScanLine(Rectangle bounds, string name) {
            string scannedLine = "";
            var bitmap = screen.cropAtRect(screenshot, bounds);
            bitmap = screen.ResizeImage(bitmap, bitmap.Width*2, bitmap.Height*2);
            
            
            // var fstream = File.Create("A:/Desktop/"+name+".bmp");
            // bitmap.Save(fstream, ImageFormat.Bmp);
            // fstream.Dispose();
            
            var ocrResult = engine.Process(bitmap, PageSegMode.SingleLine);
            
            using (var iter = ocrResult.GetIterator()) {
                iter.Begin();

                var text = iter.GetText(PageIteratorLevel.TextLine);
                scannedLine = Regex.Replace(text, @"\t|\n|\r", "");
            }
            ocrResult.Dispose();

            return scannedLine;
        }
        
        public Bitmap TakeScreenshot() {
            var bitmap = Program.debug ? Image.FromFile("A:/Desktop/ex.bmp") : (Bitmap) screen.CaptureWindow(handle);

            var fstream = File.Create("A:/Desktop/example.bmp");
            bitmap.Save(fstream, ImageFormat.Bmp);
            fstream.Dispose();
            
            return (Bitmap) bitmap;
        }
    }
}