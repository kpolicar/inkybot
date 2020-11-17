using Inkybot.Helpers;
using Tesseract;

namespace Inkybot.Services
{
    public static class Measurements
    {

        public static Responsive.Measurement RuneBoxBounds(int column, int row) {
            var x1 = 1050 + column * 55;
            var y1 = 305 + (int) (row * 38.7);
            var x2 = 1090 + column * 55;
            var y2 = 336 + (int) (row * 38.7);

            return new Responsive.Measurement {
                Rectangle = Rect.FromCoords(x1, y1, x2, y2),
                Width = 1920,
                Height = 1017
            };
        }

        public static Responsive.Measurement[] SplitStatLineMeasurementsIntoIndividualLineMeasurements(
            Responsive.Measurement measurement) {
            var b = measurement.Rectangle;
            var n = 14; // number of stat lines

            var measurements = new Responsive.Measurement[n];
            for (int i = 0; i < n; ++i) {
                var smallerRect = new Rect(b.X1, b.Y1 + (int) (1f * b.Height / n * i), b.Width / 3, b.Height / n);
                var m = new Responsive.Measurement {
                    Rectangle = smallerRect,
                    Height = measurement.Height,
                    Width = measurement.Width
                };
                measurements[i] = m;
            }

            return measurements;
        }

        public static Responsive.Measurement[] RuneBoundsIndividualMeasurements {
            get {
                var measurements = new Responsive.Measurement[3*14];
                for (int i = 0; i < 3; i++) {
                    for (int j = 0; j < 14; j++) {
                        var measurement = RuneBoxBounds(i, j);
                        measurements[i * 10 + j] = measurement;
                    }
                }
                return measurements;
            }
        }

        public static Responsive.Measurement[] StatMinBoundsIndividualLines =>
            SplitStatLineMeasurementsIntoIndividualLineMeasurements(StatMinBounds);
        
        public static Responsive.Measurement[] StatMaxBoundsIndividualLines =>
            SplitStatLineMeasurementsIntoIndividualLineMeasurements(StatMaxBounds);
        
        public static readonly Responsive.Measurement HistoryBounds = new Responsive.Measurement {
            Rectangle = Rect.FromCoords(346, 117, 590, 844),
            Width = 1920,
            Height = 1017
        };
        
        public static readonly Responsive.Measurement ShortHistoryBounds = new Responsive.Measurement {
            Rectangle = Rect.FromCoords(346, 117, 590, 844),
            Width = 1920,
            Height = 1017
        };

        public static readonly Responsive.Measurement StatValuesBounds = new Responsive.Measurement {
            Rectangle = Rect.FromCoords(745, 307, 973, 842),
            Width = 1920,
            Height = 1017
        };

        public static readonly Responsive.Measurement StatMinBounds = new Responsive.Measurement {
            Rectangle = Rect.FromCoords(645, 307, 695, 842),
            Width = 1920,
            Height = 1017
        };
        
        public static readonly Responsive.Measurement StatMaxBounds = new Responsive.Measurement {
            Rectangle = Rect.FromCoords(695, 307, 745, 842),
            Width = 1920,
            Height = 1017
        };
    }
}
