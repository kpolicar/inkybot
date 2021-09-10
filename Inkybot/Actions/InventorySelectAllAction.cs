using System.Windows.Forms;
using Inkybot.Services;

namespace Inkybot.Actions
{
    public class InventorySelectAllAction : InputAction
    {
        public InventorySelectAllAction(Control targetControl) : base(targetControl) {
        }

        public override void Execute() {
            var resourceCategoryPosition = GetCursorTarget(Measurements.InventorySelectAllCategory);
            Input.Click(resourceCategoryPosition.X, resourceCategoryPosition.Y);
        }
    }
}
