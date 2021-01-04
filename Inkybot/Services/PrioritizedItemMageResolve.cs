using System.Collections.Generic;
using System.Linq;
using Inkybot.Dofus;

namespace Inkybot.Services
{
    internal abstract class PrioritizedItemMageResolve
    {
        protected MageConfig config;
        protected Item item;

        public PrioritizedItemMageResolve(MageConfig config, Item item) {
            this.config = config;
            this.item = item;
        }

        public ItemMage? Resolve() {
            var prioritized = Prioritize();

            var proposed = ChooseFromPrioritized(prioritized);
            
            if (proposed.Equals(default(ItemMage)))
                return null;
            return proposed;
        }

        protected virtual ItemMage ChooseFromPrioritized(IOrderedEnumerable<ItemMage> prioritized) {
            return prioritized.FirstOrDefault(itemMage =>
                itemMage.CanHit && !itemMage.WillOvertarget);
        }

        protected abstract IEnumerable<ItemMage> PotentialMages();

        protected abstract int Priority(ItemMage itemMage);
        
        protected virtual IOrderedEnumerable<ItemMage> Prioritize() {
            var potentialMages = PotentialMages();
            return potentialMages.OrderByDescending(Priority);
        }
        
        protected virtual Rune.Type ResolveRuneType(ItemStat itemStat) {
            var itemConfig = config[itemStat];

            if (itemConfig.ShouldUseRaRunes && itemStat.value >= itemConfig.ChangeToRaRuneThreshold) return Rune.Type.Ra;
            if (itemConfig.ShouldUsePaRunes && itemStat.value >= itemConfig.ChangeToPaRuneThreshold) return Rune.Type.Pa;

            return Rune.Type.Sm;
        }
    }
}
