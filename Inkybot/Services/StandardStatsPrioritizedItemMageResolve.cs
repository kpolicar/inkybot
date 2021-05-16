using System.Collections.Generic;
using System.Linq;
using Inkybot.Dofus;
using Inkybot.Dofus.Domain;

namespace Inkybot.Services
{
    internal abstract class StandardStatsPrioritizedItemMageResolve : PrioritizedItemMageResolve
    {
        public StandardStatsPrioritizedItemMageResolve(MageConfig config, Item item) : base(config, item) {
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
                        config[itemStat]!.Value,
                        itemStat.Value
                    );
                });
        }
        
        protected override int Priority(ItemMage itemMage) =>
            itemMage.NumberOfRunesNeededForFullMage * item.Stats.Length + itemMage.MageConfig.Priority;
    }
}
