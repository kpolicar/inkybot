using System.Windows.Forms;
using Inkybot.Actions;
using Inkybot.Contracts;
using Inkybot.Domain;

namespace Inkybot.Services
{
    public class MouseActionFactory : ActionFactory
    {
        private Control targetControl = null!;

        public void SetRelativeToControl(Control targetControl) {
            this.targetControl = targetControl;
        }
        
        public IAction Finish(Item item) {
            return new Finish(targetControl, item);
        }

        public IAction CombineRune(Rune rune, bool exo) {
            return new CombineRune(targetControl, rune, exo);
        }

        public IAction InventorySelectResourcesAction() {
            return new InventorySelectResourcesAction(targetControl);
        }

        public IAction InventoryClearSelectionAction() {
            return new InventoryClearSelectionAction(targetControl);
        }
    }
}
