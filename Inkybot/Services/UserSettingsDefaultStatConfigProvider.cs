using System.Collections.Generic;
using System.Linq;
using Inkybot.Contracts;
using Inkybot.Dofus;
using Inkybot.Helpers;
using StatConfig = Inkybot.Dofus.Stat.StatConfig;

namespace Inkybot.Services
{
    public class UserSettingsDefaultConfigProvider : DefaultConfigProvider
    {
        public Dictionary<Stat, StatConfig> DefaultStatConfig() {
            return Stat.Stats.Values.Select(stat => {
                var savedConfig = (Inkybot.Resources.StatConfig) Properties.Settings.Default[stat.Identifier];
                var config = new StatConfig(
                    Numbers.Parse(savedConfig.MaxValueAtWhichSmRuneCanLand),
                    Numbers.Parse(savedConfig.ChangeToPaRuneThreshold),
                    Numbers.Parse(savedConfig.MaxValueAtWhichPaRuneCanLand),
                    Numbers.Parse(savedConfig.ChangeToRaRuneThreshold), 
                    Stat.DefaultConfig[stat].HighSinkStat
                );
                
                return (stat, config);
            }).ToDictionary(pair => pair.stat, pair => pair.config);
        }
    }
}
