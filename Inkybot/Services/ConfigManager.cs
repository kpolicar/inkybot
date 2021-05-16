using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Inkybot.Adapters;
using Inkybot.Contracts;
using Inkybot.Design;
using Inkybot.Dofus;
using Inkybot.Dofus.Contracts;
using Inkybot.Events;
using Inkybot.Helpers;
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
        public StatConfigProviderContract StatConfig = null!;
        public MageConfigProviderContract MageConfig = null!;
        public UserSettingsConfigManager UserSettings = null!;

        public MageConfig? Config {
            get;
            private set;
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

        public void ResetConfig(Item item) {
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
                    ResetConfig(item);
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

        private bool ConfigIsSetForItem(Item item)
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
            var statConfig = Config!.StatsConfig[stat];
            priority = Math.Min(Config!.StatsConfig.Count-1, priority);
            priority = Math.Max(0, priority);
            var newStatConfig = statConfig.Clone(statConfig.Target, statConfig.TargetMinimum, priority);
            ChangeStatConfig(stat, newStatConfig);

            if (priority == 0)
                return;
            
            var samePriority =
                Config!.StatsConfig.FirstOrDefault(statConfig => statConfig.Value.Priority == priority);
            if (!samePriority.Equals(default(KeyValuePair<Stat,MageConfig.ItemStatMageConfig>))) {
                ChangeStatConfigPriority(samePriority.Key, priority-1);
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
    }
}
