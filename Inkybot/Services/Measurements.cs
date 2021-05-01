using System.Collections.Generic;
using Inkybot.Contracts;
using Inkybot.Design;
using Inkybot.Helpers;
using Tesseract;

namespace Inkybot.Services
{
    public static class Measurements
    {
        const int StatLineCount = 13;

        private static UserSettingsConfigManager config = null!;

        public static void BindDependencies(ServiceContainer serviceContainer) {
            config = serviceContainer.GetService<UserSettingsConfigManager>();
        }

        public static IEnumerable<Responsive.Measurement> SplitBoundsToStatNumber(Responsive.Measurement measurement) {
            for (var i = 0; i < StatLineCount; i++) {
                var r = measurement.Rectangle;
                yield return new Responsive.Measurement {
                    Height = measurement.Height,
                    Width = measurement.Width,
                    Rectangle = new Rect(r.X1, r.Y1 + (r.Height / StatLineCount) * i, r.Width, r.Height / StatLineCount)
                };
            }
        }

        public static Responsive.Measurement RuneBoxBounds(int column, int row) {
            var x1 = 1100 + column * 51;
            var y1 = 306 + (int) (row * 38.7);
            var x2 = 1140 + column * 51;
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
            var n = StatLineCount;

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

        public static IEnumerable<Responsive.Measurement> RuneBoundsIndividualMeasurements {
            get {
                for (var i = 0; i < 3; i++) {
                    for (var j = 0; j < 13; j++) {
                        yield return RuneBoxBounds(i, j);
                    }
                }
            }
        }

        public static Responsive.Measurement[] StatMinBoundsIndividualLines =>
            SplitStatLineMeasurementsIntoIndividualLineMeasurements(StatMinBounds);
        
        public static Responsive.Measurement[] StatMaxBoundsIndividualLines =>
            SplitStatLineMeasurementsIntoIndividualLineMeasurements(StatMaxBounds);
        
        public static readonly Responsive.Measurement MagingTable = new Responsive.Measurement {
            Rectangle = Rect.FromCoords(230, 56, 1386, 1013),
            Width = 2050,
            Height = 1212
        };
        
        public static readonly Responsive.Measurement HistoryBounds = new Responsive.Measurement {
            Rectangle = Rect.FromCoords(346, 127, 628, 844),
            Width = 1920,
            Height = 1017
        };
        
        public static readonly Responsive.Measurement ShortHistoryBounds = new Responsive.Measurement {
            Rectangle = Rect.FromCoords(346, 127, 628, 322),
            Width = 1920,
            Height = 1017
        };

        public static readonly Responsive.Measurement StatValuesBounds = new Responsive.Measurement {
            Rectangle = Rect.FromCoords(800, 307, 1030, 805),
            Width = 1920,
            Height = 1017
        };

        public static readonly Responsive.Measurement StatMinBounds = new Responsive.Measurement {
            Rectangle = Rect.FromCoords(695, 307, 745, 805),
            Width = 1920,
            Height = 1017
        };
        
        public static readonly Responsive.Measurement StatMaxBounds = new Responsive.Measurement {
            Rectangle = Rect.FromCoords(745, 307, 800, 805),
            Width = 1920,
            Height = 1017
        };
        
        public static readonly Responsive.Measurement InventoryAverageItemValueBounds = new Responsive.Measurement {
            Rectangle = Rect.FromCoords(1435, 802, 1565, 825),
            Width = 1940,
            Height = 1110
        };
        
        public static Responsive.Measurement InventorySelectResourcesCategory =>
            !config.Temporis
                ? new Responsive.Measurement {
                    Rectangle = Rect.FromCoords(1530, 103, 1530, 103),
                    Width = 1920,
                    Height = 1017
                }
                : new Responsive.Measurement {
                    Rectangle = Rect.FromCoords(1500, 103, 1500, 103),
                    Width = 1920,
                    Height = 1017
                };
        
        public static readonly Responsive.Measurement InventoryFirstItemMeasurement = new Responsive.Measurement {
            Rectangle = Rect.FromCoords(1315, 186, 1315, 186),
            Width = 1920,
            Height = 1017
        };
        
        public static readonly Responsive.Measurement InventorySearchTextBox = new Responsive.Measurement {
            Rectangle = Rect.FromCoords(1360, 775, 1360, 775),
            Width = 1920,
            Height = 1017
        };
        
        public static readonly Responsive.Measurement InventorySearchTextBoxErase = new Responsive.Measurement {
            Rectangle = Rect.FromCoords(2100, 1165, 2100, 1165),
            Width = 2310,
            Height = 1530
        };
        
        public static readonly Responsive.Measurement CombineButtonMeasurement = new Responsive.Measurement {
            Rectangle = Rect.FromCoords(1050, 225, 1050, 225),
            Width = 1920,
            Height = 1017
        };
    }
}
