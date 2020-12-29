using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using ImageMagick;
using Inkybot.Contracts;

namespace Inkybot.Services
{
    public partial class ScreenReaderDataProvider
    {
        public class RuneImagePreprocessor : ImagePreprocessor
        {
            protected override void PreprocessingSteps(MagickImage image) {
                image.Alpha(AlphaOption.Remove);
                image.ColorThreshold(new MagickColor(230, 230, 230), new MagickColor(255, 255, 255));
                image.Negate();
                image.Resize(new Percentage(300));
            }
        }

        public class ResizeImagePreprocessor : ImagePreprocessor
        {
            private int resizePercentage;

            public ResizeImagePreprocessor(int resizePercentage) {
                this.resizePercentage = resizePercentage;
            }
            
            protected override void PreprocessingSteps(MagickImage image) {
                base.PreprocessingSteps(image);
                image.Resize(new Percentage(resizePercentage));
            }
        }

        public class ResizeAndSharpenImagePreprocessor : ResizeImagePreprocessor
        {
            public ResizeAndSharpenImagePreprocessor(int resizePercentage) : base(resizePercentage) {
            }

            protected override void PreprocessingSteps(MagickImage image) {
                base.PreprocessingSteps(image);
                image.Sharpen();
            }
        }

        public class ImagePreprocessor
        {
            public Image PreprocessImage(Image image, Rectangle bounds) {
                return DoPreprocess(image, bounds, PreprocessingSteps);
            }

            protected virtual void PreprocessingSteps(MagickImage image) {
                image.Alpha(AlphaOption.Remove);
                image.BlackThreshold(new Percentage(27));
                image.Negate();
            }

            private Image DoPreprocess(Image image, Rectangle bounds, Action<MagickImage> steps) {

                using (var ms = new MemoryStream()) {
                    lock (image) {
                        image.Save(ms, ImageFormat.Bmp);
                    }
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
}
