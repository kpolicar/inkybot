using System.Collections.Generic;
using System.Linq;
using Inkybot.Dofus;

namespace Inkybot.Services
{
    internal class OverTargetItemMageResolve : PrioritizedItemMageResolve
    {
        public OverTargetItemMageResolve(MageConfig config, Item item) : base(config, item) {
        }

        protected override IEnumerable<ItemMage> PotentialMages() {
            return item.Stats
                .StandardStats
                .Select(itemStat => {
                    var runeType = ResolveRuneType(itemStat);
                
                    var rune = new Rune(itemStat.stat, runeType);
                
                    return new ItemMage(
                        itemStat.stat,
                        rune,
                        config[itemStat],
                        itemStat.value
                    );
                }).Where(itemMage =>
                    itemMage.WillOvertarget &&
                    itemMage.WithLowerRuneStrength != null &&
                    itemMage.WithLowerRuneStrength.Value.NumberOfRunesNeededToReachTarget >= 2);
        }

        protected override ItemMage ChooseFromPrioritized(IOrderedEnumerable<ItemMage> prioritized) {
            return prioritized.FirstOrDefault(itemMage =>
                itemMage.CanHit && !itemMage.WillOvermage);
        }

        protected override int Priority(ItemMage itemMage) {
            return itemMage.NumberOfRunesNeededForFullMage;
        }
    }
}
