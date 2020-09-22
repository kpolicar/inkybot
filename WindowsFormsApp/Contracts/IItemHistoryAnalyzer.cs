using System.Collections;
using System.Collections.Generic;

namespace WindowsFormsApp.Contracts
{
    public interface IItemHistoryAnalyzer
    {
        ItemHistoryAnalysis Analyse(IEnumerable<MageHistoryRecord> history);
        float ResolveSinkChange(MageHistoryRecord record);
        void onReadHistory(MageHistoryRecord changeRecord);
    }
}