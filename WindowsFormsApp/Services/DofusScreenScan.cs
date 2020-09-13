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
using ScreenCapture = WindowsFormsApp.Contracts.ScreenCapture;

namespace WindowsFormsApp
{
    public class DofusScreenScan
    {
        private static ScreenCapture screen;
        private static InMemoryRandomAccessStream stream;
        private static readonly Language language = new Language("en");
        private static OcrEngine engine;
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
            
            if (!OcrEngine.IsLanguageSupported(language)) {
                throw new Exception($"{language.LanguageTag} is not supported in this system.");
            }
            
            engine = OcrEngine.TryCreateFromLanguage(language);
            screen = (ScreenCapture) Program.Services.GetService(typeof(ScreenCapture));
            init = true;
        }
        
        ~DofusScreenScan() {
            stream.Dispose();
        }

        public async Task<(string, string)[]> Stats() {
            var result = await Scan(new Rectangle(740, 305, 1044-740, 840-305));

            return result.Lines.Select(
                line => {
                    var value = Regex.Match(line.Text, @"-?\d+").Value;
                    var name = Regex.Replace(line.Text, @"-?\d+ ?", "");
                    return (name, value);
                }
            ).ToArray();
        }

        public async Task<string[]> Max() {
            var result = await Scan(new Rectangle(640, 305, 690-640, 840-305));
            return result.Lines.Select(line => line.Text).ToArray();
        }

        public async Task<string[]> Min() {
            var result = await Scan(new Rectangle(690, 305, 740-690, 840-305));
            return result.Lines.Select(line => line.Text).ToArray();
        }

        private async Task<OcrResult> Scan(Rectangle bounds) {
            var bitmap = screen.cropAtRect(screenshot, bounds);
            bitmap.Save(stream.AsStream(), ImageFormat.Bmp);
            
            var decoder = await BitmapDecoder.CreateAsync(stream);
            var softwareBitmap = await decoder.GetSoftwareBitmapAsync();

            var ocrResult = await engine.RecognizeAsync(softwareBitmap).AsTask();
            return ocrResult;
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