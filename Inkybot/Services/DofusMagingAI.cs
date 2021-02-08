using System;
using System.Diagnostics;
using Inkybot.Contracts;
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

        private ItemMage? ResolveItemMage(Item item) {
            var proposedMage =
                new TargetItemMageResolve(config, item).Resolve() ??
                new TargetItemMageResolve(config, item, 1).Resolve() ??
                new OverTargetItemMageResolve(config, item).Resolve();
            
                
            if (!item.IsOvermaged && !item.HasExo)
                proposedMage ??=
                    new PerfectionItemMageResolve(config, item, sink).Resolve() ??
                    new PerfectionItemMageResolve(config, item, sink, 1).Resolve();
            
            proposedMage ??=
                new OverMageToReachTargetMinimumItemMageResolve(config, item).Resolve() ??
                new OverMageToReachTargetWithSinkItemMageResolve(config, item, sink).Resolve();

            return proposedMage;
        }
        
        private ItemMage? ResolveItemMageForExo(Item item) {
            return new ExoItemMageResolve(config, item, sink).Resolve();
        }

        public override IAction Resolve(Item item) {
            var proposedItemMage = ResolveItemMage(item) ?? ResolveItemMageForExo(item);
            
            if (proposedItemMage != null && !SatisfiesOversinkConstraint(item, proposedItemMage.Value))
                proposedItemMage = new ReduceOversinkItemMageResolve(config, item).Resolve();
            
            if (proposedItemMage == null)
                return Finish();
            
            var itemMage = proposedItemMage.Value;

            Debug.WriteLine(
                $"Max of {itemMage.Stat.DisplayName} is {itemMage.MageConfig.Maximum}, target is {itemMage.MageConfig.Target} stat will overmage: {itemMage.WillOvermage}"
                );
            
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
