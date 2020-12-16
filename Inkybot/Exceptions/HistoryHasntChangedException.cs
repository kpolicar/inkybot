using System;

namespace Inkybot.Exceptions
{
    public class HistoryHasntChangedException : UnexpectedMageResultException
    {
        public readonly ItemHistoryAnalysis ItemHistory;
        public readonly  ItemHistoryAnalysis PreviousItemHistory;

        public HistoryHasntChangedException(ItemHistoryAnalysis itemHistory, ItemHistoryAnalysis previousItemHistory) :
            base("Expected mage history to change, but didn't!") {
            ItemHistory = itemHistory;
            PreviousItemHistory = previousItemHistory;
        }
    }
}
