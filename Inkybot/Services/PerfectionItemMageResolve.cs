using System.Collections.Generic;
using System.Linq;
using Inkybot.Dofus;
using Inkybot.Dofus.Domain;

namespace Inkybot.Services
{
    internal class PerfectionItemMageResolve : TargetItemMageResolve
    {
        private float Sink;
        
        public PerfectionItemMageResolve(MageConfig config, Item item, float sink, int runeTypeOffset = 0)
            : base(config, item, runeTypeOffset) =>
            Sink = sink;
        
        protected override bool MatchesCriteria(ItemMage itemMage) =>
            !itemMage.WillOvermage;

        protected override IEnumerable<ItemMage> PotentialMages() {
            return base.PotentialMages().Where(itemMage => itemMage.Rune.Sink <= Sink);
        }

        protected override int Priority(ItemMage itemMage) {
            var basePriority = base.Priority(itemMage);
            return basePriority + itemMage.MageConfig.Priority*item.Stats.Length;
        }

        protected override ItemMage ChooseFromPrioritized(IOrderedEnumerable<ItemMage> prioritized) {
            return prioritized
                .FirstOrDefault(itemMage => itemMage.CanHitAccordingToConfiguration && !itemMage.WillOvermage);
        }
    }
}
