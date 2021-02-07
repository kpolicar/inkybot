using System.Windows.Forms;
using Inkybot.Actions;
using Inkybot.Contracts;
using Inkybot.Design;
using Inkybot.Dofus;
using Inkybot.Dofus.Contracts;
using Inkybot.Dofus.Domain;

namespace Inkybot.Services
{
    public class MouseActionFactory : ActionFactory, HasDependencies
    {
        private Control targetControl = null!;

        public void BindDependencies(ServiceContainer serviceContainer) {
        }
        
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
