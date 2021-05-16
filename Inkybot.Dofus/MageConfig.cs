using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Inkybot.Dofus.Contracts;

namespace Inkybot.Dofus
{
    /**
     * <summary>
     * The MageConfig class represents an item's mage configuration.
     * Every stat on the item is mapped to a ItemStatMageConfig, which
     * is taken into account by the AI in determining what stat to mage next.
     * </summary>
     */
    public class MageConfig
    {
        /**
         * <summary>The active mage config manager for the maging session.</summary>
         */
        public static MageConfigProvider ConfigManager {
            get => configManager ??= DefaultMageConfigProvider.Instance;
            set => configManager = value;
        }
        private static MageConfigProvider? configManager;
        
        /**
         * <summary>The active stat configuration</summary>
         */
        public readonly ItemMageConfig StatsConfig;
        
        /**
         * <summary>
         * A configuration detail provided by the ConfigManager which determines whether the AI
         * should prioritize the restoration of high sink stats immediately.
         * </summary>
         */
        public bool RestoreHighSinkStatsImmediately =>
            ConfigManager.RestoreHighSinkStatsImmediately;
        
        /**
         * <returns>Stat configuration that is configured for the provided stat</returns>
         */
        public ItemStatMageConfig? this[Stat index] =>
            StatsConfig.ContainsKey(index)
                ? StatsConfig[index]
                : (ItemStatMageConfig?) null;

        /**
         * <returns>Stat configuration that is configured for the provided item stat</returns>
         */
        public ItemStatMageConfig? this[ItemStat index] =>
            StatsConfig.ContainsKey(index.Stat)
                ? StatsConfig[index.Stat]
                : (ItemStatMageConfig?) null;

        /**
         * <summary>A dictionary of configured exo stats on the item.</summary>
         */
        public Dictionary<Stat, ItemStatMageConfig> Exos
            => StatsConfig.ExoStatsConfigs;

        /**
         * <param name="item">The item that the MageConfig is configuring.</param>
         */
        public MageConfig(Item item) {
            StatsConfig = new ItemMageConfig();
            
            foreach (var itemStat in item.Stats) {
                var stat = itemStat.Stat;
                StatsConfig[stat] = new ItemStatMageConfig(
                    itemStat.Stat,
                    itemStat.Min,
                    itemStat.Max,
                    itemStat.Stat.Mageable ? itemStat.Max : (int?) null,
                    null,
                    0);
            }
        }

        /**
         * <returns>Determines whether or not the configuration is applicable to another item.</returns>
         */
        public bool IsConfiguredFor(Item item)
            => StatsConfig.IsApplicableTo(item);

        /**
         * <returns>Determines whether or not the configuration is applicable to another item.</returns>
         */
        public ItemStat[] UnconfiguredItemStats(Item item)
            => StatsConfig.UnconfiguredItemStats(item);

        public override string ToString() {
            return string.Join("\r\n", StatsConfig.Values);
        }
        
