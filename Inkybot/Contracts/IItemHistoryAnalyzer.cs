using System.Collections.Generic;

namespace Inkybot.Contracts
{
    public interface IItemHistoryAnalyzer
    {
        ItemHistoryAnalysis Analyse(IEnumerable<MageHistoryRecord> history);
        float ResolveSinkChange(MageHistoryRecord record);
    }
}
