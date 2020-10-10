using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using WindowsFormsApp.Contracts;

namespace WindowsFormsApp
{
    /// <summary>
    ///     Provides functions to capture the entire screen, or a particular window, and save it to a file.
    /// </summary>
    public class Win32ScreenCapture : ScreenCapture
    {
        public Bitmap cropAtRect(Bitmap b, Rectangle r) {
            var nb = new Bitmap(r.Width, r.Height);
            using (var g = Graphics.FromImage(nb)) {
                g.DrawImage(b, -r.X, -r.Y);
                return nb;
            }
        }

        public Bitmap ResizeImage(Image image, int width, int height) {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage)) {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes()) {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        /// <summary>
        ///     Creates an Image object containing a screen shot of a specific window
        /// </summary>
        /// <param name="handle">The handle to the window. (In windows forms, this is obtained by the Handle property)</param>
        /// <returns></returns>
        public Image CaptureWindow(IntPtr handle) {
            // get te hDC of the target window
            var hdcSrc = User32.GetWindowDC(handle);
            // get the size
            var windowRect = new User32.RECT();
            User32.GetWindowRect(handle, ref windowRect);
            var width = windowRect.right - windowRect.left;
            var height = windowRect.bottom - windowRect.top;
            // create a device context we can copy to
            var hdcDest = GDI32.CreateCompatibleDC(hdcSrc);
            // create a bitmap we can copy it to,
            // using GetDeviceCaps to get the width/height
            var hBitmap = GDI32.CreateCompatibleBitmap(hdcSrc, width, height);
            // select the bitmap object
            var hOld = GDI32.SelectObject(hdcDest, hBitmap);
            // bitblt over
            GDI32.BitBlt(hdcDest, 0, 0, width, height, hdcSrc, 0, 0, GDI32.SRCCOPY);
            // restore selection
            GDI32.SelectObject(hdcDest, hOld);
            // clean up
            GDI32.DeleteDC(hdcDest);
            User32.ReleaseDC(handle, hdcSrc);
            // get a .NET image object for it
            Image img = Image.FromHbitmap(hBitmap);
            // free up the Bitmap object
            GDI32.DeleteObject(hBitmap);
            return img;
        }

        public Bitmap Sharpen(Bitmap image) {
            var sharpenImage = (Bitmap) image.Clone();

            var filterWidth = 3;
            var filterHeight = 3;
            var width = image.Width;
            var height = image.Height;

            // Create sharpening filter.
            var filter = new double[filterWidth, filterHeight];
            filter[0, 0] = filter[0, 1] =
                filter[0, 2] = filter[1, 0] = filter[1, 2] = filter[2, 0] = filter[2, 1] = filter[2, 2] = -1;
            filter[1, 1] = 9;

            var factor = 1.0;
            var bias = 0.0;

            var result = new Color[image.Width, image.Height];

            // Lock image bits for read/write.
            var pbits = sharpenImage.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite,
                PixelFormat.Format24bppRgb);

            // Declare an array to hold the bytes of the bitmap.
            var bytes = pbits.Stride * height;
            var rgbValues = new byte[bytes];

            // Copy the RGB values into the array.
            Marshal.Copy(pbits.Scan0, rgbValues, 0, bytes);

            int rgb;
            // Fill the color array with the new sharpened color values.
            for (var x = 0; x < width; ++x)
            for (var y = 0; y < height; ++y) {
                double red = 0.0, green = 0.0, blue = 0.0;

                for (var filterX = 0; filterX < filterWidth; filterX++) {
                    for (var filterY = 0; filterY < filterHeight; filterY++) {
                        var imageX = (x - filterWidth / 2 + filterX + width) % width;
                        var imageY = (y - filterHeight / 2 + filterY + height) % height;

                        rgb = imageY * pbits.Stride + 3 * imageX;

                        red += rgbValues[rgb + 2] * filter[filterX, filterY];
                        green += rgbValues[rgb + 1] * filter[filterX, filterY];
                        blue += rgbValues[rgb + 0] * filter[filterX, filterY];
                    }

                    var r = Math.Min(Math.Max((int) (factor * red + bias), 0), 255);
                    var g = Math.Min(Math.Max((int) (factor * green + bias), 0), 255);
                    var b = Math.Min(Math.Max((int) (factor * blue + bias), 0), 255);

                    result[x, y] = Color.FromArgb(r, g, b);
                }
            }

            // Update the image with the sharpened pixels.
            for (var x = 0; x < width; ++x)
            for (var y = 0; y < height; ++y) {
                rgb = y * pbits.Stride + 3 * x;

                rgbValues[rgb + 2] = result[x, y].R;
                rgbValues[rgb + 1] = result[x, y].G;
                rgbValues[rgb + 0] = result[x, y].B;
            }

            // Copy the RGB values back to the bitmap.
            Marshal.Copy(rgbValues, 0, pbits.Scan0, bytes);
            // Release image bits.
            sharpenImage.UnlockBits(pbits);

            return sharpenImage;
        }

        /// <summary>
        ///     Creates an Image object containing a screen shot of the entire desktop
        /// </summary>
        /// <returns></returns>
        public Image CaptureScreen() {
            return CaptureWindow(User32.GetDesktopWindow());
        }

        /// <summary>
        ///     Captures a screen shot of a specific window, and saves it to a file
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="filename"></param>
        /// <param name="format"></param>
        public void CaptureWindowToFile(IntPtr handle, string filename, ImageFormat format) {
            var img = CaptureWindow(handle);
            img.Save(filename, format);
        }

        /// <summary>
        ///     Captures a screen shot of the entire desktop, and saves it to a file
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="format"></param>
        public void CaptureScreenToFile(string filename, ImageFormat format) {
            var img = CaptureScreen();
            img.Save(filename, format);
        }

        /// <summary>
        ///     Helper class containing Gdi32 API functions
        /// </summary>
        private class GDI32
        {
            public const int SRCCOPY = 0x00CC0020; // BitBlt dwRop parameter

            [DllImport("gdi32.dll")]
            public static extern bool BitBlt(IntPtr hObject, int nXDest, int nYDest,
                int nWidth, int nHeight, IntPtr hObjectSource,
                int nXSrc, int nYSrc, int dwRop);

            [DllImport("gdi32.dll")]
            public static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int nWidth,
                int nHeight);

            [DllImport("gdi32.dll")]
            public static extern IntPtr CreateCompatibleDC(IntPtr hDC);

            [DllImport("gdi32.dll")]
            public static extern bool DeleteDC(IntPtr hDC);

            [DllImport("gdi32.dll")]
            public static extern bool DeleteObject(IntPtr hObject);

            [DllImport("gdi32.dll")]
            public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);
        }

        /// <summary>
        ///     Helper class containing User32 API functions
        /// </summary>
        private class User32
        {
            [DllImport("user32.dll")]
            public static extern IntPtr GetDesktopWindow();

            [DllImport("user32.dll")]
            public static extern IntPtr GetWindowDC(IntPtr hWnd);

            [DllImport("user32.dll")]
            public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDC);

            [DllImport("user32.dll")]
            public static extern IntPtr GetWindowRect(IntPtr hWnd, ref RECT rect);

            [StructLayout(LayoutKind.Sequential)]
            public struct RECT
            {
                public readonly int left;
                public readonly int top;
                public readonly int right;
                public readonly int bottom;
            }
        }
    }
}