        /**
         * <summary>
         * The ItemStatMageConfig struct represents a single item's stat mage configuration.
         * It is a *readonly* struct. Whenever the user modifies his config, a new struct is instantiated.
         * </summary>
         */
        public readonly struct ItemStatMageConfig
        {
            /**
             * <summary>The minimum value that the item has on the stat.</summary>
             */
            public readonly int Minimum;
            
            /**
             * <summary>The maximum value that the item has on the stat.</summary>
             */
            public readonly int Maximum;
            
            /**
             * <summary>The target value that is configured for the item stat.</summary>
             */
            public readonly int? Target;
            
            /**
             * <summary>The target value minimum that is configured for the item stat.</summary>
             */
            public readonly int? TargetMinimum;
            
            public readonly int Priority;
            
            /**
             * <summary>Whether or not the instance represents an exotic stat mage.</summary>
             */
            public bool Exo => Maximum == 0;
            
            /**
             * <summary>Whether or not the instance represents a stat overmage.</summary>
             */
            public bool Overmage => Target > Maximum;
            
            /**
             * <summary>
             * The maximum value at which a rune of SM strength can still land on the stat.
             * If set to null, SM runes can always land.
             * This value is taken from the statConfig.
             * </summary>
             */
            public int? MaxValueAtWhichSmRuneCanHit => statConfig.MaxValueAtWhichSmRuneCanHit;
            
            /**
             * <summary>
             * The lowest value at which a rune of PA strength should begin to be used.
             * If set to null, PA runes should not be used.
             * This value is taken from the statConfig.
             * </summary>
             */
            public int? ChangeToPaRuneThreshold => statConfig.ChangeToPaRuneThreshold;
            
            /**
             * <summary>
             * The maximum value at which a rune of PA strength can still land on the stat.
             * If set to null, PA runes can always land.
             * This value is taken from the statConfig.
             * </summary>
             */
            public int? MaxValueAtWhichPaRuneCanHit=> statConfig.MaxValueAtWhichPaRuneCanHit;
            
            /**
             * <summary>
             * The lowest value at which a rune of RA strength should begin to be used.
             * If set to null, RA runes should not be used.
             * This value is taken from the statConfig.
             * </summary>
             */
            public int? ChangeToRaRuneThreshold => statConfig.ChangeToRaRuneThreshold;
            
            /**
             * <summary>
             * Whether or not runes of PA strength should be used.
             * This value is taken from the statConfig.
             * </summary>
             */
            public bool ShouldUsePaRunes => statConfig.ShouldUsePaRunes;
            
            /**
             * <summary>
             * Whether or not runes of RA strength should be used.
             * This value is taken from the statConfig.
             * </summary>
             */
            public bool ShouldUseRaRunes => statConfig.ShouldUseRaRunes;
            
            /**
             * <summary>
             * Determines whether or not the stat should be interpreted as a high-sink stat.
             * This value is taken from the statConfig.
             * </summary>
             */
            public bool HighSinkStat => statConfig.HighSinkStat;
            
            /**
             * <summary>The active stat configuration for this config</summary>
             */
            private StatConfig statConfig => Stat.Config;
            
            /**
             * <summary>The stat that is configured</summary>
             */
            private readonly Stat Stat;

            public static ItemStatMageConfig Default(Stat stat) =>
                new ItemStatMageConfig(stat, default, default, default, default, default);

            public ItemStatMageConfig(Stat stat, int minimum, int maximum, int? target, int? targetMinimum, int priority) =>
                (Stat, Minimum, Maximum, Target, TargetMinimum, Priority) =
                (stat, minimum, maximum, target, targetMinimum, priority);

            /**
             * <summary>Create a new configuration for an exotic stat</summary>
             */
            public static ItemStatMageConfig MakeExo(Stat stat, int? target, int? targetMinimum, int priority) =>
                new ItemStatMageConfig(stat, 0, 0, target, targetMinimum, priority);

            /**
             * <returns>Determines whether or not the configuration is applicable to another item stat.</returns>
             */
            public bool IsApplicableTo(ItemStat itemStat) {
                return (itemStat.Min, itemStat.Max, itemStat.Stat)
                       == (Minimum, Maximum, Stat);
            }

            public ItemStatMageConfig Clone
                (int? target, int? targetMinimum, int priority, Stat? stat = null, int? minimum=null, int? maximum=null)
                => new ItemStatMageConfig(
                    stat ?? Stat,
                    minimum ?? Minimum,
                    maximum ?? Maximum,
                    target,
                    targetMinimum,
                    priority);
            
            
            public static bool operator ==(ItemStatMageConfig? x, ItemStatMageConfig? y) => 
                ReferenceEquals(x, null) == ReferenceEquals(y, null) &&
                ReferenceEquals(x, null) || Equals(x, y);
            public static bool operator !=(ItemStatMageConfig? x, ItemStatMageConfig? y) =>
                !(x == y);

            public override bool Equals(object? obj) =>
                obj is ItemStatMageConfig other && Equals(other);

            public bool Equals(ItemStatMageConfig other) =>
                (Stat, Minimum, Maximum, Target, TargetMinimum, Priority).Equals(
                    (other.Stat, other.Minimum, other.Maximum, other.Target, other.TargetMinimum, other.Priority));

            public override int GetHashCode() {
                unchecked {
                    var hashCode = Minimum;
                    hashCode = (hashCode * 397) ^ Maximum;
                    hashCode = (hashCode * 397) ^ Target ?? 1;
                    hashCode = (hashCode * 397) ^ TargetMinimum ?? 1;
                    hashCode = (hashCode * 397) ^ Priority;
                    hashCode = (hashCode * 397) ^ Stat.GetHashCode();
                    return hashCode;
                }
            }

            public override string ToString() =>
                $"{Stat.Identifier}: Min {Minimum}, Max {Maximum}, Target {Target}, TargetMinimum {TargetMinimum?.ToString() ?? "-"}, Priority {Priority}";
        }
    }
}
