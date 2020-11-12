using Inkybot.Domain;
using Inkybot.Domain.Repositories;

namespace Inkybot.Contracts
{
    public interface DofusMagingAI
    {
        IAction ResolveAction(Item item, IAction previousAction);
    }
}
