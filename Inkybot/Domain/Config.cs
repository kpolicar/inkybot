using System.Collections.Generic;
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
    }

    public class Config
    {
        public Dictionary<Stat, StatConfig> stats = new Dictionary<Stat, StatConfig>();
        public Dictionary<Stat, StatConfig> exos = new Dictionary<Stat, StatConfig>();

        public Config(ItemStatRepository itemStats) {
            ResetDefaults(itemStats);
        }

        public StatConfig For(ItemStat itemStat) {
            if (exos.ContainsKey(itemStat.stat))
                return exos[itemStat.stat];
            
            return stats[itemStat.stat];
        }

        public void ResetDefaults(ItemStatRepository itemStats) {
            foreach (var itemStat in itemStats) {
                var stat = itemStat.stat;
                stats[stat] = new StatConfig(stat.changeToPaRuneThreshold, stat.changeToRaRuneThreshold, itemStat.max);
                exos = new Dictionary<Stat, StatConfig>();
            }
        }
    }
}
