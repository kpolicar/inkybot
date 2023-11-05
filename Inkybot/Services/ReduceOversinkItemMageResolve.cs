using System;
using System.Collections.Generic;
using System.Linq;
using Inkybot.Dofus;
using Inkybot.Dofus.Domain;

namespace Inkybot.Services
{
    internal class ReduceOversinkItemMageResolve : PrioritizedItemMageResolve
    {
        private readonly List<Stat> excludedStats = new List<Stat>();
        
        public ReduceOversinkItemMageResolve(MageConfig config, Item item) : base(config, item) {
        }
        
        public ReduceOversinkItemMageResolve ExcludeStats(Stat[]? stats) {
            if (stats != null && stats.Length > 0)
                excludedStats.AddRange(stats);

            return this;
        } 

        protected override bool MatchesCriteria(ItemMage itemMage) =>
            !itemMage.WillOvermage;
        
        protected override int Priority(ItemMage itemMage) =>
            (int) itemMage.Rune.Sink;

        protected override IEnumerable<ItemMage> PotentialMages() {
            return item.Stats
                .StandardStats
                .Where(itemStat => !itemStat.Overmaged && !excludedStats.Contains(itemStat.Stat) && itemStat.Stat.Mageable)
                .Select(itemStat => {
                    var rune = new Rune(itemStat.Stat, Rune.RuneType.Sm);
                
                    return new ItemMage(
                        itemStat.Stat,
                        rune,
                        config[itemStat]!.Value,
                        itemStat.Value
                    );
                });
        }
        
        protected override ItemMage ChooseFromPrioritized(IOrderedEnumerable<ItemMage> prioritized) =>
            prioritized.FirstOrDefault(MatchesCriteria);

        protected override IOrderedEnumerable<ItemMage> Prioritize() {
            var potentialMages = PotentialMages();
            return potentialMages.OrderBy(Priority);
        }
    }
}
