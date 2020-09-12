using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Globalization;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;
using Windows.Storage.Streams;

namespace WindowsFormsApp
{
    public class Win32ScreenReader
    {
        ScreenCapture screen;
        InMemoryRandomAccessStream stream;
        readonly Language language = new Language("en");
        OcrEngine engine;
        IntPtr handle;

        public Win32ScreenReader(IntPtr hwnd) {
            if (!OcrEngine.IsLanguageSupported(language)) {
                throw new Exception($"{language.LanguageTag} is not supported in this system.");
            }
            
            engine = OcrEngine.TryCreateFromLanguage(language);
            screen = (ScreenCapture) Program.Services.GetService(typeof(ScreenCapture));
            stream = new InMemoryRandomAccessStream();
            handle = hwnd;
        }
        
        ~Win32ScreenReader() {
            stream.Dispose();
        }
        
        public async Task<Dictionary<string, string>> Stats() {
            var stats = new Dictionary<string, string>();
            
            var result = Program.debug ?
                await Scan(new Rectangle(745, 305, 1050-745, 760-305)) :
                await Scan(new Rectangle(740, 305, 1044-740, 840-305));
            foreach (var line in result.Lines) {
                var data = line.Text.Split(new [] { ' ' }, 2);
                stats[data[1]] = data[0];
            }
            
            return stats;
        }

        public async Task<Dictionary<string, string>> Max() {
            throw new NotImplementedException();
        }

        public async Task<Dictionary<string, string>> Min() {
            throw new NotImplementedException();
        }

        public async Task<OcrResult> Scan(Rectangle bounds) {
            var bitmap = Program.debug ? Image.FromFile("A:/Desktop/ex.jpg") : (Bitmap) screen.CaptureWindow(handle);
            bitmap = screen.cropAtRect((Bitmap) bitmap, bounds);

            var fstream = File.Create("A:/Desktop/example.bmp");
            bitmap.Save(fstream, ImageFormat.Bmp);//choose the specific image format by your own bitmap source
            fstream.Dispose();
            
            bitmap.Save(stream.AsStream(), ImageFormat.Bmp);
            var decoder = await BitmapDecoder.CreateAsync(stream);
            var softwareBitmap = await decoder.GetSoftwareBitmapAsync();

            var ocrResult = await engine.RecognizeAsync(softwareBitmap).AsTask();
            return ocrResult;
        }
    }
}