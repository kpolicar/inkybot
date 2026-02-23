using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using ImageMagick;
using NUnit.Framework;
using Tesseract;

namespace Tests
{
    /// <summary>
    /// Tests OCR column reading on a Dofus item stats screenshot.
    /// Uses two Tesseract engines: one for digit columns (Min, Max)
    /// and one for reading English stat names (Effects).
    /// </summary>
    [TestFixture]
    public class OcrColumnReadingTests
    {
        private static readonly string TessDataPath =
            Path.Combine(AppContext.BaseDirectory, @"Resources\Tesseract");

        // ── Crop coordinates within the full 1920x1080 screenshot ──
        private const int CropX = 587;
        private const int CropY = 325;
        private const int CropW = 908 - 587; // 356
        private const int CropH = 887 - 325; // 596

        // ── Row layout within the 356x596 cropped area ──
        // Header row occupies the first ~35px, data rows start after.
        // Each data row is ~35px tall. We always read 16 rows (empty rows produce "").
        private const int HeaderHeight = 35;
        private const int RowHeight = 35;
        private const int MaxDataRows = 16;

        // ── Column layout (in original 356-wide crop coordinates) ──
        // Measured from step5b (3x upscaled) and divided by 3:
        //   Min numbers centered ~x=20, span roughly x=3..40
        //   Max numbers centered ~x=80, span roughly x=55..100
        //   Effects text (icon+value+name) starts ~x=120, ends ~x=340
        private const int MinColX = 2;
        private const int MinColW = 42;
        private const int MaxColX = 45;
        private const int MaxColW = 52;
        private const int EffectsColX = 145;
        private const int EffectsColW = 205;

        private const int ScaleFactor = 3; // 300% upscale

        /// <summary>
        /// Returns the path to a screenshot file by index (1-16).
        /// </summary>
        private static string ScreenshotPath(int index) =>
            Path.Combine("Resources", "Screenshots", $"Screenshot_{index}.png");

        /// <summary>
        /// Loads a full 1920x1080 screenshot and crops the stats area.
        /// </summary>
        private MagickImage LoadAndCropScreenshot(string path)
        {
            var full = new MagickImage(path);
            full.Crop(new MagickGeometry(CropX, CropY, CropW, CropH));
            full.ResetPage();
            return full;
        }

        /// <summary>
        /// Runs the heavy preprocessing pipeline on the cropped stats image:
        /// upscale 300%, grayscale, alpha removal, median filter, negate, Otsu threshold, line removal.
        /// Returns a new MagickImage that callers must dispose.
        /// </summary>
        private MagickImage PreprocessFull(MagickImage source, string debugDir = null)
        {
            var processed = (MagickImage)source.Clone();

            if (debugDir != null) processed.Write(Path.Combine(debugDir, "step0_original.png"));

            // 1. Upscale
            processed.FilterType = FilterType.Lanczos;
            processed.Resize(new Percentage(300));
            if (debugDir != null) processed.Write(Path.Combine(debugDir, "step1_upscaled.png"));

            // 2. Grayscale AND Remove Alpha
            processed.ColorSpace = ColorSpace.Gray;
            processed.Alpha(AlphaOption.Remove);
            processed.MedianFilter(2);
            if (debugDir != null) processed.Write(Path.Combine(debugDir, "step2_grayscale_median.png"));

            // 3. Negate (light-on-dark → dark-on-light)
            processed.Negate();
            if (debugDir != null) processed.Write(Path.Combine(debugDir, "step3_negated.png"));

            // 4. Otsu threshold
            processed.AutoThreshold(AutoThresholdMethod.OTSU);
            if (debugDir != null) processed.Write(Path.Combine(debugDir, "step4_otsu.png"));

            // 5. Line removal via morphology
            using (var lineMask = processed.Clone())
            {
                lineMask.Negate();

                var morphologySettings = new MorphologySettings
                {
                    Method = MorphologyMethod.Open,
                    Kernel = Kernel.Rectangle,
                    KernelArguments = "60x1"
                };
                lineMask.Morphology(morphologySettings);

                if (debugDir != null) ((MagickImage)lineMask).Write(Path.Combine(debugDir, "step5a_linemask.png"));

                processed.Composite(lineMask, CompositeOperator.Lighten);
            }

            if (debugDir != null) processed.Write(Path.Combine(debugDir, "step5b_lines_removed.png"));

            return processed;
        }

        /// <summary>
        /// Crops a region from the already-preprocessed image (coordinates are in
        /// original cropped-image space and get scaled by <see cref="ScaleFactor"/>),
        /// then adds a white border and returns a Bitmap ready for Tesseract.
        /// </summary>
        private Bitmap CropForOcr(MagickImage preprocessed, MagickGeometry cropArea, string debugPath = null)
        {
            // Scale crop coordinates to match the 300% upscaled image
            var scaledCrop = new MagickGeometry(
                cropArea.X * ScaleFactor,
                cropArea.Y * ScaleFactor,
                (uint)(cropArea.Width * ScaleFactor),
                (uint)(cropArea.Height * ScaleFactor));

            using (var slice = (MagickImage)preprocessed.Clone())
            {
                slice.Crop(scaledCrop);
                slice.ResetPage();

                if (debugPath != null) slice.Write(debugPath + "_a_cropped.png");

                // Place on white canvas with whitespace border
                using (var canvas = new MagickImage(MagickColors.White, slice.Width + 150, slice.Height + 150))
                {
                    canvas.Composite(slice, 75, 75, CompositeOperator.Over);
                    if (debugPath != null) canvas.Write(debugPath + "_b_bordered.png");
                    return canvas.ToBitmap();
                }
            }
        }

