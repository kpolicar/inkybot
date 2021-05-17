using System.Collections.Generic;
using System.Linq;
using Inkybot.Dofus;
using Inkybot.Dofus.Domain;

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
                itemMage.CanHitAccordingToConfiguration && MatchesCriteria(itemMage));
        }

        protected abstract bool MatchesCriteria(ItemMage itemMage);
        
        protected abstract IEnumerable<ItemMage> PotentialMages();

        protected abstract int Priority(ItemMage itemMage);
        
        protected virtual IOrderedEnumerable<ItemMage> Prioritize() {
            var potentialMages = PotentialMages();
            return potentialMages.OrderByDescending(Priority);
        }
        
        protected virtual Rune.RuneType? ResolveRuneType(ItemStat itemStat) {
            var itemConfig = config[itemStat];

            if (itemConfig!.Value.ShouldUseRaRunes && itemStat.Value >= itemConfig!.Value.ChangeToRaRuneThreshold) return Rune.RuneType.Ra;
            if (itemConfig!.Value.ShouldUsePaRunes && itemStat.Value >= itemConfig!.Value.ChangeToPaRuneThreshold) return Rune.RuneType.Pa;

            return itemConfig!.Value.ShouldUseSmRunes
                ? Rune.RuneType.Sm
                : null;
        }
    }
}
