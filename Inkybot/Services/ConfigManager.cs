using System;
using System.Diagnostics;
using System.Linq;
using Inkybot.Contracts;
using Inkybot.Domain.Repositories;
using Inkybot.Events;

namespace Inkybot.Services
{
    public class ConfigManager
    {
        public event EventHandler<ConfigChangedEventArgs> ConfigChanged;

        public ItemConfig Config {
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
                Config.Config.Where(statConfig => statConfig.Value.maximum == 0 && !item.HasStat(statConfig.Key))
                    .Select(statConfig => statConfig.Key)
                    .ToArray();

            foreach (var stat in fallenUnconfiguredStats) {
                Config.Config.Remove(stat);
            }
            if (fallenUnconfiguredStats.Length > 0)
                ConfigChanged?.Invoke(this, new ConfigChangedEventArgs(Config, true));
        }

        public void ResetConfig(Item item) {
            Config = new ItemConfig(item);
            ConfigChanged?.Invoke(this, new ConfigChangedEventArgs(Config, true));
        }

        public void EnforceConfigSetForItem(Item item) {
            if (!ConfigIsSetForItem(item)) {
                Debug.WriteLine("resetting");
                ResetConfig(item);
            }
        }

        private bool ConfigIsSetForItem(Item item) {
            return Config != null && Config.IsConfiguredForItem(item);
        }

        public void ChangeStatConfig(Stat stat, StatConfig statConfig) {
            var structureChanged = !Config.Config.ContainsKey(stat);
            Config.Config[stat] = statConfig;
            ConfigChanged?.Invoke(this, new ConfigChangedEventArgs(Config, structureChanged));
            
            foreach (var keyValuePair in Config.Config) {
                Debug.WriteLine(keyValuePair.Key.DisplayName);
            }
        }
    }
}
