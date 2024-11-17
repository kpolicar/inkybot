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
            var x1 = 900 + column * 41;
            var y1 = 316 + (int) (row * 40);
            var x2 = 927 + column * 41;
            var y2 = 342 + (int) (row * 40);

            return new Responsive.Measurement {
                Rectangle = Rect.FromCoords(x1, y1, x2, y2),
                Width = 1694,
                Height = 1009
            };
        }

        public static Responsive.Measurement InventoryBoxBounds(int column, int row) {
            var x1 = 1259 + column * 70;
            var y1 = 182 + row * 70;
            var x2 = 1327 + column * 70;
            var y2 = 249 + row * 70;

            return new Responsive.Measurement {
                Rectangle = Rect.FromCoords(x1, y1, x2, y2),
                Width = 1744,
                Height = 1189
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

        public static IEnumerable<Responsive.Measurement> InventoryBoundsIndividualMeasurements {
            get {
                for (var i = 0; i < 9; i++) {
                    for (var j = 0; j < 5; j++) {
                        yield return InventoryBoxBounds(j, i);
                    }
                }
            }
        }

        public static Responsive.Measurement[] StatMinBoundsIndividualLines =>
            SplitStatLineMeasurementsIntoIndividualLineMeasurements(StatMinBounds);
        
        public static Responsive.Measurement[] StatMaxBoundsIndividualLines =>
            SplitStatLineMeasurementsIntoIndividualLineMeasurements(StatMaxBounds);
        
        public static Responsive.Measurement MagingTable => new Responsive.Measurement {
            Rectangle = Rect.FromCoords(230, 56, 1386, 1013),
            Width = 2050,
            Height = 1212
        };
        
        public static Responsive.Measurement RemoveItemBounds => new Responsive.Measurement {
            Rectangle = Rect.FromCoords(885, 165, 885, 165),
            Width = 1920,
            Height = 1017
        };
        
        public static Responsive.Measurement HistoryBounds => new Responsive.Measurement {
            Rectangle = Rect.FromCoords(285, 275, 284, 812),
            Width = 1694,
            Height = 1009
        };
        
        public static Responsive.Measurement ShortHistoryBounds => new Responsive.Measurement {
            Rectangle = Rect.FromCoords(346, 127, 628, 322),
            Width = 1920,
            Height = 1017
        };

        public static Responsive.Measurement StatValuesBounds => new Responsive.Measurement {
            Rectangle = Rect.FromCoords(622, 320, 814, 818),
            Width = 1694,
            Height = 1009
        };

        public static Responsive.Measurement StatMinBounds => new Responsive.Measurement {
            Rectangle = Rect.FromCoords(486, 320, 514, 818),
            Width = 1694,
            Height = 1009
        };
        
        public static Responsive.Measurement StatMaxBounds => new Responsive.Measurement {
            Rectangle = Rect.FromCoords(544, 320, 571, 818),
            Width = 1694,
            Height = 1009
        };
        
        public static Responsive.Measurement InventoryAverageItemValueBounds => new Responsive.Measurement {
            Rectangle = Rect.FromCoords(1310, 796, 1380, 808),
            Width = 1694,
            Height = 1009
        };

        public static Responsive.Measurement InventorySelectResourcesCategory => new Responsive.Measurement {
            Rectangle = Rect.FromCoords(1512, 103, 1512, 103),
            Width = 1920,
            Height = 1017
        };

        public static Responsive.Measurement InventorySelectEquipmentCategory => new Responsive.Measurement {
            Rectangle = Rect.FromCoords(1062, 100, 1062, 100),
            Width = 1269,
            Height = 965
        };

        public static Responsive.Measurement InventorySelectAllCategory => new Responsive.Measurement {
            Rectangle = Rect.FromCoords(1021, 100, 1021, 100),
            Width = 1269,
            Height = 965
        };

        public static Responsive.Measurement InventorySelectResourcesCategoryBox => new Responsive.Measurement {
            Rectangle = Rect.FromCoords(1475, 90, 1550, 115),
            Width = 1925,
            Height = 1014
        };
        
        public static Responsive.Measurement InventoryFirstItemMeasurement => new Responsive.Measurement {
            Rectangle = Rect.FromCoords(1315, 186, 1315, 186),
            Width = 1920,
            Height = 1017
        };
        
        public static Responsive.Measurement InventoryFirstItemBoxMeasurement => new Responsive.Measurement {
            Rectangle = Rect.FromCoords(1392, 177, 1452, 238),
            Width = 2035,
            Height = 1150
        };
        
        public static Responsive.Measurement InventorySearchTextBoxSelectMeasurement => new Responsive.Measurement {
            Rectangle = Rect.FromCoords(1356, 210, 1356, 210),
            Width = 1694,
            Height = 1009
        };
        
        public static Responsive.Measurement InventorySearchTextBox=> new Responsive.Measurement {
            Rectangle = Rect.FromCoords(1131, 200, 1274, 222),
            Width = 1694,
            Height = 1009
        };
        
        public static Responsive.Measurement InventorySearchTextBoxErase => new Responsive.Measurement {
            Rectangle = Rect.FromCoords(2100, 1165, 2100, 1165),
            Width = 2310,
            Height = 1530
        };
        
        public static Responsive.Measurement CombineButtonMeasurement => new Responsive.Measurement {
            Rectangle = Rect.FromCoords(1050, 225, 1050, 225),
            Width = 1920,
            Height = 1017
        };
        
        public static Responsive.Measurement CombineButtonBoxMeasurement => new Responsive.Measurement {
            Rectangle = Rect.FromCoords(957, 209, 1150, 235),
            Width = 1925,
            Height = 1014
        };
    }
}
