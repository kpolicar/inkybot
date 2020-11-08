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

        public ItemConfig ItemConfig {
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
                ItemConfig.Config.Where(statConfig => statConfig.Value.maximum == 0 && !item.HasStat(statConfig.Key))
                    .Select(statConfig => statConfig.Key)
                    .ToArray();

            foreach (var stat in fallenUnconfiguredStats) {
                ItemConfig.Config.Remove(stat);
            }
            if (fallenUnconfiguredStats.Length > 0)
                ConfigChanged?.Invoke(this, new ConfigChangedEventArgs(ItemConfig, true));
        }

        public void RemoveExos() {
            var configuredExoStats = (from itemConfig in ItemConfig.Config 
                where !(from standardStat in ItemConfig.Item.Stats.StandardStats.Select(itemStat => itemStat.stat) 
                    select standardStat).Contains(itemConfig.Key) 
                select itemConfig.Key).ToArray();
            
            foreach (var stat in configuredExoStats) {
                ItemConfig.Config.Remove(stat);
            }
            if (configuredExoStats.Length > 0)
                ConfigChanged?.Invoke(this, new ConfigChangedEventArgs(ItemConfig, true));
        }

        public void ResetConfig(Item item) {
            ItemConfig = new ItemConfig(item);
            ConfigChanged?.Invoke(this, new ConfigChangedEventArgs(ItemConfig, true));
        }

        public void EnforceConfigSetForItem(Item item) {
            if (!ConfigIsSetForItem(item)) {
                ResetConfig(item);
            }
        }

        private bool ConfigIsSetForItem(Item item) {
            return ItemConfig != null && ItemConfig.IsConfiguredForItem(item);
        }

        public void ChangeStatConfig(Stat stat, StatConfig statConfig) {
            var isNewStatConfiguration = !ItemConfig.Config.ContainsKey(stat);
            if (!isNewStatConfiguration && statConfig == ItemConfig.Config[stat])
                return;
            
            ItemConfig.Config[stat] = statConfig;
            ConfigChanged?.Invoke(this, new ConfigChangedEventArgs(ItemConfig, isNewStatConfiguration));
            
            foreach (var keyValuePair in ItemConfig.Config) {
                Debug.WriteLine(keyValuePair.Key.DisplayName);
            }
        }
    }
}
