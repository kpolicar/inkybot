using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using ImageMagick;

namespace Inkybot.Services
{
    public class ImagePreprocessor
    {
        public Image PreprocessImage(Image image, Rectangle bounds) {
            return DoPreprocess(image, bounds, image => {
                image.Alpha(AlphaOption.Remove);
                image.BlackThreshold(new Percentage(27));
                image.Negate();
                image.Resize(new Percentage(130));
            });
        }
        
        private Image PreprocessRunesImage(Image image, Rectangle bounds) {
            return DoPreprocess(image, bounds, image => {
                image.Resize(new Percentage(300));
                image.ColorThreshold(new MagickColor(230, 230, 230), new MagickColor(255, 255, 255));
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

                    steps(newImage);

                    newImage.Write(ms);

                    var outImage = Image.FromStream(ms);

                    return outImage;
                }
            }
        }
    }
}
