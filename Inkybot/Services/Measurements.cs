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
        private static RowSpacingDetector rowSpacingDetector = null!;

        public static void BindDependencies(ServiceContainer serviceContainer) {
            config = serviceContainer.GetService<UserSettingsConfigManager>();
            rowSpacingDetector = serviceContainer.GetService<RowSpacingDetector>();
        }

        /// <summary>
        /// Returns the full bounds clamped to exactly 13 rows height.
        /// </summary>
        public static Responsive.Measurement StatColumnBounds(Responsive.Measurement measurement) {
            var rowHeight = rowSpacingDetector?.ReferenceRowHeight ?? (1.0 * measurement.Rectangle.Height / StatLineCount);
            var r = measurement.Rectangle;
            return new Responsive.Measurement {
                Height = measurement.Height,
                Width = measurement.Width,
                Rectangle = new Rect(r.X1, r.Y1, r.Width, (int)(rowHeight * StatLineCount))
            };
        }

        public static IEnumerable<Responsive.Measurement> SplitBoundsToStatNumber(Responsive.Measurement measurement) {
            var rowHeight = rowSpacingDetector?.ReferenceRowHeight ?? (1.0 * measurement.Rectangle.Height / StatLineCount);
            for (var i = 0; i < StatLineCount; i++) {
                var r = measurement.Rectangle;
                yield return new Responsive.Measurement {
                    Height = measurement.Height,
                    Width = measurement.Width,
                    Rectangle = new Rect(r.X1, 314 + (int)(rowHeight * i), r.Width, (int)rowHeight)
                };
            }
        }

        public static Responsive.Measurement RuneBoxBounds(int column, int row, int inset = 0) {
            var rowHeight = rowSpacingDetector?.ReferenceRowHeight ?? 40.5;
            var x1 = 910 + column * 41 + inset;
            var y1 = 314 + (int)(row * rowHeight) + inset;

            return new Responsive.Measurement {
                Rectangle = new Rect(x1, y1, 938 - 910 - inset * 2, (int)rowHeight - inset * 2),
                Width = 1694,
                Height = 1009
            };
        }

        public static Responsive.Measurement InventoryBoxBounds(int column, int row) {
            var x1 = 1146 + column * 60;
            var y1 = (int)(273 + row * 60.5);
            var x2 =  (int)(1201 + column * 60.5);
            var y2 = 333 + row * 60;

            return new Responsive.Measurement {
                Rectangle = Rect.FromCoords(x1, y1, x2, y2),
                Width = 1694,
                Height = 1009
            };
        }

        public static Responsive.Measurement[] SplitStatLineMeasurementsIntoIndividualLineMeasurements(
            Responsive.Measurement measurement) {
            var b = measurement.Rectangle;
            var n = StatLineCount;
            var rowHeight = rowSpacingDetector?.ReferenceRowHeight ?? (1.0 * b.Height / n);

            var measurements = new Responsive.Measurement[n];
            for (int i = 0; i < n; ++i) {
                var smallerRect = new Rect(b.X1, b.Y1 + (int)(rowHeight * i), b.Width / 3, (int)rowHeight);
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
                for (var i = 0; i < 8; i++) {
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
            Rectangle = Rect.FromCoords(230, 157, 1046, 851),
            Width = 1694,
            Height = 1009
        };
        
        public static Responsive.Measurement MagingTableCharacterDetails => new Responsive.Measurement {
            Rectangle = Rect.FromCoords(238, 200, 455, 272),
            Width = 1694,
            Height = 1009
        };
        
        public static Responsive.Measurement RemoveItemBounds => new Responsive.Measurement {
            Rectangle = Rect.FromCoords(643, 218, 643, 218),
            Width = 1694,
            Height = 1009
        };
        
        public static Responsive.Measurement HistoryBounds => new Responsive.Measurement {
            Rectangle = Rect.FromCoords(285, 282, 440, 815),
            Width = 1694,
            Height = 1009
        };
        
        public static Responsive.Measurement ShortHistoryBounds => new Responsive.Measurement {
            Rectangle = Rect.FromCoords(285, 282, 440, 815),
            Width = 1694,
            Height = 1009
        };

        public static Responsive.Measurement StatValuesBounds => new Responsive.Measurement {
            Rectangle = Rect.FromCoords(633, 314, 822, 846),
            Width = 1694,
            Height = 1009
        };

        public static Responsive.Measurement StatMinBounds => new Responsive.Measurement {
            Rectangle = Rect.FromCoords(494, 314, 525, 846),
            Width = 1694,
            Height = 1009
        };

        public static Responsive.Measurement StatMaxBounds => new Responsive.Measurement {
            Rectangle = Rect.FromCoords(552, 314, 585, 846),
            Width = 1694,
            Height = 1009
        };
        
        public static Responsive.Measurement InventoryAverageItemValueBounds => new Responsive.Measurement {
            Rectangle = Rect.FromCoords(1355, 795, 1431, 814),
            Width = 1694,
            Height = 1009
        };
        
        public static Responsive.Measurement InventoryItemHoverInside => new Responsive.Measurement {
            Rectangle = Rect.FromCoords(1453, 545, 1453, 545),
            Width = 1694,
            Height = 1009
        };

        public static Responsive.Measurement InventorySelectResourcesCategory => new Responsive.Measurement {
            Rectangle = Rect.FromCoords(1101, 342, 1101, 342),
            Width = 1694,   
            Height = 1009
        };

        public static Responsive.Measurement InventorySelectEquipmentCategory => new Responsive.Measurement {
            Rectangle = Rect.FromCoords(1099, 258, 1099, 258),
            Width = 1694,
            Height = 1009
        };

        public static Responsive.Measurement InventorySelectAllCategory => new Responsive.Measurement {
            Rectangle = Rect.FromCoords(1098, 215, 1098, 215),
            Width = 1694,
            Height = 1009
        };

        // public static Responsive.Measurement InventorySelectResourcesCategoryBox => new Responsive.Measurement {
        //     Rectangle = Rect.FromCoords(1475, 90, 1550, 115),
        //     Width = 1925,
        //     Height = 1014
        // };
        
        public static Responsive.Measurement InventoryFirstItemMeasurement => new Responsive.Measurement {
            Rectangle = Rect.FromCoords(1171, 324, 1171, 324),
            Width = 1694,
            Height = 1009
        };
        
        // public static Responsive.Measurement InventoryFirstItemBoxMeasurement => new Responsive.Measurement {
        //     Rectangle = Rect.FromCoords(1392, 177, 1452, 238),
        //     Width = 2035,
        //     Height = 1150
        // };
        
        public static Responsive.Measurement InventorySearchTextBoxSelectMeasurement => new Responsive.Measurement {
            Rectangle = Rect.FromCoords(1205, 212, 1205, 212),
            Width = 1694,
            Height = 1009
        };
        
        public static Responsive.Measurement SinkMeasurement => new Responsive.Measurement {
            Rectangle = Rect.FromCoords(569, 205, 603, 232),
            Width = 1694,
            Height = 1009
        };
        
        // Seems to be different on FR
        public static Responsive.Measurement SinkFrMeasurement => new Responsive.Measurement {
            Rectangle = Rect.FromCoords(582, 205, 603, 232),
            Width = 1694,
            Height = 1009
        };
        
        public static Responsive.Measurement InventorySearchTextBox=> new Responsive.Measurement {
            Rectangle = Rect.FromCoords(1141, 200, 1284, 222),
            Width = 1694,
            Height = 1009
        };
        
        public static Responsive.Measurement InventorySearchTextBoxErase => new Responsive.Measurement {
            Rectangle = Rect.FromCoords(1271, 211, 1271, 211),
            Width = 1694,
            Height = 1009
        };
        
        public static Responsive.Measurement CombineButtonMeasurement => new Responsive.Measurement {
            Rectangle = Rect.FromCoords(730, 251, 730, 251),
            Width = 1694,
            Height = 1009
        };
        
        // public static Responsive.Measurement CombineButtonBoxMeasurement => new Responsive.Measurement {
        //     Rectangle = Rect.FromCoords(957, 209, 1150, 235),
        //     Width = 1925,
        //     Height = 1014
        // };
    }
}
