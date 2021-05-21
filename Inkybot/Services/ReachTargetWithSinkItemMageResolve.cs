using System;
using System.Collections.Generic;
using System.Linq;
using Inkybot.Dofus;
using Inkybot.Dofus.Domain;

namespace Inkybot.Services
{
    internal class OverMageToReachTargetWithSinkItemMageResolve : StandardStatsPrioritizedItemMageResolve
    {
        public readonly float Sink;
        public readonly bool ShouldSaveSink;
        
        public OverMageToReachTargetWithSinkItemMageResolve(MageConfig config, Item item, float sink) : base(config, item) {
            Sink = sink;
            ShouldSaveSink = config.StatsConfig.Any(config => config.Value.Exo || config.Value.Overmage);
        }

        protected override int Priority(ItemMage itemMage) {
            var basePriority = base.Priority(itemMage);
            return basePriority + itemMage.MageConfig.Priority*30;
        }
        
        protected override bool MatchesCriteria(ItemMage itemMage) =>
            itemMage.Rune.Sink <= Sink &&
            !itemMage.HasReachedTarget &&
            (!ShouldSaveSink || (itemMage.WithLowerRuneStrength == null || (
                itemMage.WithLowerRuneStrength.Value.NumberOfRunesNeededToReachTarget >= 2 && !itemMage.WillOvermage ||
                itemMage.WithLowerRuneStrength.Value.NumberOfRunesNeededToReachTarget >= 3 && itemMage.WillOvermage
            )));
    }
}
