using System.Threading;
using System.Windows.Forms;
using Inkybot.Services;

namespace Inkybot.Actions
{
    public class InventoryClearSelectionAction : InputAction
    {
        public InventoryClearSelectionAction(Control targetControl) : base(targetControl) {
        }

        public override void Execute() {
            var resourceCategoryPosition = GetCursorTarget(Measurements.InventorySearchTextBoxErase);
            Input.Click(resourceCategoryPosition.X, resourceCategoryPosition.Y);
        }
    }
}
