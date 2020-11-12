using System.Collections.Generic;
using Inkybot.Domain;

namespace Inkybot.Contracts
{
    public interface IItemHistoryAnalyzer
    {
        ItemHistoryAnalysis Analyse(IEnumerable<MageHistoryRecord> history);
        float ResolveSinkChange(MageHistoryRecord record);
    }
}
