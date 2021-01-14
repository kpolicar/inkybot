using System;
using System.Collections.Generic;
using System.Linq;
using Inkybot.Dofus;

namespace Inkybot.Services
{
    internal class OverMageToReachTargetMinimumItemMageResolve : PrioritizedItemMageResolve
    {
        public OverMageToReachTargetMinimumItemMageResolve(MageConfig config, Item item) : base(config, item) {
        }

        protected override IEnumerable<ItemMage> PotentialMages() {
            return item.Stats
                .StandardStats
                .Select(itemStat => {
                    var runeType = ResolveRuneType(itemStat);
                
                    var rune = new Rune(itemStat.Stat, runeType);
                
                    return new ItemMage(
                        itemStat.Stat,
                        rune,
                        config[itemStat],
                        itemStat.Value
                    );
                }).Where(itemMage => !itemMage.HasReachedTargetMinimum);
        }

        protected override int Priority(ItemMage itemMage) {
            return itemMage.NumberOfRunesNeededForFullMage;
        }


        protected override ItemMage ChooseFromPrioritized(IOrderedEnumerable<ItemMage> prioritized) {
            return prioritized.FirstOrDefault(itemMage => itemMage.CanHit);
        }
    }
}
