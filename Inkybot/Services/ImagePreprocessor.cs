using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using ImageMagick;

namespace Inkybot.Services
{
    public partial class ScreenReaderDataProvider
    {
        public class RuneImagePreprocessor : ResizeImagePreprocessor
        {
            public RuneImagePreprocessor() : base(300) {
            }
            
            protected override void PreprocessingSteps(MagickImage image) {
                image.Alpha(AlphaOption.Remove);
                image.ColorThreshold(new MagickColor(230, 230, 230), new MagickColor(255, 255, 255));
                image.Negate();
                PreprocessResizeImage(image);
            }
        }

        public class ResizeImagePreprocessor : ImagePreprocessor
        {
            private int originalResizePercentage;

            public int resizePercentage {
                private set;
                get;
            }

            public ResizeImagePreprocessor(int resizePercentage) {
                this.resizePercentage = originalResizePercentage = resizePercentage;
            }
            
            public Image PreprocessImage(Image image, Rectangle bounds, double resizeRatio) {
                resizePercentage = (int) (resizeRatio * originalResizePercentage);
                return base.PreprocessImage(image, bounds);
            }

            protected override void PreprocessingSteps(MagickImage image) {
                base.PreprocessingSteps(image);
                PreprocessResizeImage(image);
            }

            protected void PreprocessResizeImage(MagickImage image) {
                image.Resize(new Percentage(resizePercentage));
            }
        }

        public class ResizeAndBinarizationImagePreprocessor : ResizeImagePreprocessor
        {
            public ResizeAndBinarizationImagePreprocessor(int resizePercentage) : base(resizePercentage) {
            }

            protected override void PreprocessingSteps(MagickImage image) {
                base.PreprocessingSteps(image);
                image.Sharpen();
                var percent = originalImageHeight >= 1080
                    ? 53
                    : 60;
                image.BlackThreshold(new Percentage(percent));
                image.WhiteThreshold(new Percentage(percent));
            }
        }

        public class ImagePreprocessor
        {
            protected int originalImageHeight;

            public Image PreprocessImage(Image image, Rectangle bounds) {
                return DoPreprocess(image, bounds, PreprocessingSteps);
            }

            protected virtual void PreprocessingSteps(MagickImage image) {
                image.Alpha(AlphaOption.Remove);
                var percent = originalImageHeight >= 1080
                    ? 29
                    : 27;
                image.BlackThreshold(new Percentage(percent));
                image.Negate();
            }

            private Image DoPreprocess(Image image, Rectangle bounds, Action<MagickImage> steps) {

                using (var ms = new MemoryStream()) {
                    lock (image) {
                        image.Save(ms, ImageFormat.Bmp);
                    }
                    ms.Position = 0;

                    using (var newImage = new MagickImage(ms)) {
                        originalImageHeight = newImage.Height;
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
