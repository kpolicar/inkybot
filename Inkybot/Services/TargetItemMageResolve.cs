using System;
using System.Collections.Generic;
using System.Linq;
using Inkybot.Dofus;

namespace Inkybot.Services
{
    internal class TargetItemMageResolve : PrioritizedItemMageResolve
    {
        private int runeTypeOffset;

        public TargetItemMageResolve(MageConfig config, Item item, int runeTypeOffset = 0) : base(config, item) {
            this.runeTypeOffset = runeTypeOffset;
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
                        config[itemStat],
                        itemStat.Value
                    );
            }).Where(itemMage => itemMage.CanHit && !itemMage.WillOvermage);
        }

        protected override int Priority(ItemMage itemMage) {
            if (IsHighSinkItemMage(itemMage) && config.RestoreHighSinkStatsImmediately && !itemMage.WillOvermage) {
                // 1000 ought to be enough to prioritize it over others
                return (int) itemMage.Rune.Sink * 1000;
            }
            return itemMage.NumberOfRunesNeededForFullMage;
        }

        private bool IsHighSinkItemMage(ItemMage itemMage) {
            // Summon or higher
            return itemMage.Rune.Sink >= 30;
        }

        protected override Rune.RuneType ResolveRuneType(ItemStat itemStat) {
            var runeType = base.ResolveRuneType(itemStat);
            return (Rune.RuneType) Math.Max(0, (int) runeType - runeTypeOffset);
        }
    }
}
