using System.Windows.Forms;
using Inkybot.Helpers;
using Tesseract;

namespace Inkybot.Actions
{
    public class Finish : MouseAction
    {
        public static readonly Responsive.Measurement FinishItemMeasurement = new Responsive.Measurement {
            Rectangle = Rect.FromCoords(850, 165, 850, 165),
            Width = 1920,
            Height = 1017
        };
        
        public Finish(Control targetControl) : base(targetControl) {
        }

        public override void Execute() {
            var target = GetCursorTarget(FinishItemMeasurement);
            
            mouse.DoubleClick(target.X, target.Y);
            var magus = (DofusMagingJob) Program.Services.GetService(typeof(DofusMagingJob));
            magus.StopMage();
        }

    }
}
