using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Inkybot.Contracts;
using Inkybot.Design;
using Inkybot.Domain;
using Inkybot.Events;

namespace Inkybot.Services
{
    public class ConfigManager : InjectableService
    {
        public event EventHandler<ConfigModifiedEventArgs> ConfigModified;

        public Config Config {
            get;
            private set;
        }
        
        public void BindDependencies() {
            var dataProvider = Program.Services.GetService<DofusDataProvider>();
            dataProvider.FetchedItem += StatsUpdated;
        }

        private void StatsUpdated(object sender, ItemEventArgs e) {
            EnforceConfigSetForItem(e.Item);
            RemoveFallenUnconfiguredStats(e.Item);
        }

        private void RemoveFallenUnconfiguredStats(Item item) {
            var fallenUnconfiguredStats =
                Config.StatsConfig.Where(statConfig => statConfig.Value.Target == 0 && !item.HasStat(statConfig.Key))
                    .Select(statConfig => statConfig.Key)
                    .ToArray();

            foreach (var stat in fallenUnconfiguredStats) {
                Config.StatsConfig.Remove(stat);
            }
            if (fallenUnconfiguredStats.Length > 0)
                ConfigModified?.Invoke(this, new ConfigModifiedEventArgs(Config, false, true));
        }

        public void RemoveExos() {
            var configuredExoStats = Config
                .Exos
                .Select(config => config.Key)
                .ToArray();
            
            foreach (var stat in configuredExoStats) {
                Config.StatsConfig.Remove(stat);
            }
            if (configuredExoStats.Length > 0)
                ConfigModified?.Invoke(this, new ConfigModifiedEventArgs(Config, true, true));
        }

        public void ResetConfig(Item item) {
            Config = new Config(item);
            ConfigModified?.Invoke(this, new ConfigModifiedEventArgs(Config, true, true));
        }

        public void EnforceConfigSetForItem(Item item) {
            if (!ConfigIsSetForItem(item)) {
                ResetConfig(item);
            }
        }

        private bool ConfigIsSetForItem(Item item) {
            return Config != null && Config.IsConfiguredForItem(item);
        }

        public void ChangeStatConfigTarget(Stat stat, int target) {
            var statConfig = Config.StatsConfig[stat];
            var newStatConfig = new StatConfig(stat, target, statConfig.Target);
            ChangeStatConfig(stat, newStatConfig);
        }

        public void ChangeStatConfig(Stat stat, StatConfig statConfig) {
            var isNewStatConfiguration = !Config.StatsConfig.ContainsKey(stat);
            if (!isNewStatConfiguration && statConfig == Config.StatsConfig[stat])
                return;
            
            Config.StatsConfig[stat] = statConfig;
            ConfigModified?.Invoke(this, new ConfigModifiedEventArgs(Config, true, isNewStatConfiguration));
            
            foreach (var keyValuePair in Config.StatsConfig) {
                Debug.WriteLine(keyValuePair.Key.DisplayName);
            }
        }
    }
}
