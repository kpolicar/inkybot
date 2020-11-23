using System.Collections.Generic;
using System.Linq;
using Inkybot.Domain;

namespace Inkybot.Services
{
    internal abstract class PrioritizedItemMageResolve
    {
        protected Config config;
        protected Item item;

        public PrioritizedItemMageResolve(Config config, Item item) {
            this.config = config;
            this.item = item;
        }

        public ItemMage? Resolve() {
            var prioritized = Prioritize();
            
            var proposed = prioritized.FirstOrDefault(itemMage => !itemMage.WillOvermage);
            
            if (proposed.Equals(default(ItemMage)))
                return null;
            return proposed;
        }

        protected abstract IEnumerable<ItemMage> PotentialMages();

        protected abstract int Priority(ItemMage itemMage);
        
        protected virtual IOrderedEnumerable<ItemMage> Prioritize() {
            var potentialMages = PotentialMages();
            return potentialMages.OrderBy(Priority);
        }
        
        protected virtual Rune.Type ResolveRuneType(ItemStat itemStat) {
            var itemConfig = config.For(itemStat);

            if (itemConfig.CanUseRaRunes && itemStat.value >= itemConfig.ChangeToRaRuneThreshold) return Rune.Type.Ra;
            if (itemConfig.CanUsePaRunes && itemStat.value >= itemConfig.ChangeToPaRuneThreshold) return Rune.Type.Pa;

            return Rune.Type.Sm;
        }
    }
}
