using System;
using System.Collections.Generic;
using System.Linq;
using Inkybot.Design;
using Inkybot.Dofus;
using Inkybot.Dofus.Contracts;
using Inkybot.Dofus.Domain;
using Inkybot.Events;
using DofusMagingJob = Inkybot.Contracts.DofusMagingJob;
using DofusMagingAIContract = Inkybot.Dofus.Contracts.DofusMagingAI;
using MageConfig = Inkybot.Dofus.MageConfig;

namespace Inkybot.Services
{
    public class DofusMagingAI : DofusMagingAIContract, HasDependencies
    {
        internal delegate ItemMage? OverrideResolve(MageResolveEventArgs eventArgs);
        
        internal OverrideResolve? OverrideTargetResolve;
        internal OverrideResolve? OverridePerfectionResolve;
        internal OverrideResolve? OverrideReachMinimumResolve;
        internal OverrideResolve? OverrideFinishSinkOverride;
        internal OverrideResolve? OverrideExoResolve;
        private MageConfig config = null!;
        private int? _simulatedSink;
        protected override int Sink => _simulatedSink ?? base.Sink;

        public override void BindDependencies(ServiceContainer serviceContainer) {
            var configManager = (ConfigManager) serviceContainer.GetService<MageConfigManager>();
            configManager.ConfigModified += (sender, args) => config = args.Config;;
            if (configManager.Config != null)
                config = configManager.Config;
            base.BindDependencies(serviceContainer);
        }

        private ItemMage? ResolveItemMageAndOverrideIfSuccessfullyResolved(
            Func<ItemMage?> resolveFunction,
            OverrideResolve? eventHandler) {
            var proposed = resolveFunction();
            if (proposed != null && eventHandler != null)
                proposed = eventHandler.Invoke(new MageResolveEventArgs(proposed.Value));
            return proposed;
        }

        private ItemMage? ResolveItemMage(Item item, Stat[] excludedStats) {
            if (!config.RestoreHighSinkStatsImmediately) {
                excludedStats = excludedStats.Concat(config.StatsConfig.HighSinkStats)
                    .ToArray();
                
            }

            var proposedMage = ResolveItemMageAndOverrideIfSuccessfullyResolved(() =>
                new TargetItemMageResolve(config, item).ExcludeStats(excludedStats).Resolve() ??
                new TargetItemMageResolve(config, item, 1).ExcludeStats(excludedStats).Resolve() ??
                new OverTargetItemMageResolve(config, item).ExcludeStats(excludedStats).Resolve(),
                OverrideTargetResolve);


            if (!item.IsOvermaged && !item.HasExo)
                proposedMage ??= ResolveItemMageAndOverrideIfSuccessfullyResolved(() =>
                    new PerfectionItemMageResolve(config, item, Sink).ExcludeStats(excludedStats).Resolve() ??
                    new PerfectionItemMageResolve(config, item, Sink, 1).ExcludeStats(excludedStats).Resolve(),
                    OverridePerfectionResolve);

            proposedMage ??= ResolveItemMageAndOverrideIfSuccessfullyResolved(() => {
                var proposed = new ReachTargetMinimumItemMageResolve(config, item).ExcludeStats(excludedStats).Resolve();
                
                if ((proposed?.WillOvermage ?? false) && !proposed!.Value.MageConfig.Exo) {
                    var proposedWithoutOvermage = new ReachTargetMinimumItemMageResolve(config, item, 1).ExcludeStats(excludedStats).Resolve();
                    proposed = proposedWithoutOvermage ?? proposed;
                }

                proposed ??= new OverMageToReachTargetWithSinkItemMageResolve(config, item, Sink).ExcludeStats(excludedStats).Resolve();

                // If a different stat other than the proposed is already overmaged, reduce it first
                if (proposed != null &&
                    item.IsOvermaged &&
                    proposed.Value.Rune.Sink >= 3 && // Whether or not this mage is likely to really ruin the current overmage
                    !item.Stats.MageableStats.Where(stat => stat.Overmaged).Any(
                        itemStat => config[itemStat]?.Target > itemStat.Max || config[itemStat]?.TargetMinimum > itemStat.Max) && // none of the overmaged stats (that need to be overmaged) are already overmaged
                    (!item.Stats[proposed.Value.Stat]?.Overmaged ?? false))
                {
                    proposed = new ReduceOversinkItemMageResolve(config, item).ExcludeStats(excludedStats).Resolve();
                }

                return proposed;
            }, OverrideReachMinimumResolve);

            proposedMage ??= ResolveItemMageAndOverrideIfSuccessfullyResolved(() =>
                new FinishOffRemainingSinkItemMageResolve(config, item, Sink).ExcludeStats(excludedStats).Resolve(),
                OverrideFinishSinkOverride);

            return proposedMage;
        }
        
