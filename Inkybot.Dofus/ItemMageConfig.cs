using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Inkybot.Dofus
{
    /**
     * <summary>An item mage config dictionary, mapping stats to item stat mage configurations</summary>
     */
    public class ItemMageConfig : Dictionary<Stat, MageConfig.ItemStatMageConfig>
    {
        public Dictionary<Stat, MageConfig.ItemStatMageConfig> ExoStatsConfigs =>
            this.Where(pair => pair.Value.Exo)
                .ToDictionary(pair => pair.Key, pair => pair.Value);
        public Dictionary<Stat, MageConfig.ItemStatMageConfig> StandardStatsConfigs =>
            this.Where(pair => !pair.Value.Exo)
                .ToDictionary(pair => pair.Key, pair => pair.Value);
        
        /**
         * <returns>Determine whether or not the configuration is applicable to an item</returns>
         */
        public bool IsApplicableTo(Item item) {
            var applicable = true;
            var (standardStats, exoStats) =
                (item.Stats.StandardStats, item.Stats.ExoStats);

            applicable &= 
                standardStats.All(itemStat =>
                    StandardStatsConfigs.ContainsKey(itemStat.Stat) &&
                    StandardStatsConfigs[itemStat.Stat].IsApplicableTo(itemStat));
            applicable &= 
                exoStats.All(itemStat =>
                    ExoStatsConfigs.ContainsKey(itemStat.Stat) &&
                    ExoStatsConfigs[itemStat.Stat].IsApplicableTo(itemStat));

            return applicable;
        }
        
        public ItemStat[] UnconfiguredItemStats(Item item) {
            return item.Stats.Where(itemStat => !ContainsKey(itemStat.Stat)).ToArray();
        }
    }
}
