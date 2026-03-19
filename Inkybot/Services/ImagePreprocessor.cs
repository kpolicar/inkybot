using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using ImageMagick;
using Inkybot;
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
                var sw = Stopwatch.StartNew();
                long last = 0;
                void Log(string step) { var now = sw.ElapsedMilliseconds; Profiler.Record("Preprocess", $"StatValues.{step}", now - last); last = now; }

                image.ColorSpace = ColorSpace.Gray;
                Log("Grayscale");
                image.Alpha(AlphaOption.Remove);
                Log("AlphaRemove");
                image.Negate();
                Log("Negate");
                RemoveHorizontalLines(image);
                Log("RemoveHorizontalLines");
                image.FilterType = FilterType.Lanczos;
                image.Resize(new Percentage(resizePercentage));
                Log("Resize(Lanczos)");
                image.MedianFilter(2);
                Log("MedianFilter");
                image.WhiteThreshold(new Percentage(60));
                Log("WhiteThreshold");
                image.BorderColor = MagickColors.White;
                image.Border(75);
                Log("Border");
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
                var sw = Stopwatch.StartNew();
                long last = 0;
                void Log(string step) { var now = sw.ElapsedMilliseconds; Profiler.Record("Preprocess", $"MinMax.{step}", now - last); last = now; }

                image.ColorSpace = ColorSpace.Gray;
                Log("Grayscale");
                image.Alpha(AlphaOption.Remove);
                Log("AlphaRemove");
                image.Negate();
                Log("Negate");
                RemoveHorizontalLines(image);
                Log("RemoveHorizontalLines");
                image.FilterType = FilterType.Lanczos;
                image.Resize(new Percentage(resizePercentage));
                Log("Resize(Lanczos)");
                image.MedianFilter(2);
                Log("MedianFilter");
                image.WhiteThreshold(new Percentage(60));
                Log("WhiteThreshold");
                //image.AutoThreshold(AutoThresholdMethod.OTSU);
                //Log("OTSU");
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

                    // Solidify the gray line into pure white so the morphology catches it perfectly
                    lineMask.Threshold(new Percentage(40));

                    // Kernel sizes are for the pre-upscale image (3x smaller than before)
                    var openSettings = new MorphologySettings {
                        Method = MorphologyMethod.Open,
                        Kernel = Kernel.Rectangle,
                        KernelArguments = "20x1"
                    };
                    lineMask.Morphology(openSettings);

                    var dilateSettings = new MorphologySettings {
                        Method = MorphologyMethod.Dilate,
                        Kernel = Kernel.Rectangle,
                        KernelArguments = "1x3"
                    };
                    lineMask.Morphology(dilateSettings);

                    image.Composite(lineMask, CompositeOperator.Lighten);
                }
            }

            private Image DoPreprocess(Image image, Rectangle bounds, Action<MagickImage> steps) {
                var total = Stopwatch.StartNew();

                using (var ms = new MemoryStream()) {
                    lock (image) {
                        image.Save(ms, ImageFormat.Bmp);
                    }
                    ms.Position = 0;

                    using (var newImage = new MagickImage(ms)) {
                        originalImageHeight = newImage.Height;
                        var b = bounds;

                        newImage.Crop(new MagickGeometry(b.X, b.Y, (uint)b.Width, (uint)b.Height));

                        steps(newImage);

                        var sw = Stopwatch.StartNew();
                        var result = newImage.ToBitmap();
                        var shortName = GetType().Name.Replace("ImagePreprocessor", "");
                        Profiler.Record("Preprocess", $"{shortName}.ToBitmap", sw.ElapsedMilliseconds);
                        Profiler.Record("Preprocess", $"{shortName}.total", total.ElapsedMilliseconds);
                        return result;
                    }
                }
            }
        }
        
    }
}
