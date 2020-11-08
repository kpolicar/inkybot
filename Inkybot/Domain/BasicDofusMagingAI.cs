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
        private ItemConfig itemConfig;
        private float sink;

        public BasicDofusMagingAI() {
            actions = (ActionFactory) Program.Services.GetService(typeof(ActionFactory));
            
            var configManager = (ConfigManager) Program.Services.GetService(typeof(ConfigManager));
            configManager!.ConfigModified += (sender, args) => itemConfig = args.ItemConfig;
            
            var magingJob = (DofusMagingJob) Program.Services.GetService(typeof(DofusMagingJob));
            magingJob!.SinkChanged += (sender, args) => sink = args.sink;
        }

        private ItemMage? ResolveItemMageByPriority(
                IEnumerable<ItemStat> stats, Func<IEnumerable<ItemMage>,
                IOrderedEnumerable<ItemMage>> priorityFunction)
        {
            var potentialItemMages = stats
                .Select(itemStat =>
                    new ItemMage(
                        itemStat.stat,
                        new Rune(itemStat.stat, ResolveRuneType(itemStat)),
                        itemConfig.For(itemStat),
                        itemStat.value
                        ));
                
            var prioritized = priorityFunction(potentialItemMages);

            var proposed = prioritized.FirstOrDefault(itemMage => !itemMage.WillOvermage);
            if (proposed.Equals(default(ItemMage)))
                return null;
            return proposed;
        }
        
        private ItemMage? ResolveItemMageByPriority(
                Item item,
                KeyValuePair<Stat, StatConfig>[] statsToExo,
                Func<IEnumerable<ItemMage>, IOrderedEnumerable<ItemMage>> priorityFunction)
        {
            var potentialItemMages = statsToExo
                .Select(statConfig =>
                new ItemMage(
                    statConfig.Key,
                    new Rune(statConfig.Key, statConfig.Value.StrongestRuneType),
                    statConfig.Value,
                    item.Stats[statConfig.Key].value
                ));
                
            var prioritized = priorityFunction(potentialItemMages);

            var proposed = prioritized.FirstOrDefault(itemMage => !itemMage.WillOvermage);
            if (proposed.Equals(default(ItemMage)))
                return null;
            return proposed;
        }

        public IAction ResolveAction(Item item, IAction previousAction) {
            var stats = item.Stats;
            Debug.WriteLine("has this many exos: "+item.Stats.ExoStats.Length);
            if (item.Stats.ExoStats.Length > 0)
                Debug.WriteLine("Are you sure you want to continue maging??");

            var proposedItemMage = ResolveItemMageByPriority(
                stats.StandardStats, 
                mages => mages.OrderByDescending(StatPriority));

            if (proposedItemMage == null) {
                Debug.WriteLine("no more standard mages");
                var statsToExo = itemConfig.Exos;
                
                proposedItemMage = ResolveItemMageByPriority(
                    item,
                    statsToExo, 
                    mages => mages.OrderBy(ExoPriority));
                
                if (proposedItemMage == null) 
                    return actions.Finish();
                
                Debug.WriteLine("Ready for EXO!");
            }

            var itemMage = proposedItemMage!.Value;

            Debug.WriteLine(
                $"Max of {itemMage.Stat.DisplayName} is {itemMage.MageConfig.maximum}, stat will overmage: {itemMage.WillOvermage}"
                );

            if (previousAction == null ||
                previousAction is Combine &&
                ((previousAction as Combine).target.stat != itemMage.Stat ||
                (previousAction as Combine).target.type != itemMage.Rune.type)) {
                return actions.SelectRune(itemMage.Rune);
            }

            return actions.Combine(itemMage.Rune);
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

        private int ExoPriority(ItemMage itemMage) {
            return (int) itemMage.Rune.Sink;
        }
    }
}
