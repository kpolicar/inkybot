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
        
        public FinishOffRemainingSinkItemMageResolve(MageConfig config, Item item, float sink)
            : base(config, item) =>
            Sink = sink;

        protected override bool MatchesCriteria(ItemMage itemMage) =>
            ShouldEvenConsider(itemMage) && itemMage.Rune.Sink <= Sink;

        private bool ShouldEvenConsider(ItemMage itemMage) {
            if (item.HasExo
                || IsConfiguredForOvermageWithLowSinkStat()
                || config.Exos.Count > 1
                || item.Stats.Any(itemStat => itemStat.Overmaged && itemStat.Stat != itemMage.Stat))
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

        protected override Rune.RuneType? ResolveRuneType(ItemStat itemStat) {
            var itemConfig = config[itemStat]!.Value;

            if (itemConfig.ShouldUseRaRunes
                && new Rune(itemStat.Stat, Rune.RuneType.Ra).Sink <= Sink
                && (itemStat.Value >= itemConfig.ChangeToRaRuneThreshold || (!itemConfig.ShouldUsePaRunes && !itemConfig.ShouldUseSmRunes)))
                return Rune.RuneType.Ra;
            
            if (itemConfig.ShouldUsePaRunes
                && new Rune(itemStat.Stat, Rune.RuneType.Pa).Sink <= Sink
                && (itemStat.Value >= itemConfig.ChangeToPaRuneThreshold || !itemConfig.ShouldUseSmRunes))
                return Rune.RuneType.Pa;

            return itemConfig.ShouldUseSmRunes
                ? Rune.RuneType.Sm
                : null;
        }
    }
}
