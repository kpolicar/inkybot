using System.Linq;
using Inkybot.Domain;
using Inkybot.Domain.Repositories;
using Inkybot.Helpers;

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

        public bool MatchesStandardStats(Item op1) {
            return StandardStatsMatch(this, op1);
        }
        
        private static bool StandardStatsMatch(Item item1, Item item2) {
            var stats1 = item1.Stats;
            var stats2 = item2.Stats;
            
            return stats1.ZipWithDefault(stats2,
                    (s1, s2) =>
                        s1 != default && s2 != default
                            ? (s1.stat, s1.min, s1.max) == (s2.stat, s2.min, s2.max)
                            : s1 == default
                                ? s2.Exo == true
                                : s1.Exo == true
                            
                            )
                .All(equal => equal);
        }
        
        private static bool StandardStatsStructureMatch(Item item1, Item item2) {
            var stats1 = item1.Stats.StandardStats;
            var stats2 = item2.Stats.StandardStats;
            if (stats1.Length != stats2.Length) return false;
            
            return stats1.Zip(stats2,
                    (s1, s2) => s1.stat == s2.stat)
                .All(equal => equal);
        }

        public static bool operator ==(Item op1, Item op2) {
            if (ReferenceEquals(null, op1) && ReferenceEquals(null, op2))
                return true;
            if (!ReferenceEquals(null, op1) && ReferenceEquals(null, op2))
                return false;
            if (ReferenceEquals(null, op1) && !ReferenceEquals(null, op2))
                return false;
            var comparison = op1.Stats.Stats.ZipWithDefault(op2.Stats.Stats, (stats1, stats2) => new {
                Stats1 = stats1, Stats2 = stats2
            });

            return comparison.All(comparison =>
                comparison.Stats1 == comparison.Stats2);
        }

        public static bool operator !=(Item op1, Item op2) {
            return !(op1 == op2);
        }
    }
}
