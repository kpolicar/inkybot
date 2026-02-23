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
        private const int CropW = 908 - 587;
        private const int CropH = 887 - 325;

        // ── Row layout within the cropped area ──
        private const int HeaderHeight = 8;
        private const int RowHeight = 42;
        private const int MaxDataRows = 13;

        // ── Column layout (in original crop coordinates) ──
        private const int MinColX = 2;
        private const int MinColW = 42;
        private const int MaxColX = 45;
        private const int MaxColW = 52;
        private const int EffectsColX = 145;
        private const int EffectsColW = 205;

        private const int ScaleFactor = 3; // 300% upscale

        // ── Tesseract engines loaded once for the entire fixture ──
        private TesseractEngine _digitEngine;
        private TesseractEngine _textEngine;

        [OneTimeSetUp]
        public void SetUpEngines()
        {
            _digitEngine = CreateDigitEngine();
            _textEngine = CreateTextEngine();
        }

        [OneTimeTearDown]
        public void TearDownEngines()
        {
            _digitEngine?.Dispose();
            _textEngine?.Dispose();
        }

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
        /// Shared preprocessing steps: upscale, grayscale, alpha removal, median filter, negate.
        /// Returns a new MagickImage that callers must dispose.
        /// </summary>
        private MagickImage PreprocessBase(MagickImage source, string debugDir = null)
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

            // 3. Negate (light-on-dark → dark-on-light)
            processed.Negate();

            if (debugDir != null) processed.Write(Path.Combine(debugDir, "step2_gray_negated.png"));

            return processed;
        }

        /// <summary>
        /// Applies horizontal line removal via morphology on an already-preprocessed image.
        /// Modifies the image in-place.
        /// </summary>
        private void RemoveHorizontalLines(MagickImage processed)
        {
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

                var dilateSettings = new MorphologySettings
                {
                    Method = MorphologyMethod.Dilate,
                    Kernel = Kernel.Rectangle,
                    KernelArguments = "1x6"
                };
                lineMask.Morphology(dilateSettings);

                processed.Composite(lineMask, CompositeOperator.Lighten);
            }
        }

        /// <summary>
        /// Full preprocessing for digit columns (Min/Max): shared base steps,
        /// then Otsu binarization, then line removal. Best for legacy Tesseract engine.
        /// Returns a new MagickImage that callers must dispose.
        /// </summary>
        private MagickImage PreprocessForDigits(MagickImage source, string debugDir = null)
        {
            var debugSub = debugDir != null ? debugDir : null;
            var processed = PreprocessBase(source, debugSub);

            // 4. Otsu threshold (binarization — helps legacy Tesseract engine)
            processed.AutoThreshold(AutoThresholdMethod.OTSU);

            if (debugDir != null) processed.Write(Path.Combine(debugDir, "digits_post_otsu.png"));

            // 5. Line removal
            RemoveHorizontalLines(processed);

            if (debugDir != null) processed.Write(Path.Combine(debugDir, "digits_final.png"));

            return processed;
        }

        /// <summary>
        /// Preprocessing for the Effects/Stats text column: shared base steps,
        /// then line removal, then white threshold. No Otsu binarization —
        /// LSTM engine works better with grayscale input.
        /// Returns a new MagickImage that callers must dispose.
        /// </summary>
        private MagickImage PreprocessForText(MagickImage source, string debugDir = null)
        {
            var processed = PreprocessBase(source);

            // 4. Line removal (before white threshold so lines are detected in grayscale)
            RemoveHorizontalLines(processed);

            if (debugDir != null) processed.Write(Path.Combine(debugDir, "text_after_lineremoval.png"));

            // 5. White threshold — push remaining light gray backgrounds to pure white
            processed.WhiteThreshold(new Percentage(60));

            if (debugDir != null) processed.Write(Path.Combine(debugDir, "text_final.png"));

            return processed;
        }

        /// <summary>
        /// Crops a region from the already-preprocessed image (coordinates are in
        /// original cropped-image space and get scaled by <see cref="ScaleFactor"/>),
        /// then adds a white border and returns a Bitmap ready for Tesseract.
        /// </summary>
        private Bitmap CropForOcr(MagickImage preprocessed, MagickGeometry cropArea, string debugPath = null)
        {
            var scaledCrop = new MagickGeometry(
                cropArea.X * ScaleFactor,
                cropArea.Y * ScaleFactor,
                (uint)(cropArea.Width * ScaleFactor),
                (uint)(cropArea.Height * ScaleFactor));

            // --- THE FIX IS HERE ---
            // Pass the geometry directly into Clone(). 
            // This creates a tiny image in RAM instantly, rather than cloning the giant image and cropping it after.
            using (var slice = (MagickImage)preprocessed.Clone(scaledCrop))
            {
                slice.ResetPage();

                if (debugPath != null) slice.Write(debugPath + "_cropped.png");

                using (var canvas = new MagickImage(MagickColors.White, slice.Width + 150, slice.Height + 150))
                {
                    canvas.Composite(slice, 75, 75, CompositeOperator.Over);
                    if (debugPath != null) canvas.Write(debugPath + "_bordered.png");
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
            engine.SetVariable("classify_enable_learning", "0");
            engine.SetVariable("classify_enable_adaptive_matcher", "0");
            engine.SetVariable("tessedit_enable_doc_dict", "0");
            return engine;
        }

        /// <summary>
        /// Creates a text Tesseract engine (eng-fine-tuned, Default mode).
        /// Caller must dispose.
        /// </summary>
        private TesseractEngine CreateTextEngine()
        {
            var engine = new TesseractEngine(TessDataPath, "eng-fine-tuned", EngineMode.Default);
    
            // Tell the LSTM to stop guessing based on English dictionary words
            engine.SetVariable("load_system_dawg", "0");
            engine.SetVariable("load_freq_dawg", "0");
            engine.SetVariable("load_unambig_dawg", "0");
            engine.SetVariable("classify_enable_learning", "0");
            engine.SetVariable("classify_enable_adaptive_matcher", "0");
    
            return engine;
        }

        /// <summary>
        /// Reads Min and Max columns row-by-row from a cropped source image.
        /// Preprocesses with Otsu binarization (best for legacy Tesseract digit engine).
        /// Always reads <see cref="MaxDataRows"/> rows; empty rows produce "".
        /// </summary>
        private (List<string> minValues, List<string> maxValues) ReadMinMaxColumns(
            MagickImage cropped, TesseractEngine digitEngine, string debugDir = null)
        {
            var minValues = new List<string>();
            var maxValues = new List<string>();

            using (var preprocessed = PreprocessForDigits(cropped, debugDir))
            {
                for (int row = 0; row < MaxDataRows; row++)
                {
                    var rowY = HeaderHeight + (row * RowHeight);

                    var minCrop = new MagickGeometry(MinColX, rowY, (uint)MinColW, (uint)RowHeight);
                    var minDebug = debugDir != null ? Path.Combine(debugDir, $"min_row{row:D2}") : null;
                    using (var minBmp = CropForOcr(preprocessed, minCrop, minDebug))
                    {
                        var lines = OcrLines(digitEngine, minBmp, PageSegMode.SingleWord);
                        minValues.Add(lines.Length > 0 ? lines[0] : "");
                    }

                    var maxCrop = new MagickGeometry(MaxColX, rowY, (uint)MaxColW, (uint)RowHeight);
                    var maxDebug = debugDir != null ? Path.Combine(debugDir, $"max_row{row:D2}") : null;
                    using (var maxBmp = CropForOcr(preprocessed, maxCrop, maxDebug))
                    {
                        var lines = OcrLines(digitEngine, maxBmp, PageSegMode.SingleWord);
                        maxValues.Add(lines.Length > 0 ? lines[0] : "");
                    }
                }
            }

            return (minValues, maxValues);
        }

        /// <summary>
        /// Reads the Effects/Stats column from a cropped source image.
        /// Preprocesses without Otsu binarization (LSTM engine works better with grayscale).
        /// Crops from the header down to cover all possible data rows.
        /// </summary>
        private string[] ReadEffectsColumn(
            MagickImage cropped, TesseractEngine textEngine, string debugDir = null)
        {
            using (var preprocessed = PreprocessForText(cropped, debugDir))
            {
                var dataHeight = MaxDataRows * RowHeight;
                var statsCrop = new MagickGeometry(EffectsColX, HeaderHeight, (uint)EffectsColW, (uint)dataHeight);
                var effectsDebug = debugDir != null ? Path.Combine(debugDir, "effects_full") : null;
                using (var statsBmp = CropForOcr(preprocessed, statsCrop, effectsDebug))
                {
                    return OcrLines(textEngine, statsBmp);
                }
            }
        }

        /// <summary>
        /// Shared assertion logic for a single screenshot test case.
        /// On failure, dumps all intermediate and crop images to a debug folder.
        /// </summary>
        private void AssertScreenshot(int index,
            string[] expectedMin, string[] expectedMax, string[] expectedEffects)
        {
            var path = ScreenshotPath(index);
            Assert.IsTrue(File.Exists(path),
                $"Screenshot not found at: {Path.GetFullPath(path)}");

            using (var cropped = LoadAndCropScreenshot(path))
            {
                var (minVals, maxVals) = ReadMinMaxColumns(cropped, _digitEngine);
                var effects = ReadEffectsColumn(cropped, _textEngine);

                // ── Console output: combine Min, Max, Effects per row ──
                Console.WriteLine($"=== Screenshot_{index} OCR Results ===");
                var maxRows = Math.Max(MaxDataRows, effects.Length);
                for (int i = 0; i < maxRows; i++)
                {
                    var min = i < minVals.Count ? minVals[i] : "";
                    var max = i < maxVals.Count ? maxVals[i] : "";
                    var eff = i < effects.Length ? effects[i] : "";
                    Console.WriteLine($"  Row {i:D2}: Min=[{min}]  Max=[{max}]  Effect=[{eff}]");
                }

                // ── Collect all mismatches ──
                var failures = new List<string>();

                if (minVals.Count != MaxDataRows)
                    failures.Add($"Min column row count: expected {MaxDataRows} but got {minVals.Count}");
                if (maxVals.Count != MaxDataRows)
                    failures.Add($"Max column row count: expected {MaxDataRows} but got {maxVals.Count}");

                for (int i = 0; i < MaxDataRows && i < minVals.Count; i++)
                {
                    if (expectedMin[i] != minVals[i])
                        failures.Add($"Min row {i}: expected '{expectedMin[i]}' but got '{minVals[i]}'");
                    if (expectedMax[i] != maxVals[i])
                        failures.Add($"Max row {i}: expected '{expectedMax[i]}' but got '{maxVals[i]}'");
                }

                if (expectedEffects.Length != effects.Length)
                    failures.Add($"Effects count: expected {expectedEffects.Length} but got {effects.Length}. " +
                                 $"Actual: [{string.Join(", ", effects)}]");

                for (int i = 0; i < Math.Min(expectedEffects.Length, effects.Length); i++)
                {
                    if (expectedEffects[i] != effects[i])
                        failures.Add($"Effects row {i}: expected '{expectedEffects[i]}' but got '{effects[i]}'");
                }

                // ── On failure: dump debug images and assert ──
                if (failures.Count > 0)
                {
                    var debugDir = Path.Combine(AppContext.BaseDirectory, "debug_ocr", $"Screenshot_{index}");
                    if (Directory.Exists(debugDir)) Directory.Delete(debugDir, true);
                    Directory.CreateDirectory(debugDir);

                    Console.WriteLine($"  FAILURES DETECTED — dumping debug images to: {debugDir}");

                    // Save the raw crop
                    cropped.Write(Path.Combine(debugDir, "00_raw_crop.png"));

                    // Re-run with debug image saving (each method preprocesses internally)
                    ReadMinMaxColumns(cropped, _digitEngine, debugDir);
                    ReadEffectsColumn(cropped, _textEngine, debugDir);

                    Assert.Fail(string.Join("\n", failures));
                }
            }
        }

        [Test]
        public void Screenshot_01_Reads_AllColumns()
        {
            AssertScreenshot(1,
                new[] { "301", "71", "31", "3", "1", "1", "11", "11", "16", "16", "7", "7", "-1" },
                new[] { "350", "100", "50", "4", "1", "1", "15", "15", "20", "20", "10", "10", "-1" },
                new[] { "437 Vitality", "100 Agility", "39 Wisdom", "4% Critical", "1 AP", "1 Summons",
                    "9 Air damage", "13 Prospecting", "18 Water Resistance", "18 Air Resistance",
                    "9 Lock", "9 Critical Damage", "-1 Range" });
        }

        [Test]
        public void Screenshot_02_Reads_AllColumns()
        {
            AssertScreenshot(2,
                new[] { "201", "41", "41", "31", "1", "8", "8", "7", "301", "5", "5", "5", "4" },
                new[] { "250", "60", "60", "40", "1", "12", "12", "10", "400", "7", "7", "7", "5" },
                new[] { "292 Vitality", "59 Chance", "58 Agility", "30 Wisdom", "1 Range",
                    "11 Water Damage", "11 Air damage", "6 Prospecting", "391 Initiative",
                    "7% Neutral Resistance", "7% Earth Resistance", "7% Fire Resistance", "4 Lock" });
        }

        [Test]
        public void Screenshot_03_Reads_AllColumns()
        {
            AssertScreenshot(3,
                new[] { "", "251", "41", "31", "1", "11", "11", "7", "4", "11", "", "", "" },
                new[] { "", "300", "60", "40", "1", "15", "15", "10", "6", "15", "", "", "" },
                new[] { "1 AP", "294 Vitality", "57 Agility", "33 Wisdom", "1 Range",
                    "14 Air damage", "9 Prospecting", "10% Earth Resistance",
                    "5 MP Parry", "14 Pushback Resistance" });
        }

        [Test]
        public void Screenshot_04_Reads_AllColumns()
        {
            AssertScreenshot(4,
                new[] { "301", "41", "41", "31", "1", "1", "9", "9", "9", "11", "7", "5", "11" },
                new[] { "350", "60", "60", "40", "1", "1", "12", "12", "12", "15", "10", "7", "15" },
                new[] { "393 Vitality", "54 Strength", "59 Agility", "36 Wisdom", "1 AP", "1 MP",
                    "10 Neutral Damage", "11 Earth Damage", "11 Air damage", "6 Prospecting",
                    "10% Water Resistance", "7 Dodge", "14 Critical Resistance" });
        }

        [Test]
        public void Screenshot_05_Reads_AllColumns()
        {
            AssertScreenshot(5,
                new[] { "301", "31", "31", "3", "1", "11", "7", "7", "5", "16", "16", "-10", "" },
                new[] { "350", "40", "50", "4", "1", "15", "10", "10", "7", "20", "20", "-10", "" },
                new[] { "374 Vitality", "36 Wisdom", "49 Power", "4% Critical", "1 AP",
                    "2 Prospecting", "10% Neutral Resistance", "10% Water Resistance",
                    "5 MP Parry", "19 Critical Damage", "18 Pushback Resistance", "-10 Dodge" });
        }

        [Test]
        public void Screenshot_06_Reads_AllColumns()
        {
            AssertScreenshot(6,
                new[] { "251", "41", "31", "1", "11", "11", "7", "4", "11", "", "", "", "" },
                new[] { "300", "60", "40", "1", "15", "15", "10", "6", "15", "", "", "", "" },
                new[] { "257 Vitality", "40 Agility", "16 Wisdom", "1 Range",
                    "9 Air damage", "5 Prospecting", "7% Earth Resistance",
                    "2 MP Parry", "10 Pushback Resistance" });
        }

        [Test]
        public void Screenshot_07_Reads_AllColumns()
        {
            AssertScreenshot(7,
                new[] { "251", "41", "31", "1", "11", "11", "7", "4", "11", "", "", "", "" },
                new[] { "300", "60", "40", "1", "15", "15", "10", "6", "15", "", "", "", "" },
                new[] { "257 Vitality", "43 Agility", "16 Wisdom", "1 Range",
                    "9 Air damage", "5 Prospecting", "7% Earth Resistance",
                    "2 MP Parry", "10 Pushback Resistance" });
        }

        [Test]
        public void Screenshot_08_Reads_AllColumns()
        {
            AssertScreenshot(8,
                new[] { "251", "41", "31", "1", "11", "11", "7", "4", "11", "", "", "", "" },
                new[] { "300", "60", "40", "1", "15", "15", "10", "6", "15", "", "", "", "" },
                new[] { "257 Vitality", "40 Agility", "14 Wisdom", "1 Range",
                    "9 Air damage", "5 Prospecting", "7% Earth Resistance",
                    "2 MP Parry", "10 Pushback Resistance" });
        }

        [Test]
        public void Screenshot_09_Reads_AllColumns()
        {
            AssertScreenshot(9,
                new[] { "251", "41", "31", "1", "11", "11", "7", "4", "11", "", "", "", "" },
                new[] { "300", "60", "40", "1", "15", "15", "10", "6", "15", "", "", "", "" },
                new[] { "307 Vitality", "40 Agility", "12 Wisdom", "1 Range",
                    "9 Air damage", "3 Prospecting", "7% Earth Resistance",
                    "2 MP Parry", "10 Pushback Resistance" });
        }

        [Test]
        public void Screenshot_10_Reads_AllColumns()
        {
            AssertScreenshot(10,
                new[] { "", "251", "41", "31", "1", "11", "11", "7", "4", "11", "", "", "" },
                new[] { "", "300", "60", "40", "1", "15", "15", "10", "6", "15", "", "", "" },
                new[] { "10 Initiative", "302 Vitality", "40 Agility", "12 Wisdom", "1 Range",
                    "9 Air damage", "3 Prospecting", "7% Earth Resistance",
                    "2 MP Parry", "10 Pushback Resistance" });
        }

        [Test]
        public void Screenshot_11_Reads_AllColumns()
        {
            AssertScreenshot(11,
                new[] { "101", "36", "2", "1", "7", "11", "5", "5", "7", "7", "", "", "" },
                new[] { "150", "45", "3", "1", "10", "15", "7", "7", "10", "10", "", "", "" },
                new[] { "112 Vitality", "44 Wisdom", "2% Critical", "1 AP", "0 Heal",
                    "10 Prospecting", "7% Earth Resistance", "0% Fire Resistance",
                    "0 Critical Damage", "0 Pushback Damage" });
        }

        [Test]
        public void Screenshot_12_Reads_AllColumns()
        {
            AssertScreenshot(12,
                new[] { "101", "36", "2", "1", "7", "11", "5", "5", "7", "7", "", "", "" },
                new[] { "150", "45", "3", "1", "10", "15", "7", "7", "10", "10", "", "", "" },
                new[] { "112 Vitality", "44 Wisdom", "2% Critical", "1 AP", "0 Heal",
                    "10 Prospecting", "7% Earth Resistance", "0% Fire Resistance",
                    "0 Critical Damage", "1 Pushback Damage" });
        }

        [Test]
        public void Screenshot_13_Reads_AllColumns()
        {
            AssertScreenshot(13,
                new[] { "151", "26", "26", "21", "2", "6", "11", "301", "6", "6", "", "", "" },
                new[] { "200", "40", "40", "30", "3", "10", "20", "400", "10", "10", "", "", "" },
                new[] { "19 Vitality", "16 Intelligence", "33 Chance", "16 Wisdom",
                    "1% Critical", "4 Damage", "28 Prospecting", "20 Initiative",
                    "0% Earth Resistance", "0 Earth Resistance" });
        }

        [Test]
        public void Screenshot_14_Reads_AllColumns()
        {
            AssertScreenshot(14,
                new[] { "", "16", "7", "", "", "", "", "", "", "", "", "", "" },
                new[] { "-", "20", "10", "", "", "", "", "", "", "", "", "", "" },
                new[] { "3 Initiative", "25 Vitality", "12 Power" });
        }

        [Test]
        public void Screenshot_15_Reads_AllColumns()
        {
            AssertScreenshot(15,
                new[] { "16", "16", "", "", "", "", "", "", "", "", "", "", "" },
                new[] { "20", "20", "", "", "", "", "", "", "", "", "", "", "" },
                new[] { "32 Vitality", "7 Intelligence" });
        }

        [Test]
        public void Screenshot_16_Reads_AllColumns()
        {
            AssertScreenshot(16,
                new[] { "", "", "101", "31", "31", "1", "5", "11", "6", "6", "6", "6", "11" },
                new[] { "", "", "150", "50", "40", "1", "7", "20", "10", "10", "10", "10", "20" },
                new[] { "10 Initiative", "1 Pushback Resistance", "127 Vitality", "44 Agility",
                    "40 Wisdom", "1 Range", "6 Damage", "17 Prospecting", "7% Water Resistance",
                    "8 Neutral Resistance", "6 Earth Resistance", "7 Water Resistance",
                    "17 Trap Damage" });
        }

        [Test]
        public void Screenshot_17_Reads_AllColumns()
        {
            AssertScreenshot(17,
                new[] { "-", "-", "-", "16", "", "", "", "", "", "", "", "", "" },
                new[] { "-", "-", "-", "20", "", "", "", "", "", "", "", "", "" },
                new[] { "4 Strength", "31 Initiative", "1% Earth Resistance", "5 Vitality" });
        }

        [Test]
        public void Screenshot_18_Reads_AllColumns()
        {
            AssertScreenshot(18,
                new[] { "6", "1", "101", "21", "21", "", "", "", "", "", "", "", "" },
                new[] { "15", "10", "130", "35", "35", "", "", "", "", "", "", "", "" },
                new[] { "6 to 15 Neutral damage", "1 to 10 Fire steal", "108 Vitality",
                    "31 Intelligence", "30 Wisdom" });
        }

        [Test]
        public void Screenshot_19_Reads_AllColumns()
        {
            AssertScreenshot(19,
                new[] { "1", "251", "41", "41", "6", "11", "", "", "", "", "", "", "" },
                new[] { "10", "300", "60", "60", "10", "20", "", "", "", "", "", "", "" },
                new[] { "1 to 10 Air steal", "243 Vitality", "50 Agility", "60 Wisdom",
                    "9 Damage", "13 Prospecting" });
        }

        [Test]
        public void Screenshot_20_Reads_AllColumns()
        {
            AssertScreenshot(20,
                new[] { "1", "251", "41", "41", "6", "11", "", "", "", "", "", "", "" },
                new[] { "10", "300", "60", "60", "10", "20", "", "", "", "", "", "", "" },
                new[] { "1 to 10 Air steal", "238 Vitality", "63 Agility", "57 Wisdom",
                    "8 Damage", "12 Prospecting" });
        }

        [Test]
        public void Screenshot_21_Reads_AllColumns()
        {
            AssertScreenshot(21,
                new[] { "6", "21", "7", "2", "", "", "", "", "", "", "", "", "" },
                new[] { "10", "25", "10", "2", "", "", "", "", "", "", "", "", "" },
                new[] { "6 to 10 Neutral damage", "71 Vitality", "4 Strength", "0 Earth Damage" });
        }

        [Test]
        public void Screenshot_22_Reads_AllColumns()
        {
            AssertScreenshot(22,
                new[] { "11", "11", "201", "41", "21", "3", "1", "6", "11", "6", "", "", "" },
                new[] { "23", "23", "250", "60", "30", "4", "1", "10", "20", "10", "", "", "" },
                new[] { "11 to 23 Neutral damage", "11 to 23 Water damage", "225 Vitality",
                    "58 Chance", "22 Wisdom", "4% Critical", "1 Range", "7 Damage",
                    "27 Prospecting", "5 Earth Resistance" });
        }

        [Test]
        public void Screenshot_23_Reads_AllColumns()
        {
            AssertScreenshot(23,
                new[] { "81", "46", "36", "2", "6", "6", "6", "6", "", "", "", "", "" },
                new[] { "120", "60", "50", "2", "10", "10", "10", "10", "", "", "", "", "" },
                new[] { "88 Vitality", "54 Chance", "50 Wisdom", "2 Range", "6 Prospecting",
                    "6% Neutral Resistance", "10% Earth Resistance", "7% Water Resistance" });
        }

        [Test]
        public void Screenshot_24_Reads_AllColumns()
        {
            AssertScreenshot(24,
                new[] { "81", "46", "36", "2", "6", "6", "6", "6", "", "", "", "", "" },
                new[] { "120", "60", "50", "2", "10", "10", "10", "10", "", "", "", "", "" },
                new[] { "143 Vitality", "48 Chance", "49 Wisdom", "1 Range", "9 Prospecting",
                    "6% Neutral Resistance", "10% Earth Resistance", "7% Water Resistance" });
        }

        [Test]
        public void Screenshot_25_Reads_AllColumns()
        {
            AssertScreenshot(25,
                new[] { "61", "61", "41", "6", "6", "3", "3", "3", "", "", "", "", "" },
                new[] { "80", "80", "60", "10", "15", "5", "5", "5", "", "", "", "", "" },
                new[] { "74 Strength", "74 Intelligence", "58 Wisdom", "9 Damage",
                    "5 Prospecting", "1% Neutral Resistance", "5% Earth Resistance",
                    "6% Air Resistance" });
        }

        [Test]
        public void Screenshot_26_Reads_AllColumns()
        {
            AssertScreenshot(26,
                new[] { "", "26", "16", "11", "2", "4", "", "", "", "", "", "", "" },
                new[] { "", "35", "20", "15", "3", "5", "", "", "", "", "", "", "" },
                new[] { "1 Damage", "0 Vitality", "1 Intelligence", "32 Wisdom",
                    "0 Fire Damage", "0 Dodge" });
        }

        [Test]
        public void Screenshot_27_Reads_AllColumns()
        {
            AssertScreenshot(27,
                new[] { "", "", "", "21", "4", "", "", "", "", "", "", "", "" },
                new[] { "-", "-", "-", "30", "5", "", "", "", "", "", "", "", "" },
                new[] { "3 Agility", "2 Wisdom", "3 MP Parry", "20 Vitality", "1 Lock" });
        }

        [Test]
        public void Screenshot_28_Reads_AllColumns()
        {
            AssertScreenshot(28,
                new[] { "", "5", "5", "5", "5", "", "", "", "", "", "", "", "" },
                new[] { "", "5", "5", "5", "5", "", "", "", "", "", "", "", "" },
                new[] { "2 Wisdom", "6 Strength", "6 Intelligence", "5 Chance", "5 Agility" });
        }

        [Test]
        public void Screenshot_29_Reads_AllColumns()
        {
            AssertScreenshot(29,
                new[] { "251", "41", "31", "1", "11", "11", "7", "4", "11", "", "", "", "" },
                new[] { "300", "60", "40", "1", "15", "15", "10", "6", "15", "", "", "", "" },
                new[] { "287 Vitality", "39 Agility", "34 Wisdom", "1 Range", "8 Air damage",
                    "0 Prospecting", "7% Earth Resistance", "0 MP Parry",
                    "4 Pushback Resistance" });
        }

        [Test]
        public void Screenshot_30_Reads_AllColumns()
        {
            AssertScreenshot(30,
                new[] { "251", "41", "31", "1", "11", "11", "7", "4", "11", "", "", "", "" },
                new[] { "300", "60", "40", "1", "15", "15", "10", "6", "15", "", "", "", "" },
                new[] { "226 Vitality", "39 Agility", "33 Wisdom", "1 Range", "3 Air damage",
                    "0 Prospecting", "8% Earth Resistance", "4 MP Parry",
                    "2 Pushback Resistance" });
        }

        [Test]
        public void Screenshot_31_Reads_AllColumns()
        {
            AssertScreenshot(31,
                new[] { "251", "41", "31", "1", "11", "11", "7", "4", "11", "", "", "", "" },
                new[] { "300", "60", "40", "1", "15", "15", "10", "6", "15", "", "", "", "" },
                new[] { "205 Vitality", "39 Agility", "32 Wisdom", "1 Range", "2 Air damage",
                    "1 Prospecting", "7% Earth Resistance", "4 MP Parry",
                    "0 Pushback Resistance" });
        }
    }
}
