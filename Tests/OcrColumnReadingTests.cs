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
    /// Uses two Tesseract engines: one for digit columns (Min, Max, stat values, Modif)
    /// and one for reading English stat names.
    /// </summary>
    [TestFixture]
    public class OcrColumnReadingTests
    {
        private static readonly string TessDataPath =
            Path.Combine(AppContext.BaseDirectory, @"Resources\Tesseract");

        private static readonly string ImagePath =
            Path.Combine("Resources", "Screenshots", "item_stats.png");

        // ── Column crop areas (x, y, width, height) estimated from the image layout ──
        // The image is ~480px wide x ~480px tall with 10 stat rows.
        // Min column:  roughly x=0..70
        // Max column:  roughly x=70..120
        // Stat value+name column (Effects/Stats): roughly x=130..360
        // Modif column: roughly x=360..420

        private const int ScaleFactor = 3; // 300% upscale

        /// <summary>
        /// Runs the heavy preprocessing pipeline once on the entire source image:
        /// upscale 300%, grayscale, alpha removal, negate, Otsu threshold, line removal.
        /// Returns a new MagickImage that callers must dispose.
        /// </summary>
        private MagickImage PreprocessFull(MagickImage source)
        {
            Directory.CreateDirectory("images");

            var processed = (MagickImage)source.Clone();
            processed.Write("images/step0_original.png");

            // 1. Upscale
            processed.FilterType = FilterType.Lanczos;
            processed.Resize(new Percentage(300));
            processed.Write("images/step1_upscaled.png");

            // 2. Grayscale AND Remove Alpha
            processed.ColorSpace = ColorSpace.Gray;
            processed.Alpha(AlphaOption.Remove);
            processed.Write("images/step2_grayscale.png");
            processed.MedianFilter(2);

            // 3. Negate (light-on-dark → dark-on-light)
            processed.Negate();
            processed.Write("images/step3_negated.png");

            // 4. Otsu threshold
            processed.AutoThreshold(AutoThresholdMethod.OTSU);
            processed.Write("images/step4_otsu.png");

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
                ((MagickImage)lineMask).Write("images/step5a_linemask.png");

                processed.Composite(lineMask, CompositeOperator.Lighten);
            }
            processed.Write("images/step5b_lines_removed.png");

            return processed;
        }

        /// <summary>
        /// Crops a region from the already-preprocessed image (coordinates are in
        /// original image space and get scaled by <see cref="ScaleFactor"/>),
        /// then adds a white border and returns a Bitmap ready for Tesseract.
        /// </summary>
        private Bitmap CropForOcr(MagickImage preprocessed, MagickGeometry cropArea, string label = null)
        {
            var tag = label ?? $"{cropArea.X}_{cropArea.Y}";

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
                slice.Write($"images/crop_{tag}_a_cropped.png");

                // Compositing fix: place on white canvas with whitespace border
                using (var canvas = new MagickImage(MagickColors.White, slice.Width + 150, slice.Height + 150))
                {
                    canvas.Composite(slice, 75, 75, CompositeOperator.Over);
                    canvas.Write($"images/crop_{tag}_b_bordered.png");
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
        
        [Test]
        public void ReadDigitColumns_MinAndMax()
        {
            Assert.IsTrue(File.Exists(ImagePath),
                $"Test image not found at: {Path.GetFullPath(ImagePath)}");

            using (var source = new MagickImage(ImagePath))
            using (var preprocessed = PreprocessFull(source))
            {
                var imgWidth = (int)source.Width;
                var imgHeight = (int)source.Height;
                
                // Calculate headers and row heights
                var headerHeight = imgHeight / 11; // 1 header row
                var dataAreaHeight = imgHeight - headerHeight; // The remaining 10 rows
                var rowH = dataAreaHeight / 10;

                // ── Digit engine for Min / Max columns (digits, -, %) ──
                using (var digitEngine = new TesseractEngine(TessDataPath, "eng", EngineMode.TesseractOnly))
                {
                    digitEngine.SetVariable("tessedit_char_whitelist", "0123456789-%");
                    digitEngine.SetVariable("load_system_dawg", "0");
                    digitEngine.SetVariable("load_freq_dawg", "0");
                    digitEngine.SetVariable("load_unambig_dawg", "0");
                    digitEngine.SetVariable("load_punc_dawg", "0");
                    digitEngine.SetVariable("load_number_dawg", "0");
                    digitEngine.SetVariable("classify_bln_numeric_mode", "0");

                    var minValues = new List<string>();
                    var maxValues = new List<string>();
                    
                    using (var minBmp = CropForOcr(preprocessed, new MagickGeometry(12, 30, 55-12, 465-30), "min_full"))
                    {
                        var lines = OcrLines(digitEngine, minBmp, PageSegMode.SparseText);
                        Console.WriteLine(lines);
                    }

                    // Iterate row by row to prevent Tesseract from dropping single digits
                    for (int row = 0; row < 10; row++)
                    {
                        var rowY = headerHeight + (row * rowH);

                        // -- Min column cell --
                        var minCrop = new MagickGeometry(0, rowY, 70, (uint)rowH);
                        using (var minBmp = CropForOcr(preprocessed, minCrop, $"min_row{row}"))
                        {
                            var lines = OcrLines(digitEngine, minBmp, PageSegMode.SingleWord);
                            minValues.Add(lines.Length > 0 ? lines[0] : "");
                        }

                        // -- Max column cell --
                        var maxCrop = new MagickGeometry(70, rowY, 55, (uint)rowH);
                        using (var maxBmp = CropForOcr(preprocessed, maxCrop, $"max_row{row}"))
                        {
                            var lines = OcrLines(digitEngine, maxBmp, PageSegMode.SingleWord);
                            maxValues.Add(lines.Length > 0 ? lines[0] : "");
                        }
                    }

                    // --- Console Output ---
                    Console.WriteLine("=== Min column OCR ===");
                    for (int i = 0; i < minValues.Count; i++) Console.WriteLine($"  Row {i}: [{minValues[i]}]");

                    Console.WriteLine("=== Max column OCR ===");
                    for (int i = 0; i < maxValues.Count; i++) Console.WriteLine($"  Row {i}: [{maxValues[i]}]");

                    // --- Assertions ---
                    var expectedMin = new[] { "-", "251", "51", "3%", "7", "4%", "4%", "5", "16", "-10" };
                    var expectedMax = new[] { "-", "300", "70", "4%", "10", "7%", "7%", "8", "25", "-10" };

                    Assert.AreEqual(expectedMin.Length, minValues.Count, 
                        "Min column: Row count mismatch. Should always be 10.");
                    Assert.AreEqual(expectedMax.Length, maxValues.Count, 
                        "Max column: Row count mismatch. Should always be 10.");

                    for (int i = 0; i < expectedMin.Length; i++)
                    {
                        Assert.AreEqual(expectedMin[i], minValues[i],
                            $"Min row {i}: expected '{expectedMin[i]}' but got '{minValues[i]}'");
                        
                        Assert.AreEqual(expectedMax[i], maxValues[i],
                            $"Max row {i}: expected '{expectedMax[i]}' but got '{maxValues[i]}'");
                    }
                }
            }
        }

        [Test]
        public void ReadTextColumn_StatNames()
        {
            Assert.IsTrue(File.Exists(ImagePath),
                $"Test image not found at: {Path.GetFullPath(ImagePath)}");

            using (var source = new MagickImage(ImagePath))
            using (var preprocessed = PreprocessFull(source))
            {
                var imgHeight = (int)source.Height;
                var rowHeight = imgHeight / 11;

                // ── Text engine for the Effects / Stats column (English text + digits) ──
                using (var textEngine = new TesseractEngine(TessDataPath, "eng-fine-tuned", EngineMode.Default))
                {
                    // The Effects/Stats column contains icon + value + stat name.
                    // We crop starting after the icon area (~148px) to get "10 Initiative", etc.
                    var statsCrop = new MagickGeometry(162, rowHeight, 200, (uint)(imgHeight - rowHeight));
                    using (var statsBmp = CropForOcr(preprocessed, statsCrop, "stats_full"))
                    {
                        var statsLines = OcrLines(textEngine, statsBmp);

                        Console.WriteLine("=== Stats column OCR ===");
                        foreach (var line in statsLines) Console.WriteLine($"  [{line}]");

                        // Expected stat text values read from the image
                        var expectedStats = new[]
                        {
                            "10 Initiative",
                            "284 Vitality",
                            "67 Agility",
                            "4% Critical",
                            "9 Air damage",
                            "6% Earth Resistance",
                            "6% Air Resistance",
                            "6 MP Parry",
                            "21 Pushback Damage",
                            "-10 Dodge"
                        };

                        Assert.AreEqual(expectedStats.Length, statsLines.Length,
                            $"Stats column: expected {expectedStats.Length} rows but got {statsLines.Length}.\n" +
                            $"Actual: [{string.Join(", ", statsLines)}]");

                        for (int i = 0; i < expectedStats.Length; i++)
                        {
                            Assert.AreEqual(expectedStats[i], statsLines[i],
                                $"Stats row {i}: expected '{expectedStats[i]}' but got '{statsLines[i]}'");
                        }
                    }
                }
            }
        }
    }
}

