using System;
using System.Drawing;

namespace Inkybot.Helpers
{
    public class RowSpacingDetector
    {
        private int _cachedWidth;
        private int _cachedHeight;
        private int _cachedRowCount;
        private double _cachedRowHeight;

        private const int MaxRows = 20;
        private const int MinRows = 10;
        private const int DefaultRows = 13;
        private const int DesiredRows = 13;

        // StatMinBounds at reference 1694x1009: y=314 to y=846
        private const int ReferenceStatColumnHeight = 846 - 314; // 532
        private const double DefaultReferenceRowHeight = 1.0 * ReferenceStatColumnHeight / DefaultRows; // ~40.92

        /// <summary>
        /// The detected row height converted to reference-space (1694x1009).
        /// Use this in Measurements instead of hardcoded row spacing values.
        /// </summary>
        public double ReferenceRowHeight { get; private set; } = DefaultReferenceRowHeight;

        public event EventHandler? RowHeightChanged;

        /// <summary>
        /// Detect row count from the image. Pass originalCropHeight if the image
        /// was resized during preprocessing (so the reference conversion is correct).
        /// </summary>
        public int GetRowCount(Bitmap image, int originalCropHeight = 0)
        {
            if (image.Width == _cachedWidth && image.Height == _cachedHeight)
                return _cachedRowCount;

            var rowHeight = DetectRowHeight(image);
            var rowCount = (int)Math.Round(1.0 * image.Height / rowHeight);

            if (rowCount < MinRows || rowCount > MaxRows)
                rowCount = DefaultRows;

            _cachedWidth = image.Width;
            _cachedHeight = image.Height;
            _cachedRowCount = Math.Min(rowCount, DesiredRows);
            _cachedRowHeight = rowHeight;

            // Convert pixel row height to reference-space using original crop dimensions
            var cropHeight = originalCropHeight > 0 ? originalCropHeight : image.Height;
            var pixelRowHeight = rowHeight * cropHeight / image.Height; // undo any preprocessing resize
            var newRefHeight = pixelRowHeight * ReferenceStatColumnHeight / cropHeight;
            if (Math.Abs(newRefHeight - ReferenceRowHeight) > 0.5)
            {
                ReferenceRowHeight = newRefHeight;
                RowHeightChanged?.Invoke(this, EventArgs.Empty);
            }

            return _cachedRowCount;
        }

        public double GetRowHeight(Bitmap image)
        {
            if (image.Width != _cachedWidth || image.Height != _cachedHeight)
                GetRowCount(image);

            return _cachedRowHeight;
        }

        private static double DetectRowHeight(Bitmap image)
        {
            int height = image.Height;
            int width = image.Width;

            // Compute average brightness per row (horizontal projection)
            var rowBrightness = new double[height];
            int sampleCount = Math.Min(width, 10);

            for (int y = 0; y < height; y++)
            {
                double sum = 0;
                for (int s = 0; s < sampleCount; s++)
                {
                    int x = (int)((s + 0.5) * width / sampleCount);
                    var pixel = image.GetPixel(x, y);
                    sum += (pixel.R + pixel.G + pixel.B) / 3.0;
                }
                rowBrightness[y] = sum / sampleCount;
            }

            // Subtract mean for autocorrelation
            double mean = 0;
            for (int y = 0; y < height; y++) mean += rowBrightness[y];
            mean /= height;

            for (int y = 0; y < height; y++) rowBrightness[y] -= mean;

            // Find the row period via autocorrelation
            // Search range: image could have 10-20 rows
            int minPeriod = Math.Max(1, height / MaxRows);
            int maxPeriod = height / MinRows;

            double bestCorr = double.MinValue;
            int bestPeriod = height / DefaultRows;

            for (int p = minPeriod; p <= maxPeriod; p++)
            {
                double corr = 0;
                int n = height - p;
                for (int y = 0; y < n; y++)
                {
                    corr += rowBrightness[y] * rowBrightness[y + p];
                }
                corr /= n;

                if (corr > bestCorr)
                {
                    bestCorr = corr;
                    bestPeriod = p;
                }
            }

            return bestPeriod;
        }
    }
}
