using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Inkybot.Adapters;
using Inkybot.Contracts;
using Inkybot.Design;
using Inkybot.Dofus;
using Inkybot.Events;
using Inkybot.Helpers;
using MageConfig = Inkybot.Dofus.MageConfig;
using StatConfig = Inkybot.Dofus.Stat.StatConfig;

namespace Inkybot.Services
{
    public class ConfigManager : InjectableService
    {
        public event EventHandler<ConfigModifiedEventArgs>? ConfigModified;

        public Dictionary<Stat, StatConfig> DefaultStatConfig = null!;
        public MageConfig? Config {
            get;
            private set;
        }
        
        public void BindDependencies(ServiceContainer serviceContainer) {
            var defaultConfig = serviceContainer.GetService<DefaultConfigProvider>();
            DefaultStatConfig = defaultConfig.DefaultStatConfig();
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
            var configuredExoStats = Config
                .Exos
                .Select(config => config.Key)
                .ToArray();
            
            foreach (var stat in configuredExoStats) {
                Config.StatsConfig.Remove(stat);
            }
            if (configuredExoStats.Length > 0)
                ConfigModified?.Invoke(this,
                    new ConfigModifiedEventArgs(Config, true, true));
        }

        public void ResetConfig(Item item) {
            Config = new MageConfig(item, DefaultStatConfig);
            ConfigModified?.Invoke(this, 
                new ConfigModifiedEventArgs(Config, true, true));
        }

        public void EnforceConfigSetForItem(Item item) {
            if (!ConfigIsSetForItem(item))
                ResetConfig(item);
        }

        private bool ConfigIsSetForItem(Item item)
            => Config != null && Config.IsConfiguredFor(item);

        public void ChangeStatConfigTarget(Stat stat, int target) {
            var statConfig = Config!.StatsConfig[stat];
            var newStatConfig = statConfig.Clone(target: target);
            ChangeStatConfig(stat, newStatConfig);
        }

        public void ChangeStatConfig(Stat stat, MageConfig.ItemStatMageConfig statConfig) {
            var isNewStatConfiguration = !Config!.StatsConfig.ContainsKey(stat);
            if (!isNewStatConfiguration && statConfig == Config.StatsConfig[stat])
                return;
            
            Config.StatsConfig[stat] = statConfig;
            ConfigModified?.Invoke(this, 
                new ConfigModifiedEventArgs(Config, true, isNewStatConfiguration));
        }
    }
}