        /// <summary>
        /// Runs a Tesseract engine on a preprocessed bitmap and returns trimmed lines.
        /// </summary>
        private string[] OcrLines(TesseractEngine engine, Bitmap image, PageSegMode segMode = PageSegMode.SparseText)
        {
            using (var page = engine.Process(image, segMode))
            {
                var raw = page.GetText();
                return raw
                    .Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(l => l.Trim())
                    .Where(l => l.Length > 0)
                    .ToArray();
            }
        }

        /// <summary>
        /// Creates a digit-only Tesseract engine (eng, TesseractOnly mode).
        /// Caller must dispose.
        /// </summary>
        private TesseractEngine CreateDigitEngine()
        {
            var engine = new TesseractEngine(TessDataPath, "eng", EngineMode.TesseractOnly);
            engine.SetVariable("tessedit_char_whitelist", "0123456789-%");
            engine.SetVariable("load_system_dawg", "0");
            engine.SetVariable("load_freq_dawg", "0");
            engine.SetVariable("load_unambig_dawg", "0");
            engine.SetVariable("load_punc_dawg", "0");
            engine.SetVariable("load_number_dawg", "0");
            engine.SetVariable("classify_bln_numeric_mode", "0");
            return engine;
        }

        /// <summary>
        /// Creates a text Tesseract engine (eng-fine-tuned, Default mode).
        /// Caller must dispose.
        /// </summary>
        private TesseractEngine CreateTextEngine()
        {
            return new TesseractEngine(TessDataPath, "eng-fine-tuned", EngineMode.Default);
        }

        /// <summary>
        /// Reads Min and Max columns row-by-row from a preprocessed image.
        /// Always reads <see cref="MaxDataRows"/> rows; empty rows produce "".
        /// </summary>
        private (List<string> minValues, List<string> maxValues) ReadMinMaxColumns(
            MagickImage preprocessed, TesseractEngine digitEngine, string debugDir = null)
        {
            var minValues = new List<string>();
            var maxValues = new List<string>();

            for (int row = 0; row < MaxDataRows; row++)
            {
                var rowY = HeaderHeight + (row * RowHeight);

                // Min column cell
                var minCrop = new MagickGeometry(MinColX, rowY, (uint)MinColW, (uint)RowHeight);
                var minDebug = debugDir != null ? Path.Combine(debugDir, $"min_row{row:D2}") : null;
                using (var minBmp = CropForOcr(preprocessed, minCrop, minDebug))
                {
                    var lines = OcrLines(digitEngine, minBmp, PageSegMode.SingleWord);
                    minValues.Add(lines.Length > 0 ? lines[0] : "");
                }

                // Max column cell
                var maxCrop = new MagickGeometry(MaxColX, rowY, (uint)MaxColW, (uint)RowHeight);
                var maxDebug = debugDir != null ? Path.Combine(debugDir, $"max_row{row:D2}") : null;
                using (var maxBmp = CropForOcr(preprocessed, maxCrop, maxDebug))
                {
                    var lines = OcrLines(digitEngine, maxBmp, PageSegMode.SingleWord);
                    maxValues.Add(lines.Length > 0 ? lines[0] : "");
                }
            }

            return (minValues, maxValues);
        }

        /// <summary>
        /// Reads the Effects/Stats column from a preprocessed image.
        /// Crops from the header down to cover all 16 possible data rows.
        /// </summary>
        private string[] ReadEffectsColumn(
            MagickImage preprocessed, TesseractEngine textEngine, string debugDir = null)
        {
            var dataHeight = MaxDataRows * RowHeight;
            var statsCrop = new MagickGeometry(EffectsColX, HeaderHeight, (uint)EffectsColW, (uint)dataHeight);
            var effectsDebug = debugDir != null ? Path.Combine(debugDir, "effects_full") : null;
            using (var statsBmp = CropForOcr(preprocessed, statsCrop, effectsDebug))
            {
                return OcrLines(textEngine, statsBmp);
            }
        }

        /// <summary>
        /// Discovery test: runs OCR on Screenshot_1 and dumps all intermediate
        /// images to a debug folder for manual inspection of crop coordinates.
        /// </summary>
        [Test]
        public void DiscoverAllScreenshots()
        {
            var debugDir = Path.Combine(AppContext.BaseDirectory, "debug_ocr");
            if (Directory.Exists(debugDir)) Directory.Delete(debugDir, true);
            Directory.CreateDirectory(debugDir);

            Console.WriteLine($"Debug images will be written to: {debugDir}");

            var path = ScreenshotPath(1);
            Assert.IsTrue(File.Exists(path), $"Screenshot not found at: {Path.GetFullPath(path)}");

            using (var digitEngine = CreateDigitEngine())
            using (var textEngine = CreateTextEngine())
            using (var cropped = LoadAndCropScreenshot(path))
            {
                // Save the raw crop so we can verify the initial crop region
                cropped.Write(Path.Combine(debugDir, "00_raw_crop.png"));

                Console.WriteLine($"Cropped image size: {cropped.Width}x{cropped.Height}");

                using (var preprocessed = PreprocessFull(cropped, debugDir))
                {
                    var (minVals, maxVals) = ReadMinMaxColumns(preprocessed, digitEngine, debugDir);
                    var effects = ReadEffectsColumn(preprocessed, textEngine, debugDir);

                    Console.WriteLine("\n=== Screenshot_1.png ===");
                    for (int i = 0; i < MaxDataRows; i++)
                    {
                        Console.WriteLine($"  Row {i:D2}: Min=[{minVals[i]}] Max=[{maxVals[i]}]");
                    }
                    Console.WriteLine("  Effects: " + string.Join(", ", effects.Select(v => $"[{v}]")));
                    Console.WriteLine($"  Effects count: {effects.Length}");
                }
            }
        }
    }
}

