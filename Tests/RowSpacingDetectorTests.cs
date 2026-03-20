using System.Drawing;
using Inkybot.Helpers;
using NUnit.Framework;
using Tesseract;

namespace Tests
{
    [TestFixture]
    public class RowSpacingDetectorTests
    {
        private RowSpacingDetector _detector;

        // StatMinBounds from Measurements: Rect.FromCoords(496, 314, 527, 846) at 1694x1009
        private static readonly Responsive.Measurement StatMinBounds = new Responsive.Measurement {
            Rectangle = Rect.FromCoords(496, 314, 527, 846),
            Width = 1694,
            Height = 1009
        };

        [SetUp]
        public void SetUp()
        {
            _detector = new RowSpacingDetector();
        }

        private static Bitmap CropToStatMinBounds(Bitmap screenshot)
        {
            var bounds = Responsive.ResponsiveRectangle(StatMinBounds, screenshot.Width, screenshot.Height);
            return screenshot.Clone(bounds, screenshot.PixelFormat);
        }

        // All screenshots should detect 13 rows (clamped from 13-14 actual rows)
        [TestCase("./Resources/Screenshots/Screenshot_1.png")]           // 1920x1080
        [TestCase("./Resources/Screenshots/Screenshot_2.png")]           // 1920x1080
        [TestCase("./Resources/Screenshots/Screenshot_3.png")]           // 1920x1080
        [TestCase("./Resources/Screenshots/Screenshot_1920x1032.png")]   // 1920x1032
        [TestCase("./Resources/Screenshots/Screenshot_1920x1032_v2.png")]// 1920x1032
        [TestCase("./Resources/Screenshots/Screenshot_1466x823.png")]    // 1466x823
        [TestCase("./Resources/Screenshots/Screenshot_1466x546.png")]    // 1466x546
        [TestCase("./Resources/Screenshots/Screenshot_1808x546.png")]    // 1808x546
        [TestCase("./Resources/Screenshots/Screenshot_1916x904.png")]    // 1916x904 - 14 rows visible
        [TestCase("./Resources/Screenshots/Screenshot_1916x751.png")]    // 1916x751 - 14 rows visible
        public void AlwaysReturns13Rows(string path)
        {
            using var screenshot = new Bitmap(path);
            using var crop = CropToStatMinBounds(screenshot);

            var rowCount = _detector.GetRowCount(crop);

            Assert.AreEqual(13, rowCount,
                $"Expected 13 rows for {path} ({screenshot.Width}x{screenshot.Height}), got {rowCount}");
        }

        // The detected row height * 13 should cover most of the image (but not all if 14th row is partially visible)
        [TestCase("./Resources/Screenshots/Screenshot_1.png")]           // 1920x1080
        [TestCase("./Resources/Screenshots/Screenshot_1920x1032.png")]   // 1920x1032
        [TestCase("./Resources/Screenshots/Screenshot_1920x1032_v2.png")]// 1920x1032
        [TestCase("./Resources/Screenshots/Screenshot_1466x823.png")]    // 1466x823
        [TestCase("./Resources/Screenshots/Screenshot_1916x904.png")]    // 1916x904
        [TestCase("./Resources/Screenshots/Screenshot_1916x751.png")]    // 1916x751
        public void RowHeightCovers13RowsWithinBounds(string path)
        {
            using var screenshot = new Bitmap(path);
            using var crop = CropToStatMinBounds(screenshot);

            var rowHeight = _detector.GetRowHeight(crop);
            var coveredHeight = rowHeight * 13;

            // 13 rows should cover between 85% and 100% of the image
            // (the remaining space is either empty or the partial 14th row)
            Assert.That(coveredHeight, Is.GreaterThan(crop.Height * 0.85),
                $"13 rows * {rowHeight:F1}px = {coveredHeight:F1}px should cover >85% of {crop.Height}px");
            Assert.That(coveredHeight, Is.LessThanOrEqualTo(crop.Height * 1.05),
                $"13 rows * {rowHeight:F1}px = {coveredHeight:F1}px should not exceed image height {crop.Height}px by >5%");
        }

        // Row height should differ across resolutions (proving we're detecting, not hardcoding)
        [Test]
        public void RowHeightDiffersAcrossResolutions()
        {
            using var large = new Bitmap("./Resources/Screenshots/Screenshot_1.png");         // 1920x1080
            using var small = new Bitmap("./Resources/Screenshots/Screenshot_1916x751.png");  // 1916x751

            using var largeCrop = CropToStatMinBounds(large);
            using var smallCrop = CropToStatMinBounds(small);

            var largeRowHeight = _detector.GetRowHeight(largeCrop);

            _detector = new RowSpacingDetector(); // fresh instance to avoid cache
            var smallRowHeight = _detector.GetRowHeight(smallCrop);

            Assert.That(largeRowHeight, Is.GreaterThan(smallRowHeight),
                $"Larger window should have taller rows: {largeRowHeight:F1} vs {smallRowHeight:F1}");
        }

        [Test]
        public void CacheReturnsSameResultForSameDimensions()
        {
            using var screenshot = new Bitmap("./Resources/Screenshots/Screenshot_1.png");
            using var crop = CropToStatMinBounds(screenshot);

            var first = _detector.GetRowCount(crop);
            var second = _detector.GetRowCount(crop);

            Assert.AreEqual(first, second);
        }

        [Test]
        public void CacheInvalidatesOnNewDimensions()
        {
            using var large = new Bitmap("./Resources/Screenshots/Screenshot_1.png");
            using var small = new Bitmap("./Resources/Screenshots/Screenshot_1916x751.png");

            using var largeCrop = CropToStatMinBounds(large);
            using var smallCrop = CropToStatMinBounds(small);

            var largeHeight = _detector.GetRowHeight(largeCrop);
            var smallHeight = _detector.GetRowHeight(smallCrop);

            Assert.AreNotEqual(largeHeight, smallHeight,
                "Cache should invalidate when image dimensions change");
        }
    }
}
