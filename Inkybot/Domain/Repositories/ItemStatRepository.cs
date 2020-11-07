using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Inkybot.Domain.Repositories
{
    public class ItemStatRepository : IEnumerable<ItemStat>
    {
        public readonly ItemStat[] Stats;
        public int Length => Stats.Length;

        public ItemStatRepository(ItemStat[] stats) {
            Stats = stats;
        }
        
        public ItemStat[] Standard =>
            Stats.Where(stat => !stat.Exo).ToArray();
        
        public ItemStat[] Exos =>
            Stats.Where(stat => stat.Exo).ToArray();

        public IEnumerator<ItemStat> GetEnumerator() {
            return (IEnumerator<ItemStat>) Stats.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public ItemStat this[int i] => Stats.ElementAt(i);
    }
}
