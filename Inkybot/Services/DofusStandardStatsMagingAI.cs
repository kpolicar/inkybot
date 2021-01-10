using Inkybot.Contracts;
using Inkybot.Design;
using Inkybot.Dofus;
using DofusMagingAIContract = Inkybot.Contracts.DofusMagingAI;
using IAction = Inkybot.Domain.IAction;
using MageConfig = Inkybot.Dofus.MageConfig;

namespace Inkybot.Services
{
    public class DofusStandardStatsMagingAI : DofusMagingAIContract, HasDependencies
    {
        private ActionFactory actions = null!;
        private MageConfig config;

        
        public void BindDependencies(ServiceContainer serviceContainer) {
            actions = serviceContainer.GetService<ActionFactory>();
            
            var configManager = serviceContainer.GetService<ConfigManager>();
            configManager.ConfigModified += (sender, args) => config = args.Config;
        }

        private ItemMage? ResolveItemMage(Item item) {
            var proposedMage =
                new TargetItemMageResolve(config, item).Resolve() ??
                new TargetItemMageResolve(config, item, 1).Resolve();

            return proposedMage;
        }
        
        public IAction ResolveAction(Item item) {
            var proposedItemMage = ResolveItemMage(item);
            
            if (proposedItemMage == null)
                return actions.Finish(item);
            
            var itemMage = proposedItemMage.Value;

            return actions.CombineRune(itemMage.Rune, itemMage.Exo);
        }
    }
}
