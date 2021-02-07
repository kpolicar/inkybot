using System.Linq;
using Inkybot.Dofus.Repositories;
using Inkybot.Helpers;

namespace Inkybot.Dofus
{
    public class Item
    {
        public readonly ItemStatRepository Stats;
        public bool IsValid => Stats.Length > 0;
        public bool IsInvalid => !IsValid;
        public bool IsOvermaged => Stats.StandardStats.Any(itemStat => itemStat.Value > itemStat.Max);
        public bool HasExo => Stats.ExoStats.Length > 0;
        public float Oversink => Stats.Sum(itemStat => itemStat.Oversink);

        public Item(ItemStatRepository stats) {
            Stats = stats;
        }

        public Stat? this[Stat index]
            => Stats[index]?.Stat ?? null;

        public bool HasStat(Stat stat)
            => this[stat] != null;

        public bool MatchesStandardStatsStructure(Item op1) {
            return StandardStatsStructureMatch(this, op1);
        }

        public bool HasDifferentStatValues(Item op1) {
            return Stats.Length != op1.Stats.Length ||
                Stats.ZipWithDefault(op1.Stats, (stats1, stats2) =>
                (stats1.Stat, stats1.Max, stats1.Min, stats1.Value) !=
                (stats2.Stat, stats2.Max, stats2.Min, stats2.Value))
                .Any(match => match);
        }

        private static bool StandardStatsStructureMatch(Item item1, Item item2) {
            var stats1 = item1.Stats.StandardStats;
            var stats2 = item2.Stats.StandardStats;
            if (stats1.Length != stats2.Length) return false;
            
            return stats1.Zip(stats2,
                    (s1, s2) => s1.Stat == s2.Stat)
                .All(equal => equal);
        }

        public override bool Equals(object obj) {
            if (obj is Item other)
                return Equals(other);
            return false;
        }

        public bool Equals(Item other) =>
            Stats.Equals(other.Stats);

        public static bool operator ==(Item? op1, Item? op2) {
            if (ReferenceEquals(null, op1) && ReferenceEquals(null, op2))
                return true;
            if (!ReferenceEquals(null, op1) && ReferenceEquals(null, op2))
                return false;
            if (ReferenceEquals(null, op1) && !ReferenceEquals(null, op2))
                return false;
            return op1!.Equals(op2!);
        }

        public static bool operator !=(Item? op1, Item? op2) {
            return !(op1 == op2);
        }
        
        public override int GetHashCode() {
            return Stats.GetHashCode();
        }
    }
}
