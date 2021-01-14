using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Inkybot.Dofus.Repositories
{
    public class ItemStatRepository : IEnumerable<ItemStat>
    {
        public readonly ItemStat[] Stats;
        public int Length => Stats.Length;

        public ItemStatRepository(ItemStat[] stats) {
            Stats = stats;
        }
        
        public ItemStat[] MageableStats =>
            Stats.Where(itemStat => itemStat.Stat.Mageable).ToArray();
        
        public ItemStat[] UnmageableStats =>
            Stats.Where(itemStat => !itemStat.Stat.Mageable).ToArray();
        
        public ItemStat[] StandardStats =>
            MageableStats.Where(itemStat => !itemStat.Exo).ToArray();
        
        public ItemStat[] ExoStats =>
            MageableStats.Where(itemStat => itemStat.Exo).ToArray();

        public IEnumerator<ItemStat> GetEnumerator() {
            return Stats.Select(stat => stat).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public override bool Equals(object obj) =>
            obj is ItemStatRepository other && Equals(other);

        public bool Equals(ItemStatRepository other) =>
            Length == other.Length && Stats
                .All(itemStat => other.Stats.Contains(itemStat));
        
        public override int GetHashCode() {
            unchecked {
                return Stats
                    .Aggregate(1, (acc, stat) => (stat.GetHashCode() * 397) ^ acc);
            }
        }

        public ItemStat this[int i] => Stats.ElementAt(i);
        public ItemStat? this[Stat? stat] => Stats.FirstOrDefault(itemStat => itemStat.Stat == stat);
    }
}
