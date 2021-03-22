using System;
using System.Collections.Generic;
using System.Linq;
using Inkybot.Dofus;
using Inkybot.Dofus.Domain;

namespace Inkybot.Services
{
    internal class OverMageToReachTargetMinimumItemMageResolve : StandardStatsPrioritizedItemMageResolve
    {
        public OverMageToReachTargetMinimumItemMageResolve(MageConfig config, Item item) : base(config, item) {
        }

        protected override bool MatchesCriteria(ItemMage itemMage) =>
            !itemMage.HasReachedTargetMinimum;
    }
}
