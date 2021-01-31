using Inkybot.Dofus;
using Inkybot.Dofus.Domain;

namespace Inkybot.Dofus.Contracts
{
    public abstract class DofusMagingAI
    {
        protected ActionFactory Action {
            get;
            private set;
        }

        public void AddServices(ActionFactory actions) =>
            Action = actions;

        public virtual void Init() {
        }
        
        public abstract IAction ResolveAction(Item item);
    }
}
