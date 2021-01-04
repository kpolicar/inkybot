using Inkybot.Contracts;
using Inkybot.Design;
using Inkybot.Domain;
using DofusMagingJob = Inkybot.Contracts.DofusMagingJob;
using DofusMagingAIContract = Inkybot.Contracts.DofusMagingAI;

namespace Inkybot.Services
{
    public class DofusStandardStatsMagingAI : DofusMagingAIContract, InjectableService
    {
        private ActionFactory actions = null!;
        private Config config = null!;

        
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
