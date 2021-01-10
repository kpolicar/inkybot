using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Inkybot.Dofus
{
    public class ItemMageConfig : Dictionary<Stat, MageConfig.ItemStatMageConfig>
    {
        public Dictionary<Stat, MageConfig.ItemStatMageConfig> ExoStatsConfigs =>
            this.Where(pair => pair.Value.Exo)
                .ToDictionary(pair => pair.Key, pair => pair.Value);
        public Dictionary<Stat, MageConfig.ItemStatMageConfig> StandardStatsConfigs =>
            this.Where(pair => !pair.Value.Exo)
                .ToDictionary(pair => pair.Key, pair => pair.Value);
        
        public bool IsApplicableTo(Item item) {
            var applicable = true;
            var (standardStats, exoStats) =
                (item.Stats.StandardStats, item.Stats.ExoStats);

            applicable &= 
                standardStats.Length == StandardStatsConfigs.Count &&
                standardStats.All(itemStat =>
                    StandardStatsConfigs[itemStat.stat].IsApplicableTo(itemStat));
            applicable &= 
                exoStats.Length == ExoStatsConfigs.Count &&
                exoStats.All(itemStat =>
                    ExoStatsConfigs[itemStat.stat].IsApplicableTo(itemStat));

            return applicable;
        }
    }
}
