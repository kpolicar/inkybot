using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Inkybot.Dofus;
using Inkybot.Domain;
using Inkybot.Helpers;
using Inkybot.Services;
using Rect = Tesseract.Rect;

namespace Inkybot.Actions
{
    public class CombineRune : InputAction, RuneAction
    {
        public Rune Rune { get; private set; }
        public readonly bool Exo;

        public CombineRune(Control targetControl, Rune rune, bool exo) : base(targetControl) {
            this.targetControl = targetControl;
            Rune = rune;
            Exo = exo;
        }
        
        public override void Execute() {
            var itemStats = screenDataProvider.previousScannedItem!.Stats;
            var column = (int) Rune.Type;

            var combineButtonPosition = GetCursorTarget(Measurements.CombineButtonMeasurement);
            for (var row = 0; row < itemStats.Length; row++) {
                if (Rune.Stat != itemStats[row].Stat)
                    continue;

                var pos = RunePosition(column, row);
                
                Input.Click(pos.X, pos.Y);
                Thread.Sleep(5);
                return;
            }

            var resourceCategoryPosition = GetCursorTarget(Measurements.InventorySelectResourcesCategory);
            Input.Click(resourceCategoryPosition.X, resourceCategoryPosition.Y);

            Thread.Sleep(300);
            
            
            var searchTextBoxPosition = GetCursorTarget(Measurements.InventorySearchTextBoxSelectMeasurement);
            Input.Click(searchTextBoxPosition.X, searchTextBoxPosition.Y);
            Thread.Sleep(500);
            
            Cancel?.ThrowIfCancellationRequested();
            Input.TypeMessage(Rune.DisplayName, Cancel);
            Thread.Sleep(2000);
            
            Cancel?.ThrowIfCancellationRequested();
            var targetRunePosition = GetCursorTarget(Measurements.InventoryFirstItemMeasurement);
            Input.DoubleClick(targetRunePosition.X, targetRunePosition.Y);
            Thread.Sleep(1000);
            
            Cancel?.ThrowIfCancellationRequested();
            var eraseSearchPosition = GetCursorTarget(Measurements.InventorySearchTextBoxErase);
            Input.Click(eraseSearchPosition.X, eraseSearchPosition.Y);
            
            
            Thread.Sleep(500);
            
            Cancel?.ThrowIfCancellationRequested();
            Input.Click(combineButtonPosition.X, combineButtonPosition.Y);
        }
        
        private Point RunePosition(int column, int row) {
            var x1 = 910 + column * 41;
            var y1 = 326 + (int) (row * 40);
            
            var measurement = new Responsive.Measurement {
                Rectangle = Rect.FromCoords(x1,y1,x1,y1),
                Width = 1694,
                Height = 1009
            };
            
            return GetCursorTarget(measurement);
        }
        
        public override string ToString() {
            return $"Combine: {Rune}";
        }
    }
}
