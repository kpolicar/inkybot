using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WindowsFormsApp.Contracts
{
    public interface DofusDataProvider
    {
        Item.ItemStat[] Stats();
        void FetchData();
        IEnumerable<MageHistoryRecord> History();
    }
}