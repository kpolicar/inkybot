using System.Linq;
using Inkybot.Domain;
using Inkybot.Domain.Repositories;

namespace Inkybot.Domain
{
    public class Item
    {
        public readonly ItemStatRepository Stats;
        public bool IsValid => Stats.Length > 0;
        public bool IsInvalid => !IsValid;
        public bool IsOvermaged => Stats.StandardStats.Any(itemStat => itemStat.value > itemStat.max);
        public bool HasExo => Stats.ExoStats.Length > 0;

        public Item(ItemStatRepository stats) {
            Stats = stats;
        }

        public bool HasStat(Stat stat) {
            return Stats.Any(itemStat => itemStat.stat == stat);
        }

        public bool MatchesStandardStatsStructure(Item op1) {
            return StandardStatsStructureMatch(this, op1);
        }
        
        private static bool StandardStatsStructureMatch(Item item1, Item item2) {
            var stats1 = item1.Stats.StandardStats;
            var stats2 = item2.Stats.StandardStats;
            if (stats1.Length != stats2.Length) return false;
            
            return stats1.Zip(stats2,
                    (s1, s2) => s1.stat == s2.stat)
                .All(equal => equal);
        }
    }
}
