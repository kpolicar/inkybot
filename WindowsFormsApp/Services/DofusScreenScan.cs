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

        public async Task<(string, string)[]> Stats() {
            var result = await Scan(new Rectangle(740, 305, 980-740, 840-305), "stats");

            return result.Select(
                line => {
                    var value = Regex.Match(line, @"-?\d+").Value;
                    var name = Regex.Replace(line, @"-?\d+ ?", "");
                    return (name, value);
                }
            ).ToArray();
        }

        public async Task<string[]> Max() {
            var result = await Scan(new Rectangle(690, 305, 740-690, 840-305), "max");
            return result;
        }

        public async Task<string[]> Min() {
            var result = await Scan(new Rectangle(640, 305, 690-640, 840-305), "min");
            return result;
        }

        private async Task<string[]> Scan(Rectangle bounds, string name) {
            var lines = new List<string>();
            var bitmap = screen.cropAtRect(screenshot, bounds);
            bitmap = screen.ResizeImage(bitmap, bitmap.Width*2, bitmap.Height*2);
            
            
            var fstream = File.Create("A:/Desktop/"+name+".bmp");
            bitmap.Save(fstream, ImageFormat.Bmp);
            fstream.Dispose();
            
            var ocrResult = engine.Process(bitmap, PageSegMode.SingleBlock);
            
            using (var iter = ocrResult.GetIterator()) {
                iter.Begin();

                do {
                    var text = iter.GetText(PageIteratorLevel.TextLine);
                    var trimmed = Regex.Replace(text, @"\t|\n|\r", "");
                    lines.Add(trimmed);
                } while (iter.Next(PageIteratorLevel.TextLine));
            }
            ocrResult.Dispose();

            return lines.ToArray();
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