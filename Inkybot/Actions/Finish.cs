using System.Windows.Forms;
using Inkybot.Domain;
using Inkybot.Helpers;
using Tesseract;
using DofusMagingJob = Inkybot.Contracts.DofusMagingJob;

namespace Inkybot.Actions
{
    public class Finish : InputAction
    {
        public static readonly Responsive.Measurement FinishItemMeasurement = new Responsive.Measurement {
            Rectangle = Rect.FromCoords(885, 165, 885, 165),
            Width = 1920,
            Height = 1017
        };

        public readonly Item Item;
        
        public Finish(Control targetControl, Item item) : base(targetControl) {
            Item = item;
        }

        public override void Execute() {
            var target = GetCursorTarget(FinishItemMeasurement);
            
            Input.DoubleClick(target.X, target.Y);
            var magus = (DofusMagingJob) Program.Services.GetService(typeof(DofusMagingJob));
            magus.StopMage();
        }

    }
}
