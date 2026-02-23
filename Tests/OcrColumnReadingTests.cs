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

        /// <summary>
        /// Preprocesses a column slice from the source image:
        /// crop, upscale 300%, grayscale, negate, Otsu threshold, white border.
        /// Returns a Bitmap ready for Tesseract.
        /// </summary>
        private Bitmap PreprocessSlice(MagickImage source, MagickGeometry cropArea)
        {
            using (var slice = (MagickImage)source.Clone())
            {
                slice.ToBitmap().Save("preprocessed_slice1.png"); 
    
                // 1. Crop to the region of interest
                slice.Crop(cropArea);
                slice.ResetPage();
                slice.ToBitmap().Save("preprocessed_slice2.png"); 

                // 2. Upscale
                slice.FilterType = FilterType.Lanczos;
                slice.Resize(new Percentage(300));
                slice.ToBitmap().Save("preprocessed_slice3.png"); 

                // 3. Grayscale AND Remove Alpha (The Safety Net)
                slice.ColorSpace = ColorSpace.Gray;
                slice.Alpha(AlphaOption.Remove); // <-- Strips any transparent UI artifacts

                // 4. Negate 
                slice.Negate();
                slice.ToBitmap().Save("preprocessed_slice4.png"); 

                // 5. Otsu threshold
                slice.AutoThreshold(AutoThresholdMethod.OTSU);
                slice.ToBitmap().Save("preprocessed_slice5.png"); 
                
                using (var lineMask = slice.Clone())
                {
                    // Negate so the shapes we want to target are White (ImageMagick morphology prefers white foregrounds)
                    lineMask.Negate(); 

                    // Apply an "Open" morphology using a horizontal rectangle kernel.
                    // "Rectangle:60x1" means: Destroy everything that cannot fit a 60-pixel wide horizontal line inside it.
                    // Since your upscaled minus signs '-' are likely around 20-30 pixels wide, they get destroyed.
                    // The bounding box lines (which span the whole 120px crop) will survive.
                    var morphologySettings = new MorphologySettings
                    {
                        Method = MorphologyMethod.Open,
                        Kernel = Kernel.Rectangle,
                        KernelArguments = "60x1" // 60 pixels wide, 1 pixel tall
                    };
                    lineMask.Morphology(morphologySettings);

                    // Now lineMask contains ONLY the long white bounding box lines on a black background.
                    // We composite this over our original slice using 'Lighten' or 'Screen'.
                    // This essentially paints white over the black bounding boxes on the original image, erasing them seamlessly!
                    slice.Composite(lineMask, CompositeOperator.Lighten);
                }
                slice.ToBitmap().Save("preprocessed_slice6.png"); 

                // 6. The Compositing Fix (Avoids Magick.NET's 1-bit palette washout)
                using (var canvas = new MagickImage(MagickColors.White, slice.Width + 150, slice.Height + 150))
                {
                    canvas.Composite(slice, 75, 75, CompositeOperator.Over);
                    canvas.ToBitmap().Save("preprocessed_slice7.png"); 
        
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
            {
                var imgWidth = (int)source.Width;
                var imgHeight = (int)source.Height;
                
                // Calculate headers and row heights
                var headerHeight = imgHeight / 11; // 1 header row
                var dataAreaHeight = imgHeight - headerHeight; // The remaining 10 rows
                var rowH = dataAreaHeight / 10;

                // ── Digit engine for Min / Max columns (digits, -, %) ──
                // Note: We use the "eng" model to safely process the '%' sign
                using (var digitEngine = new TesseractEngine(TessDataPath, "eng", EngineMode.Default))
                {
                    // Removed the trailing space from the whitelist to prevent hallucinated gaps
                    digitEngine.SetVariable("tessedit_char_whitelist", "0123456789-%");
                    digitEngine.SetVariable("load_system_dawg", "0");
                    digitEngine.SetVariable("load_freq_dawg", "0");
                    digitEngine.SetVariable("load_unambig_dawg", "0");
                    digitEngine.SetVariable("load_punc_dawg", "0");
                    digitEngine.SetVariable("load_number_dawg", "0");
                    // Removed the legacy 'classify_bln_numeric_mode' as it does nothing in v5

                    var minValues = new List<string>();
                    var maxValues = new List<string>();

                    // Iterate row by row to prevent Tesseract from dropping single digits
                    for (int row = 0; row < 10; row++)
                    {
                        var rowY = headerHeight + (row * rowH);

                        // -- Min column cell --
                        var minCrop = new MagickGeometry(0, rowY, 70, (uint)rowH);
                        using (var minBmp = PreprocessSlice(source, minCrop))
                        {
                            // Force SingleLine mode so it reads isolated single digits like '7'
                            var lines = OcrLines(digitEngine, minBmp, PageSegMode.SingleLine);
                            minValues.Add(lines.Length > 0 ? lines[0] : "");
                        }

                        // -- Max column cell --
                        var maxCrop = new MagickGeometry(70, rowY, 55, (uint)rowH);
                        using (var maxBmp = PreprocessSlice(source, maxCrop))
                        {
                            var lines = OcrLines(digitEngine, maxBmp, PageSegMode.SingleLine);
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
            {
                var imgHeight = (int)source.Height;
                var rowHeight = imgHeight / 11;

                // ── Text engine for the Effects / Stats column (English text + digits) ──
                using (var textEngine = new TesseractEngine(TessDataPath, "eng", EngineMode.Default))
                {
                    // The Effects/Stats column contains icon + value + stat name.
                    // We crop starting after the icon area (~148px) to get "10 Initiative", etc.
                    var statsCrop = new MagickGeometry(148, rowHeight, 230, (uint)(imgHeight - rowHeight));
                    using (var statsBmp = PreprocessSlice(source, statsCrop))
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

        [Test]
        public void ReadModifColumn_Values()
        {
            Assert.IsTrue(File.Exists(ImagePath),
                $"Test image not found at: {Path.GetFullPath(ImagePath)}");

            using (var source = new MagickImage(ImagePath))
            {
                var imgWidth = (int)source.Width;
                var imgHeight = (int)source.Height;
                var rowHeight = imgHeight / 11;

                // ── Digit engine for the Modif column ──
                using (var digitEngine = new TesseractEngine(TessDataPath, "eng", EngineMode.Default))
                {
                    digitEngine.SetVariable("tessedit_char_whitelist", "0123456789-+% ");

                    // Modif column: the coloured badges on the right side
                    // Only 2 rows have values: row 0 = "+10", row 1 = "-2"
                    // We process each row individually for better accuracy on small badges
                    var modifValues = new List<string>();

                    for (int row = 0; row < 10; row++)
                    {
                        var rowY = rowHeight + (row * ((imgHeight - rowHeight) / 10));
                        var rowH = (imgHeight - rowHeight) / 10;
                        var modifCrop = new MagickGeometry(370, rowY, (uint)imgWidth - 370 - 15, (uint)rowH);

                        using (var modifBmp = PreprocessSlice(source, modifCrop))
                        {
                            var lines = OcrLines(digitEngine, modifBmp, PageSegMode.SingleLine);
                            var value = lines.Length > 0 ? lines[0] : "";
                            modifValues.Add(value);
                        }
                    }

                    Console.WriteLine("=== Modif column OCR ===");
                    for (int i = 0; i < modifValues.Count; i++)
                        Console.WriteLine($"  Row {i}: [{modifValues[i]}]");

                    // We mainly care that the first two rows have recognisable modifier values
                    // Row 0: +10 (Initiative), Row 1: -2 (Vitality)
                    Assert.IsTrue(modifValues[0].Contains("10"),
                        $"Modif row 0: expected to contain '10', got '{modifValues[0]}'");
                    Assert.IsTrue(modifValues[1].Contains("2"),
                        $"Modif row 1: expected to contain '2', got '{modifValues[1]}'");
                }
            }
        }
    }
}

