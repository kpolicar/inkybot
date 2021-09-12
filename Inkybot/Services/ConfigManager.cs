using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Inkybot.Adapters;
using Inkybot.Api.Resources;
using Inkybot.Contracts;
using Inkybot.Design;
using Inkybot.Dofus;
using Inkybot.Dofus.Contracts;
using Inkybot.Dofus.Repositories;
using Inkybot.Events;
using Inkybot.Exceptions;
using Inkybot.Helpers;
using Inkybot.Resources;
using Tesseract;
using MageConfig = Inkybot.Dofus.MageConfig;
using StatConfigProviderContract = Inkybot.Dofus.Contracts.StatConfigProvider;
using MageConfigProviderContract = Inkybot.Dofus.Contracts.MageConfigProvider;

namespace Inkybot.Services
{
    public class ConfigManager : MageConfigManager, HasDependencies
    {
        public event EventHandler<ConfigModifiedEventArgs>? ConfigModified;
        public event EventHandler<ConfigResetEventArgs>? ConfigReset;
        public event EventHandler<ItemEventArgs>? ApplyingPreset;
        public event EventHandler<PresetEventArgs>? AppliedPreset;
        public StatConfigProviderContract StatConfig = null!;
        public MageConfigProviderContract MageConfig = null!;
        public UserSettingsConfigManager UserSettings = null!;

        public MageConfig? Config {
            get;
            private set;
        }

        public ConfigManager() {
            ConfigModified += EnforceStatConfigPrioritiesInCorrectRange;
        }

        public void BindDependencies(ServiceContainer serviceContainer) {
            StatConfig = serviceContainer.GetService<StatConfigProviderContract>();
            MageConfig = serviceContainer.GetService<MageConfigProviderContract>();
            UserSettings = serviceContainer.GetService<UserSettingsConfigManager>();
        }

        public void RemoveFallenUnconfiguredStats(Item item) {
            if (Config == null)
                return;
            var fallenUnconfiguredStats =
                Config.StatsConfig
                    .Where(statConfig => statConfig.Value.Target == 0 && !item.HasStat(statConfig.Key))
                    .Select(statConfig => statConfig.Key)
                    .ToArray();

            foreach (var stat in fallenUnconfiguredStats) {
                Config.StatsConfig.Remove(stat);
            }
            if (fallenUnconfiguredStats.Length > 0)
                ConfigModified?.Invoke(this, new ConfigModifiedEventArgs(Config, false, true));
        }

        public void RemoveExos() {
            if (Config == null)
                return;
            var configuredExoStats = Config.Exos.Keys;
            
            foreach (var stat in configuredExoStats) {
                Config.StatsConfig.Remove(stat);
            }
            if (configuredExoStats.Count > 0)
                ConfigModified?.Invoke(this,
                    new ConfigModifiedEventArgs(Config, true, true));
        }

        public void ResetUserSettings(Item item) {
            var previousConfig = Config;
            Config = new MageConfig(item);
            ConfigModified?.Invoke(this, 
                new ConfigModifiedEventArgs(Config, true, true));
            ConfigReset?.Invoke(this, 
                new ConfigResetEventArgs(item, Config, previousConfig));
        }

        public void EnforceConfigSetForItem(Item item) {
            if (!ConfigIsSetForItem(item)) {
                var success = TryToAddMissingItemStats(item);
                if (!success || !ConfigIsSetForItem(item))
                    ResetUserSettings(item);
            }
        }

        private bool TryToAddMissingItemStats(Item item) {
            var unconfigured = Config?.UnconfiguredItemStats(item);
            if (Config == null || unconfigured == null || unconfigured.Any(itemStat => !itemStat.Exo))
                return false;

            foreach (var itemStat in unconfigured) {
                ChangeStatConfig(itemStat.Stat, Dofus.MageConfig.ItemStatMageConfig.Default(itemStat.Stat));
            }
            return true;
        }

        public bool ConfigIsSetForItem(Item item)
            => Config?.IsConfiguredFor(item) ?? false;

        public void ChangeStatConfigTarget(Stat stat, int? target) {
            var statConfig = Config!.StatsConfig[stat];
            var newStatConfig = statConfig.Clone(target, statConfig.TargetMinimum, statConfig.Priority);
            ChangeStatConfig(stat, newStatConfig);
        }
        
        public void ChangeStatConfigTargetMinimum(Stat stat, int? targetMinimum) {
            var statConfig = Config!.StatsConfig[stat];
            var newStatConfig = statConfig.Clone(statConfig.Target, targetMinimum, statConfig.Priority);
            ChangeStatConfig(stat, newStatConfig);
        }
        
        public void ChangeStatConfigPriority(Stat stat, int priority) {
            var currentStatConfig = Config!.StatsConfig[stat];
            priority = Math.Min(Config!.StatsConfig.Count-1, priority);
            priority = Math.Max(0, priority);
            var newStatConfig = currentStatConfig.Clone(currentStatConfig.Target, currentStatConfig.TargetMinimum, priority);
            ChangeStatConfig(stat, newStatConfig);
        }
        
        private void EnforceStatConfigPrioritiesInCorrectRange(object sender, ConfigModifiedEventArgs e) {
            if (Config == null || !e.StructureChanged)
                return;
            foreach (var stat in Config!.StatsConfig
                .OrderBy(statConfig => statConfig.Value.Priority)
                .Select(statConfig => statConfig.Key).ToArray()) {
                
                var statConfig = Config.StatsConfig[stat];
                var priority = Math.Min(Config!.StatsConfig.Count-1, statConfig.Priority);
                priority = Math.Max(0, priority);
                
                if (priority != statConfig.Priority)
                    ChangeStatConfigPriority(stat, priority);
            }
        }

        public void ChangeStatConfig(Stat stat, MageConfig.ItemStatMageConfig statConfig) {
            var isNewStatConfiguration = !Config!.StatsConfig.ContainsKey(stat);
            if (!isNewStatConfiguration && statConfig.Equals(Config.StatsConfig[stat]))
                return;
            
            Config.StatsConfig[stat] = statConfig;
            ConfigModified?.Invoke(this, 
                new ConfigModifiedEventArgs(Config, true, isNewStatConfiguration));
        }

        public void ApplyConfigPreset(int index) =>
            UserSettings.ApplyConfigPreset(index);

        public void ResetUserSettings() {
            UserSettings.Reset();
        }

        public void ApplyPreset(int index) {
            var preset = UserSettings.Presets.Presets[index];
            
            var itemStats = preset.Stats.Select(statPreset => {
                var stat = Stat.FirstOrNew(statPreset.Stat);

                return new ItemStat(stat, 0, statPreset.Minimum, statPreset.Maximum);
            }).ToArray();
            
            var item = new Item(new ItemStatRepository(itemStats));
            
            ApplyingPreset?.Invoke(this, new ItemEventArgs(item));
            
            if (!ConfigIsSetForItem(item)) {
                ResetUserSettings(item);
            }
            foreach (var statPreset in preset.Stats) {
                var stat = Stat.FirstOrNew(statPreset.Stat);
                if (!stat.Mageable)
                    continue;
                ChangeStatConfigTarget(stat, statPreset.Target);
                ChangeStatConfigTargetMinimum(stat, statPreset.TargetMinimum);
                ChangeStatConfigPriority(stat, statPreset.Priority);
            }
            
            AppliedPreset?.Invoke(this, new PresetEventArgs(preset, index));
        }
    }
}
