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
        
        protected virtual Rune.RuneType ResolveRuneType(ItemStat itemStat) {
            var itemConfig = config[itemStat];

            if (itemConfig.ShouldUseRaRunes && itemStat.Value >= itemConfig.ChangeToRaRuneThreshold) return Rune.RuneType.Ra;
            if (itemConfig.ShouldUsePaRunes && itemStat.Value >= itemConfig.ChangeToPaRuneThreshold) return Rune.RuneType.Pa;

            return Rune.RuneType.Sm;
        }
    }
}
