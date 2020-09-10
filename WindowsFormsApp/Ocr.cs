using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using Windows.Globalization;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;
using Windows.Storage.Streams;

namespace WindowsFormsApp
{
    public class Ocr
    {
        ScreenCapture screen;
        InMemoryRandomAccessStream stream;
        readonly Language language = new Language("en");
        OcrEngine engine;
        IntPtr handle;

        public Ocr(IntPtr hwnd) {
            if (!OcrEngine.IsLanguageSupported(language))
            {
                throw new Exception($"{language.LanguageTag} is not supported in this system.");
            }
            
            engine = OcrEngine.TryCreateFromLanguage(language);
            screen = new ScreenCapture();
            stream = new InMemoryRandomAccessStream();
            handle = hwnd;
        }
        
        ~Ocr() {
            stream.Dispose();
        }

        public async Task<OcrResult> stats()
        {
            return await ocr(new Rectangle(745, 305, 1050-745, 760-305));
            //return await ocr(new Rectangle(740, 305, 1044-740, 840-305));
        }

        public async Task<OcrResult> ocr(Rectangle bounds) {
            //var bitmap = (Bitmap) screen.CaptureWindow(handle);
            var bitmap = Bitmap.FromFile("A:/Desktop/ex.jpg");
            bitmap = ScreenCapture.cropAtRect((Bitmap)bitmap, bounds);

            var fstream = File.Create("A:/Desktop/example.jpg");
            bitmap.Save(fstream, ImageFormat.Jpeg);//choose the specific image format by your own bitmap source
            fstream.Dispose();
            
            bitmap.Save(stream.AsStream(), ImageFormat.Bmp);
            var decoder = await BitmapDecoder.CreateAsync(stream);
            var softwareBitmap = await decoder.GetSoftwareBitmapAsync();

            var ocrResult = await engine.RecognizeAsync(softwareBitmap).AsTask();
            return ocrResult;
        }
    }
}