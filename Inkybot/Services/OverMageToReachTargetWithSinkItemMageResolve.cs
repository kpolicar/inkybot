using System;
using System.Collections.Generic;
using System.Linq;
using Inkybot.Dofus;

namespace Inkybot.Services
{
    internal class OverMageToReachTargetWithSinkItemMageResolve : PrioritizedItemMageResolve
    {
        public readonly float Sink;
        
        public OverMageToReachTargetWithSinkItemMageResolve(MageConfig config, Item item, float sink) : base(config, item) {
            Sink = sink;
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
                }).Where(itemMage => itemMage.Rune.Sink <= Sink && !itemMage.HasReachedTarget);
        }
        
        protected override int Priority(ItemMage itemMage) {
            return itemMage.NumberOfRunesNeededForFullMage;
        }

        protected override ItemMage ChooseFromPrioritized(IOrderedEnumerable<ItemMage> prioritized) {
            return prioritized.FirstOrDefault(itemMage => itemMage.CanHit);
        }
    }
}
