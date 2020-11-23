using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Inkybot.Domain;
using Inkybot.Helpers;
using Rect = Tesseract.Rect;

namespace Inkybot.Actions
{
    public class SelectRune : InputAction, RuneAction
    {
        public bool SelectedExoRune;
        
        public static readonly Responsive.Measurement InventorySelectResourcesCategory = new Responsive.Measurement {
            Rectangle = Rect.FromCoords(1492, 103, 1492, 103),
            Width = 1920,
            Height = 1017
        };
        
        public static readonly Responsive.Measurement InventorySearchTextBox = new Responsive.Measurement {
            Rectangle = Rect.FromCoords(1320, 775, 1320, 775),
            Width = 1920,
            Height = 1017
        };
        public static readonly Responsive.Measurement FirstItemInInventoryMeasurement = new Responsive.Measurement {
            Rectangle = Rect.FromCoords(1284, 186, 1284, 186),
            Width = 1920,
            Height = 1017
        };
        
        public static readonly Responsive.Measurement SelectRuneMeasurement = new Responsive.Measurement {
            Rectangle = Rect.FromCoords(1052, 307, 1186, 812),
            Width = 1920,
            Height = 1017
        };
        
        public ItemStat Target { get; set; }
        public Rune Rune { get; private set; }
        private Control targetControl;

        public SelectRune(Control targetControl, Rune rune) : base(targetControl) {
            this.targetControl = targetControl;
            Rune = rune;
        }

        public SelectRune(Control targetControl) : base(targetControl) {
            this.targetControl = targetControl;
        }

        public void Test() {
            for (int i = 0; i < 3; i++) {
                for (int j = 0; j < 14; j++) {
                    
                    var pos = RunePosition(i, j);
                    Cursor.Position = targetControl.PointToScreen(new Point(pos.X, pos.Y));
                    Thread.Sleep(200);
                    
                }
            }
        }
        
        public override void Execute() {
            
            var itemStats = screenDataProvider.previousScannedItem.Stats;
            var column = (int) Rune.type;

            for (var row = 0; row < itemStats.Length; row++) {
                if (Rune.stat != itemStats[row].stat)
                    continue;

                var pos = RunePosition(column, row);
                Input.CtrlDoubleClick(pos.X, pos.Y);
                System.Diagnostics.Debug.WriteLine($"Rune changed to {Rune.stat.DisplayName}");
                return;
            }

            var resourceCategoryPosition = GetCursorTarget(new Responsive.Measurement {
                Rectangle = Rect.FromCoords(1492, 103, 1492, 103),
                Width = 1920,
                Height = 1017
            });
            Input.Click(resourceCategoryPosition.X, resourceCategoryPosition.Y);

            Thread.Sleep(50);
            
            var searchTextBoxPosition = GetCursorTarget(InventorySearchTextBox);
            Input.Click(searchTextBoxPosition.X, searchTextBoxPosition.Y);
            
            Thread.Sleep(500);
            Input.SelectAll();
            Thread.Sleep(50);
            
            Input.TypeMessage(Rune.DisplayName);
            Thread.Sleep(2000);
            
            var targetRunePosition = GetCursorTarget(FirstItemInInventoryMeasurement);
            Input.DoubleClick(targetRunePosition.X, targetRunePosition.Y);
            SelectedExoRune = true;
            
            System.Diagnostics.Debug.WriteLine($"EXO Rune changed to {Rune.stat.DisplayName}");
        }
        
        private Point RunePosition(int column, int row) {
            var x = 1065 + column * 55;
            var y = 320 + row * 39;
            
            var measurement = new Responsive.Measurement {
                Rectangle = Rect.FromCoords(x, y, x, y),
                Width = 1920,
                Height = 1017
            };
            
            return GetCursorTarget(measurement);
        }
    }
}
