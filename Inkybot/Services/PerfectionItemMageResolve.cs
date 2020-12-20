using System;
using System.Collections.Generic;
using System.Linq;
using Inkybot.Domain;

namespace Inkybot.Services
{
    internal class PerfectionItemMageResolve : StandardItemMageResolve
    {
        private float sink;
        
        public PerfectionItemMageResolve(Config config, Item item, float sink, int runeTypeOffset = 0) : base(config, item, runeTypeOffset) {
            this.sink = sink;
        }

        protected override IEnumerable<ItemMage> PotentialMages() {
            return base.PotentialMages().Where(itemMage => itemMage.Rune.Sink <= sink);
        }

        protected override ItemMage ChooseFromPrioritized(IOrderedEnumerable<ItemMage> prioritized) {
            return prioritized.FirstOrDefault(itemMage => !itemMage.WillOvermage);
        }
    }
}
