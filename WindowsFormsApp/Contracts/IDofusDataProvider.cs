using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WindowsFormsApp
{
    public interface IDofusDataProvider
    {
        Task<Item.ItemStat[]> Stats();
    }
}