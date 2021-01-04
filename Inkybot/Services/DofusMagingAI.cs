using System.Diagnostics;
using Inkybot.Contracts;
using Inkybot.Design;
using Inkybot.Dofus;
using DofusMagingJob = Inkybot.Contracts.DofusMagingJob;
using DofusMagingAIContract = Inkybot.Contracts.DofusMagingAI;
using IAction = Inkybot.Domain.IAction;
using MageConfig = Inkybot.Dofus.MageConfig;

namespace Inkybot.Services
{
    public class DofusMagingAI : DofusMagingAIContract, InjectableService
    {
        private ActionFactory actions = null!;
        private MageConfig config;
        private float sink;

        
        public void BindDependencies(ServiceContainer serviceContainer) {
            actions = serviceContainer.GetService<ActionFactory>();
            
            var configManager = serviceContainer.GetService<ConfigManager>();
            configManager.ConfigModified += (sender, args) => config = args.Config;
            
            var magingJob = serviceContainer.GetService<DofusMagingJob>();
            magingJob.SinkChanged += (sender, args) => sink = args.Sink;
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

            return proposedMage;
        }
        
        private ItemMage? ResolveItemMageForExo(Item item) {
            return new ExoItemMageResolve(config, item).Resolve();
        }

        public IAction ResolveAction(Item item) {
            var proposedItemMage = ResolveItemMage(item) ?? ResolveItemMageForExo(item);
            
            if (proposedItemMage == null)
                return actions.Finish(item);
            
            var itemMage = proposedItemMage.Value;

            Debug.WriteLine(
                $"Max of {itemMage.Stat.Identifier} is {itemMage.MageConfig.Maximum}, target is {itemMage.MageConfig.Target} stat will overmage: {itemMage.WillOvermage}"
                );
            
            return actions.CombineRune(itemMage.Rune, itemMage.Exo);
        }
    }
}
