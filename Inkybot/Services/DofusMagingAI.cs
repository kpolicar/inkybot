using System;
using System.Linq;
using Inkybot.Design;
using Inkybot.Dofus;
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
        internal OverrideResolve? Exo;
        private MageConfig config = null!;
        private float sink;

        public override void BindDependencies(ServiceContainer serviceContainer) {
            var configManager = serviceContainer.GetService<ConfigManager>();
            configManager.ConfigModified += (sender, args) => config = args.Config;;
            if (configManager.Config != null)
                config = configManager.Config;
            
            var magingJob = serviceContainer.GetService<DofusMagingJob>();
            magingJob.SinkChanged += (sender, args) => sink = args.Sink;
            base.BindDependencies(serviceContainer);
        }

        private ItemMage? ResolveItemMageAndOverrideIfSuccessfullyResolved(
            Func<ItemMage?> resolveFunction,
            OverrideResolve? eventHandler) {
            var proposed = resolveFunction();
            if (proposed != null)
                proposed = eventHandler?.Invoke(new MageResolveEventArgs(proposed.Value)) ?? proposed;
            return proposed;
        }

        private ItemMage? ResolveItemMage(Item item) {
            var proposedMage = ResolveItemMageAndOverrideIfSuccessfullyResolved(() =>
                new TargetItemMageResolve(config, item).Resolve() ??
                new TargetItemMageResolve(config, item, 1).Resolve() ??
                new OverTargetItemMageResolve(config, item).Resolve(),
                OverrideTargetResolve);
            

            if (!item.IsOvermaged && !item.HasExo)
                proposedMage ??= ResolveItemMageAndOverrideIfSuccessfullyResolved(() => 
                    new PerfectionItemMageResolve(config, item, sink).Resolve() ??
                    new PerfectionItemMageResolve(config, item, sink, 1).Resolve(),
                    OverridePerfectionResolve);

            proposedMage ??= ResolveItemMageAndOverrideIfSuccessfullyResolved(() =>
                new OverMageToReachTargetMinimumItemMageResolve(config, item).Resolve() ??
                new OverMageToReachTargetWithSinkItemMageResolve(config, item, sink).Resolve(),
                OverrideReachMinimumResolve);

            proposedMage ??= ResolveItemMageAndOverrideIfSuccessfullyResolved(() =>
                new FinishOffRemainingSinkItemMageResolve(config, item, sink).Resolve(),
                OverrideFinishSinkOverride);

            return proposedMage;
        }
        
        private bool IsConfiguredForOvermage() => 
            config.StatsConfig.Any(statConfig => statConfig.Value.Overmage);
        
        private ItemMage? ResolveItemMageForExo(Item item) {
            var proposedMage = ResolveItemMageAndOverrideIfSuccessfullyResolved(() =>
                new ExoItemMageResolve(config, item, sink).Resolve(),
                Exo);
            return proposedMage;
        }

        protected override IAction Resolve(Item item) {
            var proposedItemMage = ResolveItemMage(item) ?? ResolveItemMageForExo(item);
            
            if (proposedItemMage != null && !SatisfiesOversinkConstraint(item, proposedItemMage.Value))
                proposedItemMage = new ReduceOversinkItemMageResolve(config, item).Resolve();
            
            if (proposedItemMage == null)
                return Finish();
            
            var itemMage = proposedItemMage.Value;
            
            return Combine(itemMage.Rune);
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
