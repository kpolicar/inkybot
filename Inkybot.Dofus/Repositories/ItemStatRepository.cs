using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Inkybot.Dofus.Repositories
{
    /**
     * <summary>
     * The ItemStatRepository class represents a collection of an item's stats with helper methods
     * for filtering.
     * </summary>
     */
    public class ItemStatRepository : IEnumerable<ItemStat>
    {
        /**
         * <summary>An array of item stats that the repository represents.</summary>
         */
        public readonly ItemStat[] Stats;
        
        /**
         * <summary>The number of item stats the repository contains.</summary>
         */
        public int Length => Stats.Length;

        /**
         * <param name="stats">An array of item stats that the repository represents.</param>
         */
        public ItemStatRepository(ItemStat[] stats) =>
            Stats = stats;
        
        /**
         * <summary>Subset of item stats that are considered mageable.</summary>
         */
        public ItemStat[] MageableStats =>
            Stats.Where(itemStat => itemStat.Stat.Mageable).ToArray();
        
        /**
         * <summary>Subset of item stats that are considered unmageable.</summary>
         */
        public ItemStat[] UnmageableStats =>
            Stats.Where(itemStat => !itemStat.Stat.Mageable).ToArray();
        
        /**
         * <summary>Subset of item stats that are considered to be standard to the item.</summary>
         */
        public ItemStat[] StandardStats =>
            MageableStats.Where(itemStat => !itemStat.Exo).ToArray();
        
        /**
         * <summary>Subset of item stats that are considered to be exotic to the item.</summary>
         */
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
