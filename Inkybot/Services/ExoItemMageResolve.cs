using System;
using System.Collections.Generic;
using System.Linq;
using Inkybot.Dofus;
using Inkybot.Dofus.Domain;

namespace Inkybot.Services
{
    internal class ExoItemMageResolve : PrioritizedItemMageResolve
    {
        public readonly float Sink;
        
        public ExoItemMageResolve(MageConfig config, Item item, float sink) : base(config, item) =>
            Sink = sink;

        protected override IEnumerable<ItemMage> PotentialMages() {
            return config.Exos
                .Where(statConfig => statConfig.Key.Mageable)
                .Select(statConfig => {
                    var itemMage = new ItemMage(
                        statConfig.Key,
                        new Rune(statConfig.Key, statConfig.Key.StrongestRuneType),
                        statConfig.Value,
                        item.Stats[statConfig.Key]?.Value ?? 0
                    );

                    while (itemMage.WillOvertarget &&
                           itemMage.Rune.Weaker != null) {
                        itemMage = itemMage.Clone(rune: itemMage.Rune.Weaker);
                    }

                    return itemMage;
                });
        }

        protected override bool MatchesCriteria(ItemMage itemMage) =>
            !itemMage.WillOvertarget &&
            (itemMage.Rune.Sink <= Sink || !itemMage.HasReachedTargetMinimum);

        protected override IOrderedEnumerable<ItemMage> Prioritize() {
            var potentialMages = PotentialMages();
            return potentialMages.OrderBy(Priority);
        }
        
        protected override int Priority(ItemMage itemMage) =>
            (int) itemMage.Rune.Sink - (itemMage.Rune.Sink <= Sink ? 1 : 0) * 1000;
    }
}
