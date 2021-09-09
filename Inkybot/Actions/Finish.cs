using System;
using System.Windows.Forms;
using Inkybot.Dofus;
using Inkybot.Helpers;
using Inkybot.Services;
using Tesseract;
using DofusMagingJob = Inkybot.Contracts.DofusMagingJob;

namespace Inkybot.Actions
{
    public class Finish : InputAction
    {
        public readonly Item Item;
        public readonly MageHistoryRecord? LastHistoryRecord;

        public Finish(Control targetControl, Item item, MageHistoryRecord? lastHistoryRecord) : base(targetControl) =>
            (Item, LastHistoryRecord) = (item, lastHistoryRecord);
        
        public override void Execute() {
            var target = GetCursorTarget(Measurements.RemoveItemBounds);
            
            Input.DoubleClick(target.X, target.Y);
            var magus = (DofusMagingJob) Program.Services.GetService(typeof(DofusMagingJob));
            magus.StopMage();
        }
    }
}
