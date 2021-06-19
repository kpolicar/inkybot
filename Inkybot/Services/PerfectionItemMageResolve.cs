using System;
using System.Collections.Generic;
using System.Linq;
using Inkybot.Dofus;
using Inkybot.Dofus.Domain;

namespace Inkybot.Services
{
    internal class PerfectionItemMageResolve : TargetItemMageResolve
    {
        private decimal Sink;
        
        public PerfectionItemMageResolve(MageConfig config, Item item, decimal sink, int runeTypeOffset = 0)
            : base(config, item, runeTypeOffset) =>
            Sink = sink;
        
        protected override bool MatchesCriteria(ItemMage itemMage) =>
            !itemMage.WillOvermage && ShouldEvenConsider(itemMage);

        private bool ShouldEvenConsider(ItemMage itemMage) =>
            Sink - itemMage.Rune.Sink >= SinkNeededForAllOvermagesToReachTargetMinimum() * 2m;

        protected decimal SinkNeededForAllOvermagesToReachTargetMinimum() =>
            config.StatsConfig
                .Where(stat => !stat.Value.HighSinkStat)
                .Sum(statConfig => SinkNeededToReachTargetMinimum(statConfig.Key, statConfig.Value));
        
        protected decimal SinkNeededToReachTargetMinimum(Stat stat, MageConfig.ItemStatMageConfig mageConfig) =>
            (int) Math.Floor(stat.SinkValue * Math.Max(0, mageConfig.TargetMinimum - item.Stats[stat]?.Value ?? 0));

        protected override IEnumerable<ItemMage> PotentialMages() {
            return base.PotentialMages().Where(itemMage => itemMage.Rune.Sink <= Sink);
        }

        protected override int Priority(ItemMage itemMage) {
            var basePriority = base.Priority(itemMage);
            return basePriority + itemMage.MageConfig.Priority*item.Stats.Length
                                + (itemMage.HasReachedTargetMinimum ? 0 : 1000*item.Stats.Length);
        }
    }
}
