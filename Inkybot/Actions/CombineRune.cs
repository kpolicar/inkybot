using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Inkybot.Domain;
using Inkybot.Helpers;
using Inkybot.Services;
using Rect = Tesseract.Rect;

namespace Inkybot.Actions
{
    public class CombineRune : InputAction, RuneAction
    {
        public static readonly Responsive.Measurement CombineButtonMeasurement = new Responsive.Measurement {
            Rectangle = Rect.FromCoords(1050, 225, 1050, 225),
            Width = 1920,
            Height = 1017
        };
        
        public bool SelectedExoRune;
        
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
        
        public static readonly Responsive.Measurement FirstItemInInventoryMeasurement = new Responsive.Measurement {
            Rectangle = Rect.FromCoords(1315, 186, 1315, 186),
            Width = 1920,
            Height = 1017
        };
        
        public static readonly Responsive.Measurement SelectRuneMeasurement = new Responsive.Measurement {
            Rectangle = Rect.FromCoords(1052, 307, 1186, 812),
            Width = 1920,
            Height = 1017
        };
        
        public Rune Rune { get; private set; }
        private Control targetControl;
        public readonly bool Exo;

        public CombineRune(Control targetControl, Rune rune, bool exo) : base(targetControl) {
            this.targetControl = targetControl;
            Rune = rune;
            Exo = exo;
        }

        public CombineRune(Control targetControl) : base(targetControl) {
            this.targetControl = targetControl;
        }

        public void Test() {
            for (int i = 0; i < 3; i++) {
                for (int j = 0; j < 13; j++) {
                    
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
                
                System.Diagnostics.Debug.WriteLine(pos);
                Input.Click(pos.X, pos.Y);
                System.Diagnostics.Debug.WriteLine($"Rune changed to {Rune.stat.DisplayName}");
                return;
            }

            var resourceCategoryPosition = GetCursorTarget(Measurements.InventorySelectResourcesCategory);
            Input.Click(resourceCategoryPosition.X, resourceCategoryPosition.Y);

            Thread.Sleep(50);
            
            
            var searchTextBoxPosition = GetCursorTarget(InventorySearchTextBox);
            Input.Click(searchTextBoxPosition.X, searchTextBoxPosition.Y);
            Thread.Sleep(500);
            
            Cancel?.ThrowIfCancellationRequested();
            Input.SelectAll();
            Thread.Sleep(50);

            Input.TypeMessage(Rune.DisplayName, Cancel);
            Thread.Sleep(2000);
            
            Cancel?.ThrowIfCancellationRequested();
            var targetRunePosition = GetCursorTarget(FirstItemInInventoryMeasurement);
            Input.DoubleClick(targetRunePosition.X, targetRunePosition.Y);
            SelectedExoRune = true;
            Thread.Sleep(1000);
            
            Cancel?.ThrowIfCancellationRequested();
            var eraseSearchPosition = GetCursorTarget(InventorySearchTextBoxErase);
            Input.Click(eraseSearchPosition.X, eraseSearchPosition.Y);
            
            
            Thread.Sleep(500);
            
            Cancel?.ThrowIfCancellationRequested();
            var combineButtonPosition = GetCursorTarget(CombineButtonMeasurement);;
            Input.Click(combineButtonPosition.X, combineButtonPosition.Y);
            
            System.Diagnostics.Debug.WriteLine($"EXO Rune changed to {Rune.stat.DisplayName}");
        }
        
        private Point RunePosition(int column, int row) {
            var x = 1110 + column * 51;
            var y = 318 + (int)(row * 38.7);
            
            var measurement = new Responsive.Measurement {
                Rectangle = Rect.FromCoords(x, y, x, y),
                Width = 1920,
                Height = 1017
            };
            
            return GetCursorTarget(measurement);
        }
    }
}
