using System.Windows.Forms;
using Inkybot.Helpers;
using Tesseract;

namespace Inkybot.Actions
{
    public class Combine : MouseAction
    {
        public static readonly Responsive.Measurement FinishItemMeasurement = new Responsive.Measurement {
            Rectangle = Rect.FromCoords(1015, 225, 1015, 225),
            Width = 1920,
            Height = 1017
        };
        
        public Rune target;
        private Control targetControl;

        public Combine(Control targetControl, Rune target) : base(targetControl) {
            this.targetControl = targetControl;
            this.target = target;
        }

        public override void Execute() {
            var target = GetCursorTarget(FinishItemMeasurement);
            mouse.Click(target.X, target.Y);
        }
    }
}
