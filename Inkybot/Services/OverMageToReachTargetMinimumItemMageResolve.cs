using System;
using System.Collections.Generic;
using System.Linq;
using Inkybot.Dofus;

namespace Inkybot.Services
{
    internal class OverMageToReachTargetMinimumItemMageResolve : OverTargetItemMageResolve
    {
        public OverMageToReachTargetMinimumItemMageResolve(MageConfig config, Item item) : base(config, item) {
        }

        protected override IEnumerable<ItemMage> PotentialMages() {
            return base.PotentialMages()
                .Where(itemMage =>
                    itemMage.MageConfig.TargetMinimum != null &&
                    itemMage.Value < itemMage.MageConfig.TargetMinimum);
        }


        protected override ItemMage ChooseFromPrioritized(IOrderedEnumerable<ItemMage> prioritized) {
            return prioritized.FirstOrDefault(itemMage => itemMage.CanHit);
        }
    }
}
