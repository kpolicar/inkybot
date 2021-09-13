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
        private DofusMagingJob magingJob = null!;
        private MageQueueManager queueManager;

        public void BindDependencies(ServiceContainer serviceContainer) {
            magingJob = serviceContainer.GetService<DofusMagingJob>();
            queueManager = serviceContainer.GetService<MageQueueManager>();
        }
        
        public void SetRelativeToControl(Control targetControl) {
            this.targetControl = targetControl;
        }
        
        public IAction Finish(Item item) {
            return new Finish(targetControl, item, magingJob.LastHistoryRecord);
        }
        
        public IAction SelectItemFromQueue() {
            return new SelectEnqueuedItem(queueManager.ApplyHead(), targetControl);
        }

        public IAction RemoveItemFromMagingTable() {
            return new RemoveItemFromMagingTable(targetControl);
        }

        public IAction CombineRune(Rune rune, bool exo) {
            return new CombineRune(targetControl, rune, exo);
        }

        public IAction InventorySelectAllAction() {
            return new InventorySelectAllAction(targetControl);
        }

        public IAction InventorySelectResourcesAction() {
            return new InventorySelectResourcesAction(targetControl);
        }

        public IAction InventorySelectEquipmentAction() {
            return new InventorySelectEquipmentAction(targetControl);
        }

        public IAction InventoryClearSelectionAction() {
            return new InventoryClearSelectionAction(targetControl);
        }
    }
}
