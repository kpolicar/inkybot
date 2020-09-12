using System.Collections.Generic;

namespace WindowsFormsApp.Contracts
{
    public interface DofusMagingAI
    {
        IAction ResolveAction(Item.ItemStat[] itemStats);
        void SetHistory(List<IAction> history);
    }
}