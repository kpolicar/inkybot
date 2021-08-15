using System;
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

        private ItemMage? ResolveItemMage(Item item) {
            var proposedMage = ResolveItemMageAndOverrideIfSuccessfullyResolved(() =>
                new TargetItemMageResolve(config, item).Resolve() ??
                new TargetItemMageResolve(config, item, 1).Resolve() ??
                new OverTargetItemMageResolve(config, item).Resolve(),
                OverrideTargetResolve);


            if (!item.IsOvermaged && !item.HasExo)
                proposedMage ??= ResolveItemMageAndOverrideIfSuccessfullyResolved(() =>
                    new PerfectionItemMageResolve(config, item, Sink).Resolve() ??
                    new PerfectionItemMageResolve(config, item, Sink, 1).Resolve(),
                    OverridePerfectionResolve);

            proposedMage ??= ResolveItemMageAndOverrideIfSuccessfullyResolved(() => {
                var proposed = new ReachTargetMinimumItemMageResolve(config, item).Resolve();
                
                if ((proposed?.WillOvermage ?? false) && !proposed!.Value.MageConfig.Exo) {
                    var proposedWithoutOvermage = new ReachTargetMinimumItemMageResolve(config, item, 1).Resolve();
                    proposed = proposedWithoutOvermage ?? proposed;
                }

                proposed ??= new OverMageToReachTargetWithSinkItemMageResolve(config, item, Sink).Resolve();

                // If a different stat other than the proposed is already overmaged, reduce it first
                if (proposed != null &&
                    item.IsOvermaged &&
                    proposed.Value.Rune.Sink >= 3 && // Whether or not this mage is likely to really ruin the current overmage
                    (!item.Stats[proposed.Value.Stat]?.Overmaged ?? false))
                {
                    proposed = new ReduceOversinkItemMageResolve(config, item).Resolve();
                }
                
                return proposed;
            }, OverrideReachMinimumResolve);

            proposedMage ??= ResolveItemMageAndOverrideIfSuccessfullyResolved(() =>
                new FinishOffRemainingSinkItemMageResolve(config, item, Sink).Resolve(),
                OverrideFinishSinkOverride);

            return proposedMage;
        }
        
        private ItemMage? ResolveItemMageForExo(Item item) {
            var proposedMage = ResolveItemMageAndOverrideIfSuccessfullyResolved(() =>
                new ExoItemMageResolve(config, item, Sink).Resolve(),
                OverrideExoResolve);
            return proposedMage;
        }

        protected override IAction Resolve() {
            var proposedItemMage = ResolveItemMage(Item) ?? ResolveItemMageForExo(Item);
            
            if (proposedItemMage != null && !SatisfiesOversinkConstraint(Item, proposedItemMage.Value))
                proposedItemMage = new ReduceOversinkItemMageResolve(config, Item).Resolve();
            
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
