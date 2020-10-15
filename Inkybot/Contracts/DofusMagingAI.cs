namespace Inkybot.Contracts
{
    public interface DofusMagingAI
    {
        IAction ResolveAction(Item.ItemStat[] itemStats, IAction previousAction);
        void SetConfig(Config config);
    }
}