        private ItemMage? ResolveItemMageForExo(Item item, Stat[] excludedStats) {
            return ResolveItemMageAndOverrideIfSuccessfullyResolved(() =>
                new ExoItemMageResolve(config, item, Sink).ExcludeStats(excludedStats).Resolve(),
                OverrideExoResolve);
        }

        protected override IAction Resolve() =>
            ResolveExcludingStats(new Stat[] {});

        protected override IAction ResolveExcludingStats(Stat[] excludedStats) {
            var proposedItemMage = ResolveItemMage(Item, excludedStats) ?? ResolveItemMageForExo(Item, excludedStats);
            
            if (!config.RestoreHighSinkStatsImmediately) {
                if (Sink < proposedItemMage?.Rune.Sink || proposedItemMage == null) {
                    var targetMageResolve = new TargetItemMageResolve(config, Item).ExcludeStats(excludedStats).Resolve();
                    if (targetMageResolve != null)
                        proposedItemMage = targetMageResolve;
                }
            }
            
            if (proposedItemMage != null && !SatisfiesOversinkConstraint(Item, proposedItemMage.Value))
                proposedItemMage = new ReduceOversinkItemMageResolve(config, Item).ExcludeStats(excludedStats).Resolve();
            
            if (proposedItemMage == null)
                return Finish();
            
            var itemMage = proposedItemMage.Value;
            
            return Combine(itemMage.Rune);
        }

        public List<ItemMage> GeneratePipeline(Item item, decimal currentSink) {
            var pipeline = new List<ItemMage>();
            var simulatedItem = item;
            var simulatedSink = currentSink;

            while (true) {
                _simulatedSink = (int) simulatedSink;
                Item = simulatedItem;

                var proposed = ResolveItemMage(simulatedItem, new Stat[]{})
                            ?? ResolveItemMageForExo(simulatedItem, new Stat[]{});

                if (!config.RestoreHighSinkStatsImmediately) {
                    if (simulatedSink < proposed?.Rune.Sink || proposed == null) {
                        var targetResolve = new TargetItemMageResolve(config, simulatedItem)
                            .ExcludeStats(new Stat[]{}).Resolve();
                        if (targetResolve != null)
                            proposed = targetResolve;
                    }
                }
                if (proposed != null && !SatisfiesOversinkConstraint(simulatedItem, proposed.Value))
                    proposed = new ReduceOversinkItemMageResolve(config, simulatedItem)
                        .ExcludeStats(new Stat[]{}).Resolve();

                Item = null!;
                _simulatedSink = null;

                if (proposed == null) break;
                if (proposed.Value.WillOvermage) break;
                if (proposed.Value.WillOvertarget) break;

                pipeline.Add(proposed.Value);

                simulatedItem = simulatedItem.WithStatValueIncreased(
                    proposed.Value.Stat,
                    proposed.Value.Rune.IncreaseInValue);
                simulatedSink -= proposed.Value.Rune.Sink;
                if (simulatedSink < 0) simulatedSink = 0;
            }

            return pipeline;
        }

        private bool SatisfiesOversinkConstraint(Item item, ItemMage proposedItemMage) {
            var currentItemStat = item.Stats[proposedItemMage.Stat];
            var newItemStat = new ItemStat(
                proposedItemMage.Stat,
                proposedItemMage.Rune.IncreaseInValue + (currentItemStat?.Value ?? 0),
                proposedItemMage.Min,
                proposedItemMage.Max);
            var proposedItemOversink =
                newItemStat.Oversink - (item.Stats[proposedItemMage.Stat]?.Oversink ?? 0) + item.Oversink;

            return proposedItemOversink <= 101 || !proposedItemMage.WillOvermage;
        }
    }
}
