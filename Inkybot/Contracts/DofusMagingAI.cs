using Inkybot.Domain.Repositories;

namespace Inkybot.Contracts
{
    public interface DofusMagingAI
    {
        IAction ResolveAction(ItemStatRepository itemStats, IAction previousAction);
        void SetConfig(Config config);
    }
}
