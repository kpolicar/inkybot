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

        private ItemMage? ResolveItemMage(Item item) {
            var proposedMage =
                new TargetItemMageResolve(config, item).Resolve() ??
                new TargetItemMageResolve(config, item, 1).Resolve();

            return proposedMage;
        }
        
        protected override IAction Resolve() {
            var proposedItemMage = ResolveItemMage(Item);

            if (proposedItemMage == null)
                return Finish();
            
            var itemMage = proposedItemMage.Value;

            return Combine(itemMage.Rune);
        }
    }
}
