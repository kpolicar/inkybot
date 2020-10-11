namespace Inkybot.Contracts
{
    public interface DofusMagingAI
    {
        IAction ResolveAction(Item.ItemStat[] itemStats);
        void SetConfig(Config config);
    }
}
