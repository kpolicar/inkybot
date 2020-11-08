using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Inkybot.Actions;
using Inkybot.Contracts;
using Inkybot.Domain.Repositories;
using Inkybot.Events;
using Inkybot.Services;

namespace Inkybot
{
    public class BasicDofusMagingAI : DofusMagingAI
    {
        private readonly ActionFactory actions;
        private readonly ConfigManager configManager;
        private ItemConfig itemConfig;

        public BasicDofusMagingAI() {
            actions = (ActionFactory) Program.Services.GetService(typeof(ActionFactory));
            var configManager = (ConfigManager) Program.Services.GetService(typeof(ConfigManager));
            configManager.ConfigChanged += OnConfigChanged;
        }

        public void OnConfigChanged(object sender, ConfigChangedEventArgs args) {
            itemConfig = args.ItemConfig;
        }

        public void SetConfig(ItemConfig itemConfig) {
            this.itemConfig = itemConfig;
        }

        public IAction ResolveAction(Item item, IAction previousAction) {
            var stats = item.Stats;
            // Todo: fix
            var itemMage = stats
                .Select(itemStat => new ItemMage(itemStat, new Rune(itemStat.stat, ResolveRuneType(itemStat)), ref itemConfig))
                .OrderByDescending(StatPriority)
                .Cast<ItemMage?>()
                .FirstOrDefault(itemMage => !itemMage!.Value.WillOvermage)
                .GetValueOrDefault(new ItemMage(stats.Stats.First(), new Rune(stats.First().stat, ResolveRuneType(stats.First())), ref itemConfig));

            Debug.WriteLine(
                $"Max of {itemMage.stat.stat.DisplayName} is {itemConfig.For(itemMage.stat).maximum}, stat will overmage: {itemMage.WillOvermage}"
                );
            // Todo: add condition based on remaining sink
            if (itemMage.WillOvermage)
                return actions.Finish();

            if (previousAction == null ||
                previousAction is Combine &&
                ((previousAction as Combine).target.stat != itemMage.stat.stat ||
                (previousAction as Combine).target.type != itemMage.rune.type)) {
                return actions.SelectRune(itemMage.rune);
            }

            return actions.Combine(itemMage.rune);
        }

        private Rune.Type ResolveRuneType(ItemStat itemStat) {
            var itemConfig = this.itemConfig.For(itemStat);

            if (itemConfig.CanUseRaRunes && itemStat.value > itemConfig.ChangeToRaRuneValue) return Rune.Type.Ra;

            if (itemConfig.CanUsePaRunes && itemStat.value > itemConfig.ChangeToPaRuneValue) return Rune.Type.Pa;

            return Rune.Type.Sm;
        }

        private int StatPriority(ItemMage itemMage) {
            return itemMage.NumberOfRunesNeededForFullMage;
        }

        private struct ItemMage
        {
            public readonly ItemStat stat;
            public readonly Rune rune;
            private readonly ItemConfig mageItemConfig;

            public bool WillOvermage => stat.value + rune.IncreaseInValue > mageItemConfig.For(stat).maximum;

            public ItemMage(ItemStat stat, Rune rune, ref ItemConfig mageItemConfig) {
                this.stat = stat;
                this.rune = rune;
                this.mageItemConfig = mageItemConfig;
            }

            // Todo: should you change priority based on item stat max or config stat max?
            public int NumberOfRunesNeededForFullMage =>
                (int) Math.Ceiling((stat.max - stat.value) / (float) rune.IncreaseInValue);
        }
    }
}
