using System;
using System.Windows.Forms;
using Inkybot.Dofus;
using Inkybot.Services;

namespace Inkybot.Actions
{
    public class RemoveItemFromMagingTable : InputAction
    {
        public RemoveItemFromMagingTable(Control targetControl) : base(targetControl) {
        }
        
        public override void Execute() {
            var target = GetCursorTarget(Measurements.RemoveItemBounds);
            Input.DoubleClick(target.X, target.Y);
        }
    }
}
