using System.Collections.Generic;
using System.Linq;
using Inkybot.Dofus;
using Inkybot.Dofus.Domain;

namespace Inkybot.Services
{
    internal abstract class StandardStatsPrioritizedItemMageResolve : PrioritizedItemMageResolve
    {
        private readonly List<Stat> excludedStats = new List<Stat>();

        public StandardStatsPrioritizedItemMageResolve(MageConfig config, Item item) : base(config, item) {
        }

        public StandardStatsPrioritizedItemMageResolve ExcludeStats(Stat[]? stats) {
            if (stats != null && stats.Length > 0)
                excludedStats.AddRange(stats);

            return this;
        } 

        protected override IEnumerable<ItemMage> PotentialMages() {
            var stats = item.Stats.StandardStats.Where(itemStat => itemStat.Stat.Mageable);
            if (excludedStats.Count > 0) {
                stats = stats.Where(itemStat => !excludedStats.Contains(itemStat.Stat)).ToArray();
            }
            
            return stats
                .Where(itemStat => ResolveRuneType(itemStat) != null)
                .Select(itemStat => {
                    var runeType = ResolveRuneType(itemStat)!.Value;
                
                    var rune = new Rune(itemStat.Stat, runeType);
                
                    return new ItemMage(
                        itemStat.Stat,
                        rune,
                        config[itemStat]!.Value,
                        itemStat.Value
                    );
                });
        }
        
        protected override int Priority(ItemMage itemMage) =>
            itemMage.NumberOfRunesNeededForFullMage * item.Stats.Length + itemMage.MageConfig.Priority
            - (itemMage.MageConfig.Target == 0 && itemMage.Min > 0 ? 1000 : 0)
            - (itemMage.MageConfig.Target <= itemMage.Max && itemMage.Max < 0 ? 1000 : 0);
    }
}
