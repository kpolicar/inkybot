using System.Collections.Generic;
using Inkybot.Contracts;
using Inkybot.Dofus;

namespace Inkybot.Services
{
    public class QueuedConfigProvider : Dofus.Contracts.StatConfigProvider
    {
        private readonly Dictionary<Stat, StatConfig> StatsConfig;

        public QueuedConfigProvider(Dictionary<Stat, StatConfig> statsConfig) {
            StatsConfig = statsConfig;
        }
        
        public StatConfig Config(Stat stat) {
            return StatsConfig[stat];
        }

        public Dictionary<Stat, StatConfig> Config() {
            return StatsConfig;
        }

        public void ApplyToConfigManager(ConfigManager configManager) {
            foreach (var statConfig in configManager.UserSettings.Config()) {
                configManager.UserSettings.SetConfig(statConfig.Key, statConfig.Value);
            }
        }
    }
}
