using System.Windows.Forms;
using Inkybot.Services;

namespace Inkybot.Actions
{
    public class InventorySelectEquipmentAction : InputAction
    {
        public InventorySelectEquipmentAction(Control targetControl) : base(targetControl) {
        }

        public override void Execute() {
            var resourceCategoryPosition = GetCursorTarget(Measurements.InventorySelectEquipmentCategory);
            Input.Click(resourceCategoryPosition.X, resourceCategoryPosition.Y);
        }
    }
}
