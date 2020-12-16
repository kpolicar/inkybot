using System.Collections.Generic;
using Inkybot.Domain;

namespace Inkybot.Contracts
{
    public interface IItemHistoryAnalyzer
    {
        ItemHistoryAnalysis Analyse(IEnumerable<MageHistoryRecord> history, bool fullHistory = true);
        float ResolveSinkChange(MageHistoryRecord record);
    }
}
