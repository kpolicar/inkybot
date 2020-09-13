using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
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
        public string stat;
        public string max;
        public string min;

        public StatLineScanResult(string min, string max, string stat) {
            this.min = min;
            this.max = max;
            this.stat = stat;
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
                EngineMode.Default);
            
            screen = (ScreenCapture) Program.Services.GetService(typeof(ScreenCapture));
            init = true;
        }
        
        ~DofusScreenScan() {
            stream.Dispose();
        }

        public async Task<StatLineScanResult[]> Stats() {
            var Results = new List<StatLineScanResult>();
            var (x, y) = (626, 302);

            bool isResultValid = true;
            for (int yOffset = 0; isResultValid; yOffset += 39) {
                var result = await ScanLine(new Rectangle(x, y+yOffset, 980-x, 39), "stats"+yOffset);
                var separated = result.Split(new[] {' '}, 3);
                
                isResultValid = separated.Length == 3;
                if (!isResultValid) continue;
                
                var (min, max, stat) = (separated[0], separated[1], separated[2]);
                Results.Add(new StatLineScanResult(min, max, stat));
            }

            return Results.ToArray();
        }

        private async Task<string> ScanLine(Rectangle bounds, string name) {
            string scannedLine = "";
            var bitmap = screen.cropAtRect(screenshot, bounds);
            bitmap = screen.ResizeImage(bitmap, bitmap.Width*2, bitmap.Height*2);
            
            
            var fstream = File.Create("A:/Desktop/"+name+".bmp");
            bitmap.Save(fstream, ImageFormat.Bmp);
            fstream.Dispose();
            
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