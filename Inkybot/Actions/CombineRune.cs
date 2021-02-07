using System;
using System.Drawing;
using System.Threading;
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

            for (var row = 0; row < itemStats.Length; row++) {
                if (Rune.Stat != itemStats[row].Stat)
                    continue;

                var pos = RunePosition(column, row);
                
                Input.Click(pos.X, pos.Y);
                System.Diagnostics.Debug.WriteLine($"Rune changed to {Rune.Stat.ToString()}");
                return;
            }

            var resourceCategoryPosition = GetCursorTarget(Measurements.InventorySelectResourcesCategory);
            Input.Click(resourceCategoryPosition.X, resourceCategoryPosition.Y);

            Thread.Sleep(50);
            
            
            var searchTextBoxPosition = GetCursorTarget(Measurements.InventorySearchTextBox);
            Input.Click(searchTextBoxPosition.X, searchTextBoxPosition.Y);
            Thread.Sleep(500);
            
            Cancel?.ThrowIfCancellationRequested();
            Input.SelectAll();
            Thread.Sleep(50);

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
            var combineButtonPosition = GetCursorTarget(Measurements.CombineButtonMeasurement);;
            Input.Click(combineButtonPosition.X, combineButtonPosition.Y);
            
            System.Diagnostics.Debug.WriteLine($"EXO Rune changed to {Rune.Stat.DisplayName}");
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
        
        public override string ToString() {
            return $"Combine: {Rune}";
        }
    }
}
