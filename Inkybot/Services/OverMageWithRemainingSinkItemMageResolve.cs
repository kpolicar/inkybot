using System;
using System.Collections.Generic;
using System.Linq;
using Inkybot.Dofus;
using Inkybot.Dofus.Domain;

namespace Inkybot.Services
{
    internal class FinishOffRemainingSinkItemMageResolve : StandardStatsPrioritizedItemMageResolve
    {
        public readonly float Sink;
        
        
        public FinishOffRemainingSinkItemMageResolve(MageConfig config, Item item, float sink) : base(config, item) =>
            Sink = sink;

        protected override bool MatchesCriteria(ItemMage itemMage) =>
            ShouldEvenConsider() && itemMage.Rune.Sink <= Sink;

        private bool ShouldEvenConsider() {
            if (item.HasExo || IsConfiguredForOvermageWithLowSinkStat() || config.Exos.Count > 1)
                return false;

            if (config.Exos.Count == 1) {
                var potentialExoMage = new ExoItemMageResolve(config, item, Sink).Resolve();
                if (potentialExoMage == null)
                    return true;

                var theExoCanLandUsingSink =
                    potentialExoMage.Value.Rune.Sink <= Sink && potentialExoMage.Value.NumberOfRunesNeededForFullMage == 1;
                return !theExoCanLandUsingSink;
            }

            return true;
        }

        private bool IsConfiguredForOvermageWithLowSinkStat() => 
            config.StatsConfig.Any(statConfig => statConfig.Value.Overmage && !statConfig.Value.HighSinkStat);

        protected override int Priority(ItemMage itemMage) => itemMage.MageConfig.Priority;
    }
}
