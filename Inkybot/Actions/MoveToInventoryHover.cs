using System.Threading;
using System.Windows.Forms;
using Inkybot.Services;

namespace Inkybot.Actions
{
    public class MoveToInventoryHover : InputAction
    {
        public MoveToInventoryHover(Control targetControl) : base(targetControl) { }

        public override void Execute() {
            var target = GetCursorTarget(Measurements.InventoryItemHoverInside);
            Input.Click(target.X, target.Y);
        }
    }
}
