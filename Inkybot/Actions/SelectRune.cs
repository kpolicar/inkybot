using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Inkybot.Helpers;
using Rect = Tesseract.Rect;

namespace Inkybot.Actions
{
    public class SelectRune : MouseAction
    {
        public static readonly Responsive.Measurement SelectRuneMeasurement = new Responsive.Measurement {
            Rectangle = Rect.FromCoords(1052, 307, 1186, 812),
            Width = 1920,
            Height = 1017
        };
        
        public ItemStat Target { get; set; }
        private readonly Rune rune;
        private Control targetControl;

        public SelectRune(Control targetControl, Rune rune) : base(targetControl) {
            this.targetControl = targetControl;
            this.rune = rune;
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
            
            var itemStats = screenDataProvider.lastScanResults;
            var column = (int) rune.type;

            for (var row = 0; row < itemStats.Length; row++) {
                if (rune.stat.DisplayName != itemStats[row].stat.DisplayName)
                    continue;

                var pos = RunePosition(column, row);
                mouse.CtrlDoubleClick(pos.X, pos.Y);
                System.Diagnostics.Debug.WriteLine($"Rune changed to {rune.stat.DisplayName}");
            }
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
