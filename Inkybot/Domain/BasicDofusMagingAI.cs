using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Inkybot.Actions;
using Inkybot.Contracts;
using Inkybot.Domain.Repositories;
using Inkybot.Events;
using Inkybot.Exceptions;
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
                IOrderedEnumerable<ItemMage>> priorityFunction,
                bool exo = false,
                int runeTypeOffset = 0)
        {
            var potentialItemMages = stats
                .Select(itemStat => {
                    var runeType = ResolveRuneType(itemStat);
                    runeType = runeType != Rune.Type.Sm ? runeType - runeTypeOffset : runeType;
                    
                    var rune = new Rune(itemStat.stat, runeType);
                    
                    return new ItemMage(
                        itemStat.stat,
                        rune,
                        itemConfig.For(itemStat),
                        itemStat.value,
                        exo
                    );
                });
                
            var prioritized = priorityFunction(potentialItemMages);

            var proposed = prioritized.FirstOrDefault(itemMage => {
                if (itemMage.WillOvermage)
                    return false;
                if (itemMage.Rune.type == Rune.Type.Sm && itemMage.Value > itemMage.MageConfig.MaxValueAtWhichSmRuneCanHit)
                    return false;
                if (itemMage.Rune.type == Rune.Type.Pa && itemMage.Value > itemMage.MageConfig.MaxValueAtWhichPaRuneCanHit)
                    return false;

                return true;
            });

            // Allow fallback to one offset
            if (runeTypeOffset == 0 && proposed.Equals(default(ItemMage))) {
                return ResolveItemMageByPriority(
                    stats,
                    priorityFunction,
                    exo,
                    1
                    );
            }
            
            if (proposed.Equals(default(ItemMage)))
                return null;
            return proposed;
        }
        
        private ItemMage? ResolveItemMageByPriority(
                Item item,
                KeyValuePair<Stat, StatConfig>[] statsConfig,
                Func<IEnumerable<ItemMage>, IOrderedEnumerable<ItemMage>> priorityFunction,
                bool exo = false)
        {
            var potentialItemMages = statsConfig
                .Select(statConfig =>
                new ItemMage(
                    statConfig.Key,
                    new Rune(statConfig.Key, statConfig.Value.StrongestRuneType),
                    statConfig.Value,
                    item.Stats[statConfig.Key].value,
                    exo
                ));
                
            var prioritized = priorityFunction(potentialItemMages);

            var proposed = prioritized.FirstOrDefault(itemMage => !itemMage.WillOvermage);
            if (proposed.Equals(default(ItemMage)))
                return null;
            return proposed;
        }

        private void HandleItemWithExistingExos() {
            
        }

        private ItemMage? ResolveItemMage(Item item) {
            return ResolveItemMageByPriority(
                item.Stats.StandardStats, 
                mages => mages.OrderByDescending(StatPriority));
        }

        private ItemMage? ResolveItemMageForExo(Item item) {
            var statsToExo = itemConfig.Exos;
                
            return ResolveItemMageByPriority(
                item,
                statsToExo, 
                mages => mages.OrderBy(ExoPriority),
                true);
        }

        public IAction ResolveAction(Item item, IAction previousAction) {
            Debug.WriteLine("has this many exos: "+item.Stats.ExoStats.Length);

            var proposedItemMage = ResolveItemMage(item) ?? ResolveItemMageForExo(item);;
            
            if (proposedItemMage == null)
                return actions.Finish();
            
            var itemMage = proposedItemMage.Value;

            Debug.WriteLine(
                $"Max of {itemMage.Stat.DisplayName} is {itemMage.MageConfig.maximum}, stat will overmage: {itemMage.WillOvermage}"
                );
            
            var selectRune = new Func<IAction>(() => actions.SelectRune(itemMage.Rune));

            if (previousAction == null)
                return selectRune();
            
            if (previousAction is Combine previousCombine) {
                if (previousCombine.Exo)
                    return selectRune();
                    
                if (previousCombine.target.stat != itemMage.Stat || previousCombine.target.type != itemMage.Rune.type)
                    return selectRune();
            }

            return actions.Combine(itemMage.Rune, itemMage.Exo);
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
