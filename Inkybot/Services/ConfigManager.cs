using System;
using System.Diagnostics;
using System.Linq;
using Inkybot.Contracts;
using Inkybot.Domain;
using Inkybot.Domain.Repositories;
using Inkybot.Events;

namespace Inkybot.Services
{
    public class ConfigManager
    {
        public event EventHandler<ConfigModifiedEventArgs> ConfigModified;

        public Config Config {
            get;
            private set;
        }

        public ConfigManager() {
            var dataProvider = (DofusDataProvider) Program.Services.GetService(typeof(DofusDataProvider));
            dataProvider.FetchedItem += StatsUpdated;
        }

        private void StatsUpdated(object sender, ItemEventArgs e) {
            EnforceConfigSetForItem(e.Item);
            RemoveFallenUnconfiguredStats(e.Item);
        }

        private void RemoveFallenUnconfiguredStats(Item item) {
            var fallenUnconfiguredStats =
                Config.StatsConfig.Where(statConfig => statConfig.Value.maximum == 0 && !item.HasStat(statConfig.Key))
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
