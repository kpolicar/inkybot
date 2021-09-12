using System.Collections.Generic;
using Inkybot.Contracts;
using Inkybot.Design;
using Inkybot.Dofus;
using Inkybot.Resources;
using StatConfig = Inkybot.Dofus.StatConfig;

namespace Inkybot.Services
{
    public class QueuedConfigProvider : Dofus.Contracts.StatConfigProvider
    {
        private readonly Dictionary<Stat, StatConfig> StatsConfig;
        public int? PresetIndex { get; set; }
        public int? ConfigPresetIndex { get; set; }

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

            if (PresetIndex != null)
                configManager.ApplyPreset(PresetIndex.Value);

            if (ConfigPresetIndex != null)
                configManager.ApplyConfigPreset(ConfigPresetIndex.Value);
            else {
                configManager.ResetUserSettings();
            }
        }
    }
}
