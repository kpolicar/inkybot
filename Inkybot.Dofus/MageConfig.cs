using System.Collections.Generic;
using System.Linq;
using StatConfig = Inkybot.Dofus.Stat.StatConfig;

namespace Inkybot.Dofus
{
    public class MageConfig
    {
        public readonly struct ItemStatMageConfig
        {
            public readonly int Minimum;
            public readonly int Maximum;
            public readonly int Target;
            public bool Exo => Maximum == 0;
            public bool Overmage => Target > Maximum;
            public int? MaxValueAtWhichSmRuneCanHit => statConfig.MaxValueAtWhichSmRuneCanHit;
            public int? ChangeToPaRuneThreshold => statConfig.ChangeToPaRuneThreshold;
            public int? MaxValueAtWhichPaRuneCanHit=> statConfig.MaxValueAtWhichPaRuneCanHit;
            public int? ChangeToRaRuneThreshold => statConfig.ChangeToRaRuneThreshold;
            public bool ShouldUsePaRunes => ChangeToPaRuneThreshold != null;
            public bool ShouldUseRaRunes => ChangeToRaRuneThreshold != null;
            public bool HighSinkStat => statConfig.HighSinkStat;
            public readonly StatConfig statConfig;

            public ItemStatMageConfig(int minimum, int maximum, int target, StatConfig statConfig) =>
                (Minimum, Maximum, Target, this.statConfig) =
                (minimum, maximum, target, statConfig);

            public bool IsApplicableTo(ItemStat itemStat) {
                return (itemStat.min, itemStat.max)
                       == (Minimum, Maximum);
            }

            public ItemStatMageConfig Clone
                (int? minimum=null, int? maximum=null, int? target=null, StatConfig? statConfig=null)
                => new ItemStatMageConfig(
                    minimum ?? Minimum,
                    maximum ?? Maximum,
                    target ?? Target,
                    statConfig ?? this.statConfig);
            
            
            public static bool operator ==(ItemStatMageConfig x, ItemStatMageConfig y) => x.Equals(y);
            public static bool operator !=(ItemStatMageConfig x, ItemStatMageConfig y) => !x.Equals(y);
        }
        
        public ItemMageConfig StatsConfig;
        public bool RestoreHighSinkStatsImmediately;

        public ItemStatMageConfig this[Stat index]
            => StatsConfig[index];
        
        public ItemStatMageConfig this[ItemStat index]
            => StatsConfig[index.stat];

        public Dictionary<Stat, ItemStatMageConfig> Exos
            => StatsConfig.ExoStatsConfigs;

        public MageConfig(Item item, Dictionary<Stat, StatConfig>? config = null) {
            config ??= Stat.DefaultConfig;
            StatsConfig = new ItemMageConfig();
            RestoreHighSinkStatsImmediately = true;
            
            foreach (var itemStat in item.Stats) {
                var stat = itemStat.stat;
                StatsConfig[stat] = new ItemStatMageConfig(
                    itemStat.min,
                    itemStat.max,
                    itemStat.max,
                    config[stat]);
            }
        }
        
        public MageConfig(ItemMageConfig statConfig) {
            StatsConfig = statConfig;
            RestoreHighSinkStatsImmediately = true;
        }

        public bool IsConfiguredFor(Item item)
            => StatsConfig.IsApplicableTo(item);

        public override string ToString() {
            return string.Join("\r\n", StatsConfig.Values);
        }
    }
}
