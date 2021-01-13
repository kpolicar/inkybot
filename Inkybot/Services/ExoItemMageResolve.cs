using System.Collections.Generic;
using System.Linq;
using Inkybot.Dofus;

namespace Inkybot.Services
{
    internal class ExoItemMageResolve : PrioritizedItemMageResolve
    {
        public ExoItemMageResolve(MageConfig config, Item item) : base(config, item) {
        }

        protected override IEnumerable<ItemMage> PotentialMages() {
            return config.Exos
                .Where(statConfig => statConfig.Key.Mageable)
                .Select(statConfig =>
                    new ItemMage(
                        statConfig.Key,
                        new Rune(statConfig.Key, statConfig.Key.StrongestRuneType),
                        statConfig.Value,
                        item.Stats[statConfig.Key]?.Value ?? 0,
                        true
                    ));
        }
        
        protected override IOrderedEnumerable<ItemMage> Prioritize() {
            var potentialMages = PotentialMages();
            return potentialMages.OrderBy(Priority);
        }
        
        protected override int Priority(ItemMage itemMage) {
            return (int) itemMage.Rune.Sink;
        }
    }
}
