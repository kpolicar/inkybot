using System;
using Inkybot.Contracts;
using Inkybot.Design;
using Inkybot.Dofus;
using Inkybot.Dofus.Contracts;
using Inkybot.Dofus.Domain;
using DofusMagingAIContract = Inkybot.Dofus.Contracts.DofusMagingAI;
using MageConfig = Inkybot.Dofus.MageConfig;

namespace Inkybot.Services
{
    public class DofusStandardStatsMagingAI : DofusMagingAIContract, HasDependencies
    {
        private MageConfig config;

        
        public override void BindDependencies(ServiceContainer serviceContainer) {
            var configManager = serviceContainer.GetService<MageConfigManager>();
            configManager.ConfigModified += (sender, args) => config = args.Config;
            base.BindDependencies(serviceContainer);
        }

        private ItemMage? ResolveItemMage(Item item, Stat[]? excludedStats=null) {
            var proposedMage =
                new TargetItemMageResolve(config, item).ExcludeStats(excludedStats).Resolve() ??
                new TargetItemMageResolve(config, item, 1).ExcludeStats(excludedStats).Resolve();

            return proposedMage;
        }

        protected override IAction Resolve() =>
            ResolveExcludingStats(new Stat[]{});

        protected override IAction ResolveExcludingStats(Stat[] stats) {
            var proposedItemMage = ResolveItemMage(Item, stats);

            if (proposedItemMage == null)
                return Finish();
            
            var itemMage = proposedItemMage.Value;

            return Combine(itemMage.Rune);
        }
    }
}
