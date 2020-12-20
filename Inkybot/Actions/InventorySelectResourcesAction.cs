using System.Threading;
using System.Windows.Forms;
using Inkybot.Services;

namespace Inkybot.Actions
{
    public class InventorySelectResourcesAction : InputAction
    {
        public InventorySelectResourcesAction(Control targetControl) : base(targetControl) {
        }

        public override void Execute() {
            var resourceCategoryPosition = GetCursorTarget(Measurements.InventorySelectResourcesCategory);
            Input.Click(resourceCategoryPosition.X, resourceCategoryPosition.Y);
        }
    }
}
