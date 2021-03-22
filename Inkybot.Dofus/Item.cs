using System.Linq;
using Inkybot.Dofus.Repositories;
using Inkybot.Helpers;

namespace Inkybot.Dofus
{
    /**
     * <summary>
     * The Item class represents a single item.
     * Instance equality is determined by comparing stats.
     * </summary>
     */
    public class Item
    {
        /**
         * <summary>The current stats on the item.</summary>
         */
        public readonly ItemStatRepository Stats;
        
        /**
         * <summary>Whether or not the item is considered to be a valid game item, passing the game rules.</summary>
         */
        public bool IsValid => Stats.Length > 0;
        
        /**
         * <summary>Whether or not the item is considered to be an invalid game item, not passing all the game rules.</summary>
         */
        public bool IsInvalid => !IsValid;
        
        /**
         * <summary>Whether or not the item has at least one overmaged stat.</summary>
         */
        public bool IsOvermaged => Stats.StandardStats.Any(itemStat => itemStat.Value > itemStat.Max);
        
        /**
         * <summary>Whether or not the item has at least one exotically maged item stat.</summary>
         */
        public bool HasExo => Stats.ExoStats.Length > 0;
        
        /**
         * <summary>The amount of oversink on the item.</summary>
         */
        public float Oversink => Stats.Sum(itemStat => itemStat.Oversink);

        public Item(ItemStatRepository stats) {
            Stats = stats;
        }

        public Stat? this[Stat index]
            => Stats[index]?.Stat ?? null;

        /**
         * <returns>Whether or not the item has a stat</returns>
         */
        public bool HasStat(Stat stat)
            => this[stat] != null;

        /**
         * <summary>
         * Compares with another item and determines whether or not the standard stats
         * have a matching structure. An item's stat structure is matching, if it has
         * the same stats and they are all in the same order.  
         * </summary>
         */
        public bool MatchesStandardStatsStructure(Item op1) {
            return StandardStatsStructureMatch(this, op1);
        }

        /**
         * <summary>
         * Compares with another item and determines whether or not the stats have the same value.
         * </summary>
         */
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

        public override string ToString() =>
            string.Join("\n", Stats.Stats.Select(stat => stat.ToString()));
    }
}
