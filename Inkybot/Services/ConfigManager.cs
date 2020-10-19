using System;
using System.Diagnostics;
using System.Linq;
using Inkybot.Contracts;
using Inkybot.Events;

namespace Inkybot.Services
{
    public class ConfigManager
    {
        private Config config;
        private DofusDataProvider dataProvider;
        public event EventHandler<ConfigChangedEventArgs> ConfigChanged;
        
        public void ItemChanged() {
            dataProvider = (DofusDataProvider) Program.Services.GetService(typeof(DofusDataProvider));
            dataProvider.FetchedStats += StatsUpdated;
        }

        private void StatsUpdated(object sender, StatsEventArgs e) {
            //EnforceConfigSetForStats(e.stats);
        }

        public void ResetConfig(Item.ItemStat[] itemStats) {
            config = new Config(itemStats);
            ConfigChanged?.Invoke(this, new ConfigChangedEventArgs(config));
        }

        public void EnforceConfigSetForStats(Item.ItemStat[] stats) {
            if (!ConfigIsSetForStats(stats)) {
                ResetConfig(stats);
            }
        }

        private bool ConfigIsSetForStats(Item.ItemStat[] stats) {
            if (config == null || stats.Length != config.stats.Count) return false;
            
            var comparison = stats.Zip(config.stats.Keys,
                (record1, record2) => new {Record1 = record1, Record2 = record2});

            return comparison.All(comparison =>
                       comparison.Record1.stat.DisplayName == comparison.Record2.DisplayName);
        }

        public void ChangeStatConfig(Stat stat, StatConfig statConfig) {
            config.stats[stat] = statConfig;
            ConfigChanged?.Invoke(this, new ConfigChangedEventArgs(config));
            foreach (var keyValuePair in config.stats) {
                Debug.WriteLine(keyValuePair.Key.DisplayName);
            }
        }
    }
}
