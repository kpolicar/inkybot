using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Inkybot.Domain.Repositories;

namespace Inkybot
{
    public struct StatConfig
    {
        public readonly int maximum;
        public readonly int ChangeToPaRuneThreshold => stat.ChangeToPaRuneThreshold;
        public readonly int ChangeToRaRuneThreshold => stat.ChangeToRaRuneThreshold;
        public readonly int MaxValueAtWhichSmRuneCanHit => stat.MaxValueAtWhichSmRuneCanLand;
        public readonly int MaxValueAtWhichPaRuneCanHit => stat.MaxValueAtWhichPaRuneCanLand;
        private Stat stat;

        public bool CanUsePaRunes => ChangeToPaRuneThreshold != int.MaxValue;
        public bool CanUseRaRunes => ChangeToRaRuneThreshold != int.MaxValue;
        
        public Rune.Type StrongestRuneType {
            get {
                if (CanUsePaRunes) return Rune.Type.Ra;
                if (CanUsePaRunes) return Rune.Type.Pa;

                return Rune.Type.Sm;
            }
        }

        public StatConfig(Stat stat, int maximum) {
            this.stat = stat;
            this.maximum = maximum;
        }
        
        public static bool operator == (StatConfig op1, StatConfig op2) {
            return op1.ChangeToPaRuneThreshold == op2.ChangeToPaRuneThreshold &&
                   op1.ChangeToRaRuneThreshold == op2.ChangeToRaRuneThreshold &&
                   op1.MaxValueAtWhichSmRuneCanHit == op2.MaxValueAtWhichSmRuneCanHit &&
                   op1.MaxValueAtWhichPaRuneCanHit == op2.MaxValueAtWhichPaRuneCanHit &&
                   op1.maximum == op2.maximum;
        }

        public static bool operator !=(StatConfig op1, StatConfig op2) {
            return !(op1 == op1);
        }
    }

    public class Config
    {
        public readonly Item Item;
        public readonly Dictionary<Stat, StatConfig> StatsConfig = new Dictionary<Stat, StatConfig>();
        public readonly MageConfig MageConfig = new MageConfig();
        
        public KeyValuePair<Stat, StatConfig>[] Exos {
            get {
                var configuredExoStats = from itemConfig in StatsConfig 
                    where !(from standardStat in Item.Stats.StandardStats.Select(itemStat => itemStat.stat) 
                        select standardStat).Contains(itemConfig.Key) 
                    select itemConfig;

                return configuredExoStats.ToArray();
            }
        }

        public Config(Item item) {
            Item = item;
            ResetDefaults(item);
        }

        public StatConfig For(ItemStat itemStat) {
            return StatsConfig[itemStat.stat];
        }

        public StatConfig For(Stat stat) {
            return StatsConfig[stat];
        }

        public void ResetDefaults(Item item) {
            foreach (var itemStat in item.Stats) {
                var stat = itemStat.stat;
                StatsConfig[stat] = new StatConfig(stat,  itemStat.max);
            }
        }

        public bool IsConfiguredForItem(Item item) {
            return Item.MatchesStandardStatsStructure(item)
                && HasConfiguredExoStatsForItem(item);
        }

        public bool HasConfiguredExoStatsForItem(Item item) {
            return item.Stats.ExoStats.All(itemStat =>
                StatsConfig.ContainsKey(itemStat.stat));
        }
    }
}
