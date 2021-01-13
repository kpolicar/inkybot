using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Inkybot.Dofus.Contracts;
using Inkybot.Events;

namespace Inkybot.Dofus
{
    public class MageConfig
    {
        private static MageConfigProvider? configManager;
        public static MageConfigProvider ConfigManager {
            get => configManager ??= DefaultMageConfigProvider.Instance;
            set => configManager = value;
        }
        public readonly ItemMageConfig StatsConfig;
        public bool RestoreHighSinkStatsImmediately =>
            ConfigManager.RestoreHighSinkStatsImmediately;
        
        public ItemStatMageConfig this[Stat index] =>
            StatsConfig[index];

        public ItemStatMageConfig this[ItemStat index] =>
            StatsConfig[index.Stat];

        public Dictionary<Stat, ItemStatMageConfig> Exos
            => StatsConfig.ExoStatsConfigs;

        public MageConfig(Item item) {
            StatsConfig = new ItemMageConfig();
            
            foreach (var itemStat in item.Stats) {
                var stat = itemStat.Stat;
                StatsConfig[stat] = new ItemStatMageConfig(
                    itemStat.Stat,
                    itemStat.Min,
                    itemStat.Max,
                    itemStat.Max,
                    null);
            }
        }

        public bool IsConfiguredFor(Item item)
            => StatsConfig.IsApplicableTo(item);

        public override string ToString() {
            return string.Join("\r\n", StatsConfig.Values);
        }
        
        public readonly struct ItemStatMageConfig
        {
            public readonly int Minimum;
            public readonly int Maximum;
            public readonly int? Target;
            public readonly int? TargetMinimum;
            public bool Exo => Maximum == 0;
            public bool Overmage => Target > Maximum;
            public int? MaxValueAtWhichSmRuneCanHit => statConfig.MaxValueAtWhichSmRuneCanHit;
            public int? ChangeToPaRuneThreshold => statConfig.ChangeToPaRuneThreshold;
            public int? MaxValueAtWhichPaRuneCanHit=> statConfig.MaxValueAtWhichPaRuneCanHit;
            public int? ChangeToRaRuneThreshold => statConfig.ChangeToRaRuneThreshold;
            public bool ShouldUsePaRunes => ChangeToPaRuneThreshold != null;
            public bool ShouldUseRaRunes => ChangeToRaRuneThreshold != null;
            public bool HighSinkStat => statConfig.HighSinkStat;
            private StatConfig statConfig => Stat.Config;
            private readonly Stat Stat;

            public ItemStatMageConfig(Stat stat, int minimum, int maximum, int? target, int? targetMinimum) =>
                (Stat, Minimum, Maximum, Target, TargetMinimum) =
                (stat, minimum, maximum, target, targetMinimum);

            public bool IsApplicableTo(ItemStat itemStat) {
                return (itemStat.Min, itemStat.Max)
                       == (Minimum, Maximum);
            }

            public ItemStatMageConfig Clone
                (int? target, int? targetMinimum, Stat? stat = null, int? minimum=null, int? maximum=null)
                => new ItemStatMageConfig(
                    stat ?? Stat,
                    minimum ?? Minimum,
                    maximum ?? Maximum,
                    target,
                    targetMinimum);
            
            
            public static bool operator ==(ItemStatMageConfig? x, ItemStatMageConfig? y) => 
                ReferenceEquals(x, null) == ReferenceEquals(y, null) &&
                Equals(x, y);
            public static bool operator !=(ItemStatMageConfig? x, ItemStatMageConfig? y) =>
                !(x == y);

            public override bool Equals(object? obj) =>
                obj is ItemStatMageConfig other && Equals(other);

            public bool Equals(ItemStatMageConfig other) =>
                (Stat, Minimum, Maximum, Target, TargetMinimum).Equals(
                    (other.Stat, other.Minimum, other.Maximum, other.Target, other.TargetMinimum));

            public override int GetHashCode() {
                unchecked {
                    var hashCode = Minimum;
                    hashCode = (hashCode * 397) ^ Maximum;
                    hashCode = (hashCode * 397) ^ Target ?? 1;
                    hashCode = (hashCode * 397) ^ TargetMinimum ?? 1;
                    hashCode = (hashCode * 397) ^ Stat.GetHashCode();
                    return hashCode;
                }
            }
        }
    }
}
