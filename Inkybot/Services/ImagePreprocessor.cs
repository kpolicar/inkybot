using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using ImageMagick;
using Inkybot.Contracts;

namespace Inkybot.Services
{
    public partial class ScreenReaderDataProvider
    {
        public class RuneImagePreprocessor : ResizeImagePreprocessor
        {
            public RuneImagePreprocessor() : base(300) {
            }
            
            protected override void PreprocessingSteps(MagickImage image) {
                image.ColorSpace = ColorSpace.Gray;
                image.Alpha(AlphaOption.Remove);
                image.ColorThreshold(new MagickColor(230, 230, 230), new MagickColor(255, 255, 255));
                image.Negate();
                PreprocessResizeImage(image);
            }
        }

        public class ResizeImagePreprocessor : ImagePreprocessor
        {
            protected int originalResizePercentage;

            public virtual int resizePercentage {
                protected set;
                get;
            }

            public ResizeImagePreprocessor(int resizePercentage) {
                this.resizePercentage = originalResizePercentage = resizePercentage;
            }
            
            public virtual Image PreprocessImage(Image image, Rectangle bounds, double resizeRatio) {
                //resizePercentage = (int) (resizeRatio * originalResizePercentage);
                return base.PreprocessImage(image, bounds);
            }

            protected override void PreprocessingSteps(MagickImage image) {
                base.PreprocessingSteps(image);
                PreprocessResizeImage(image);
            }

            protected void PreprocessResizeImage(MagickImage image) {
                if (resizePercentage > 0)   
                    image.Resize(new Percentage(resizePercentage));
            }
        }

        public class ResizeAndBinarizationImagePreprocessor : ResizeImagePreprocessor
        {
            protected UserSettingsConfigManager UserSettings;
            protected override int thresholdPercentage => originalImageHeight >= 1010
                ? 60//53
                : 60;
            
            public ResizeAndBinarizationImagePreprocessor(UserSettingsConfigManager userSettings,
                int resizePercentage) : base(resizePercentage) {
                UserSettings = userSettings;
            }

            public override Image PreprocessImage(Image image, Rectangle bounds, double resizeRatio) {
                resizePercentage = (int) (UserSettings.CustomResizeMultiplier * originalResizePercentage);
                return base.PreprocessImage(image, bounds, resizeRatio);
            }

            protected override void PreprocessingSteps(MagickImage image) {
                PreprocessResizeImage(image);
                image.ColorSpace = ColorSpace.Gray;
                image.Sharpen();
                image.Alpha(AlphaOption.Remove);
                image.BlackThreshold(new Percentage(30));
                image.Negate();
                image.BlackThreshold(new Percentage(60));
            }
        }

        public class SinkScannerImagePreprocessor : ResizeImagePreprocessor
        {
            protected UserSettingsConfigManager UserSettings;
            
            public SinkScannerImagePreprocessor(UserSettingsConfigManager userSettings,
                int resizePercentage) : base(resizePercentage) {
                UserSettings = userSettings;
            }
            
            protected override void PreprocessingSteps(MagickImage image) {
                image.Resize(new Percentage(resizePercentage));
                image.ColorSpace = ColorSpace.Gray;
                image.Alpha(AlphaOption.Remove);
                image.BlackThreshold(new Percentage(40));
                image.Negate();
                //image.BlackThreshold(new Percentage(60));
            }
        }

        public class StatValuesImagePreprocessor : ResizeImagePreprocessor
        {
            protected UserSettingsConfigManager UserSettings;
            
            public StatValuesImagePreprocessor(UserSettingsConfigManager userSettings,
                int resizePercentage) : base(resizePercentage) {
                UserSettings = userSettings;
            }
            
            protected override void PreprocessingSteps(MagickImage image) {
                image.FilterType = FilterType.Lanczos;
                image.Resize(new Percentage(resizePercentage));
                image.ColorSpace = ColorSpace.Gray;
                image.Alpha(AlphaOption.Remove);
                image.MedianFilter(2);
                image.Negate();
                RemoveHorizontalLines(image);
                image.WhiteThreshold(new Percentage(60));
            }
        }

        public class MinMaxImagePreprocessor : ResizeImagePreprocessor
        {
            protected UserSettingsConfigManager UserSettings;
            
            public MinMaxImagePreprocessor(UserSettingsConfigManager userSettings,
                int resizePercentage) : base(resizePercentage) {
                UserSettings = userSettings;
            }
            
            protected override void PreprocessingSteps(MagickImage image) {
                image.FilterType = FilterType.Lanczos;
                image.Resize(new Percentage(resizePercentage));
                image.ColorSpace = ColorSpace.Gray;
                image.Alpha(AlphaOption.Remove);
                image.MedianFilter(2);
                image.Negate();
                image.AutoThreshold(AutoThresholdMethod.OTSU);
                RemoveHorizontalLines(image);
            }
        }

        public class ImagePreprocessor
        {
            protected uint originalImageHeight;
            protected virtual int thresholdPercentage => 27;

            public Image PreprocessImage(Image image, Rectangle bounds) {
                return DoPreprocess(image, bounds, PreprocessingSteps);
            }

            protected virtual void PreprocessingSteps(MagickImage image) {
                image.ColorSpace = ColorSpace.Gray;
                image.Alpha(AlphaOption.Remove);
                image.BlackThreshold(new Percentage(thresholdPercentage));
                image.Negate();
            }

            /// <summary>
            /// Removes horizontal lines via morphology: isolate lines with an Open operation,
            /// dilate the mask vertically to catch drop-shadow halos, then composite with Lighten.
            /// </summary>
            protected static void RemoveHorizontalLines(MagickImage image) {
                using (var lineMask = image.Clone()) {
                    lineMask.Negate();

                    var openSettings = new MorphologySettings {
                        Method = MorphologyMethod.Open,
                        Kernel = Kernel.Rectangle,
                        KernelArguments = "60x1"
                    };
                    lineMask.Morphology(openSettings);

                    var dilateSettings = new MorphologySettings {
                        Method = MorphologyMethod.Dilate,
                        Kernel = Kernel.Rectangle,
                        KernelArguments = "1x6"
                    };
                    lineMask.Morphology(dilateSettings);

                    image.Composite(lineMask, CompositeOperator.Lighten);
                }
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
                        newImage.Crop(new MagickGeometry(b.X, b.Y, (uint)b.Width, (uint)b.Height));

                        steps(newImage);

                        return newImage.ToBitmap();
                    }
                }
            }
        }
        
    }
}
