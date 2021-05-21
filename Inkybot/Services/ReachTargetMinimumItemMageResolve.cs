using System;
using System.Collections.Generic;
using System.Linq;
using Inkybot.Dofus;
using Inkybot.Dofus.Domain;

namespace Inkybot.Services
{
    internal class ReachTargetMinimumItemMageResolve : TargetItemMageResolve
    {
        public ReachTargetMinimumItemMageResolve(MageConfig config, Item item, int runeTypeOffset=0) : base(config, item, runeTypeOffset) {
        }

        protected override bool MatchesCriteria(ItemMage itemMage) =>
            !itemMage.HasReachedTargetMinimum;
    }
}
