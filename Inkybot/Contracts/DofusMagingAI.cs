using Inkybot.Dofus;
using IAction = Inkybot.Domain.IAction;

namespace Inkybot.Contracts
{
    public interface DofusMagingAI
    {
        IAction ResolveAction(Item item);
    }
}
