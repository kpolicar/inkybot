using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Inkybot.Actions;
using Inkybot.Contracts;
using Inkybot.Domain;
using Inkybot.Domain.Repositories;
using Inkybot.Events;
using Inkybot.Exceptions;
using Inkybot.Services;
using DofusMagingJob = Inkybot.Contracts.DofusMagingJob;

namespace Inkybot.Services
{
    public class BasicDofusMagingAI : DofusMagingAI
    {
        private readonly ActionFactory actions;
        private Config config;
        private float sink;

        public BasicDofusMagingAI() {
            actions = (ActionFactory) Program.Services.GetService(typeof(ActionFactory));
            
            var configManager = (ConfigManager) Program.Services.GetService(typeof(ConfigManager));
            configManager!.ConfigModified += (sender, args) => config = args.Config;
            
            var magingJob = (DofusMagingJob) Program.Services.GetService(typeof(DofusMagingJob));
            magingJob!.SinkChanged += (sender, args) => sink = args.Sink;
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
                        config.For(itemStat),
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
            var statsToExo = config.Exos;
                
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
                $"Max of {itemMage.Stat.DisplayName} is {itemMage.MageConfig.Maximum}, stat will overmage: {itemMage.WillOvermage}"
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
            var itemConfig = this.config.For(itemStat);

            if (itemConfig.CanUseRaRunes && itemStat.value >= itemConfig.ChangeToRaRuneThreshold) return Rune.Type.Ra;
            if (itemConfig.CanUsePaRunes && itemStat.value >= itemConfig.ChangeToPaRuneThreshold) return Rune.Type.Pa;

            return Rune.Type.Sm;
        }

        private bool IsHighSinkItemMage(ItemMage itemMage) {
            // Summon or higher
            return itemMage.Rune.Sink >= 30;
        }

        private int StatPriority(ItemMage itemMage) {
            if (IsHighSinkItemMage(itemMage) && config.MageConfig.RestoreHighSinkStatsFirst) {
                // 1000 ought to be enough to prioritize it over others
                return (int) itemMage.Rune.Sink * 1000;
            }
            return itemMage.NumberOfRunesNeededForFullMage;
        }

        private int ExoPriority(ItemMage itemMage) {
            return (int) itemMage.Rune.Sink;
        }
    }
}
