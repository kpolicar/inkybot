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

        
        public void BindDependencies(ServiceContainer serviceContainer) {
            var configManager = serviceContainer.GetService<ConfigManager>();
            configManager.ConfigModified += (sender, args) => config = args.Config;
        }

        private ItemMage? ResolveItemMage(Item item) {
            var proposedMage =
                new TargetItemMageResolve(config, item).Resolve() ??
                new TargetItemMageResolve(config, item, 1).Resolve();

            return proposedMage;
        }
        
        public override IAction ResolveAction(Item item) {
            var proposedItemMage = ResolveItemMage(item);
            
            if (proposedItemMage == null)
                return Action.Finish(item);
            
            var itemMage = proposedItemMage.Value;

            return Action.CombineRune(itemMage.Rune, itemMage.Exo);
        }
    }
}
