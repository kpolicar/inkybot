using System.Collections.Generic;

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
    }

    public class Config
    {
        public Dictionary<Stat, StatConfig> stats = new Dictionary<Stat, StatConfig>();

        public Config(Item.ItemStat[] itemStats) {
            ResetDefaults(itemStats);
        }

        public StatConfig For(Item.ItemStat itemStat) {
            return stats[itemStat.stat];
        }

        public void ResetDefaults(Item.ItemStat[] itemStats) {
            foreach (var itemStat in itemStats) {
                var stat = itemStat.stat;
                stats[stat] = new StatConfig(stat.changeToPaRuneThreshold, stat.changeToRaRuneThreshold, itemStat.max);
            }
        }
    }
}
