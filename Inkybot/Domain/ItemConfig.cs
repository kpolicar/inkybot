using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Inkybot.Domain.Repositories;

namespace Inkybot
{
    public struct StatConfig
    {
        public readonly int ChangeToPaRuneValue;
        public readonly int ChangeToRaRuneValue;
        public readonly int maximum;

        public bool CanUsePaRunes => ChangeToPaRuneValue != int.MinValue;
        public bool CanUseRaRunes => ChangeToRaRuneValue != int.MinValue;

        public StatConfig(int ChangeToPaRuneValue, int ChangeToRaRuneValue, int maximum) {
            this.ChangeToPaRuneValue = ChangeToPaRuneValue;
            this.ChangeToRaRuneValue = ChangeToRaRuneValue;
            this.maximum = maximum;
        }
        
        public static bool operator == (StatConfig op1, StatConfig op2) {
            return op1.ChangeToPaRuneValue == op2.ChangeToPaRuneValue &&
                   op1.ChangeToRaRuneValue == op2.ChangeToRaRuneValue &&
                   op1.maximum == op2.maximum;
        }

        public static bool operator !=(StatConfig op1, StatConfig op2) {
            return !(op1 == op1);
        }
    }

    public class ItemConfig
    {
        public readonly Item Item;
        public readonly Dictionary<Stat, StatConfig> Config = new Dictionary<Stat, StatConfig>();
        
        public KeyValuePair<Stat, StatConfig>[] Exos {
            get {
                var configuredExoStats = from itemConfig in Config 
                    where !(from standardStat in Item.Stats.StandardStats.Select(itemStat => itemStat.stat) 
                        select standardStat).Contains(itemConfig.Key) 
                    select itemConfig;

                return configuredExoStats.ToArray();
            }
        }

        public ItemConfig(Item item) {
            Item = item;
            ResetDefaults(item);
        }

        public StatConfig For(ItemStat itemStat) {
            return Config[itemStat.stat];
        }

        public StatConfig For(Stat stat) {
            return Config[stat];
        }

        public void ResetDefaults(Item item) {
            foreach (var itemStat in item.Stats) {
                var stat = itemStat.stat;
                Config[stat] = new StatConfig(stat.changeToPaRuneThreshold, stat.changeToRaRuneThreshold, itemStat.max);
            }
        }

        public bool IsConfiguredForItem(Item item) {
            return Item.MatchesStandardStatsStructure(item)
                && HasConfiguredExoStatsForItem(item);
        }

        public bool HasConfiguredExoStatsForItem(Item item) {
            return item.Stats.ExoStats.All(itemStat =>
                Config.ContainsKey(itemStat.stat));
        }
    }
}
