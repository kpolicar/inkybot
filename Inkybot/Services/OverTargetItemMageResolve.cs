using System;
using System.Collections.Generic;
using System.Linq;
using Inkybot.Dofus;

namespace Inkybot.Services
{
    internal class OverTargetItemMageResolve : StandardStatsPrioritizedItemMageResolve
    {
        public OverTargetItemMageResolve(MageConfig config, Item item) : base(config, item) {
        }

        protected override bool MatchesCriteria(ItemMage itemMage) =>
            !itemMage.WillOvermage &&
            itemMage.WithLowerRuneStrength != null &&
            itemMage.WithLowerRuneStrength.Value.NumberOfRunesNeededToReachTarget >= 2;

        protected override int Priority(ItemMage itemMage) =>
            itemMage.NumberOfRunesNeededForFullMage;
    }
}
