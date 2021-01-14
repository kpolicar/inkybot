using System;
using System.Collections.Generic;
using System.Linq;
using Inkybot.Dofus;

namespace Inkybot.Services
{
    internal class TargetItemMageResolve : StandardStatsPrioritizedItemMageResolve
    {
        private int runeTypeOffset;

        public TargetItemMageResolve(MageConfig config, Item item, int runeTypeOffset = 0) : base(config, item) {
            this.runeTypeOffset = runeTypeOffset;
        }

        protected override bool MatchesCriteria(ItemMage itemMage) =>
            !itemMage.WillOvermage && !itemMage.WillOvertarget;

        protected override int Priority(ItemMage itemMage) {
            if (IsHighSinkItemMage(itemMage) && config.RestoreHighSinkStatsImmediately) {
                return (int) itemMage.Rune.Sink * 1000;
            }
            return itemMage.NumberOfRunesNeededForFullMage;
        }

        private bool IsHighSinkItemMage(ItemMage itemMage) =>
            itemMage.Rune.Sink >= 30;

        protected override Rune.RuneType ResolveRuneType(ItemStat itemStat) {
            var runeType = base.ResolveRuneType(itemStat);
            return (Rune.RuneType) Math.Max(0, (int) runeType - runeTypeOffset);
        }
    }
}
