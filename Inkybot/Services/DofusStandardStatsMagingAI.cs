using Inkybot.Contracts;
using Inkybot.Design;
using Inkybot.Domain;
using DofusMagingJob = Inkybot.Contracts.DofusMagingJob;
using DofusMagingAIContract = Inkybot.Contracts.DofusMagingAI;

namespace Inkybot.Services
{
    public class DofusStandardStatsMagingAI : DofusMagingAIContract, InjectableService
    {
        private ActionFactory actions;
        private Config config;

        
        public void BindDependencies(ServiceContainer serviceContainer) {
            actions = (ActionFactory) serviceContainer.GetService(typeof(ActionFactory));
            
            var configManager = serviceContainer.GetService<ConfigManager>();
            configManager.ConfigModified += (sender, args) => config = args.Config;
        }

        private ItemMage? ResolveItemMage(Item item) {
            var proposedMage =
                new StandardItemMageResolve(config, item).Resolve() ??
                new StandardItemMageResolve(config, item, 1).Resolve();

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
