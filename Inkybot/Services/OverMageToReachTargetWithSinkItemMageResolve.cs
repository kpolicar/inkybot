using System;
using System.Collections.Generic;
using System.Linq;
using Inkybot.Dofus;

namespace Inkybot.Services
{
    internal class OverMageToReachTargetWithSinkItemMageResolve : StandardStatsPrioritizedItemMageResolve
    {
        public readonly float Sink;
        
        public OverMageToReachTargetWithSinkItemMageResolve(MageConfig config, Item item, float sink) : base(config, item) {
            Sink = sink;
        }

        protected override bool MatchesCriteria(ItemMage itemMage) =>
            itemMage.Rune.Sink <= Sink && !itemMage.HasReachedTarget;
    }
}
