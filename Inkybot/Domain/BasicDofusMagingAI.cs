using System;
using System.Collections.Generic;
using System.Linq;
using Inkybot.Actions;
using Inkybot.Contracts;
using Inkybot.Events;
using static Inkybot.Item;

namespace Inkybot
{
    public class BasicDofusMagingAI : DofusMagingAI
    {
        private readonly List<IAction> actionHistory = new List<IAction>();
        private readonly ActionFactory actions;
        private Config config;

        public BasicDofusMagingAI() {
            actions = (ActionFactory) Program.Services.GetService(typeof(ActionFactory));
            var actionHandler = (ActionHandler) Program.Services.GetService(typeof(ActionHandler));
        }

        public void SetConfig(Config config) {
            this.config = config;
        }

        public IAction ResolveAction(Item.ItemStat[] itemStats, IAction previousAction) {
            var itemMage = itemStats
                .DefaultIfEmpty(itemStats.First())
                .Select(itemStat => new ItemMage(itemStat, new Rune(itemStat.stat, ResolveRuneType(itemStat))))
                .OrderByDescending(StatPriority)
                .FirstOrDefault(item => !item.WillOvermage);

            System.Diagnostics.Debug.WriteLine(
                $"Max of {itemMage.stat.stat.DisplayName} is {itemMage.stat.max}, stat will overmage: {itemMage.WillOvermage}"
                );
            // Todo: add condition based on remaining sink
            if (itemMage.stat.max <= itemMage.stat.value)
                return actions.Finish();

            if (previousAction == null ||
                previousAction is Combine &&
                ((previousAction as Combine).target.stat.DisplayName != itemMage.stat.stat.DisplayName ||
                (previousAction as Combine).target.type != itemMage.rune.type))
                return actions.SelectRune(itemMage.rune);
            
            return actions.Combine(itemMage.rune);
        }

        private Rune.Type ResolveRuneType(Item.ItemStat itemStat) {
            var itemConfig = config.For(itemStat);

            if (itemConfig.CanUseRaRunes && itemStat.value > itemConfig.ChangeToRaRuneValue) return Rune.Type.Ra;

            if (itemConfig.CanUsePaRunes && itemStat.value > itemConfig.ChangeToPaRuneValue) return Rune.Type.Pa;

            return Rune.Type.Sm;
        }

        private int StatPriority(ItemMage itemMage) {
            return itemMage.NumberOfRunesNeededForFullMage;
        }

        private struct ItemMage
        {
            public readonly Item.ItemStat stat;
            public readonly Rune rune;

            public bool WillOvermage => stat.value + rune.IncreaseInValue > stat.max;

            public ItemMage(Item.ItemStat stat, Rune rune) {
                this.stat = stat;
                this.rune = rune;
            }

            public int NumberOfRunesNeededForFullMage =>
                (int) Math.Ceiling((stat.max - stat.value) / (float) rune.IncreaseInValue);
        }
    }
}
