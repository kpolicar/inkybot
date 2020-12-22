using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Inkybot.Actions;
using Inkybot.Contracts;
using Inkybot.Design;
using Inkybot.Domain;
using Inkybot.Domain.Repositories;
using Inkybot.Events;
using Inkybot.Exceptions;
using Inkybot.Services;
using DofusMagingJob = Inkybot.Contracts.DofusMagingJob;
using DofusMagingAIContract = Inkybot.Contracts.DofusMagingAI;

namespace Inkybot.Services
{
    public class DofusMagingAI : DofusMagingAIContract, InjectableService
    {
        private ActionFactory actions;
        private Config config;
        private float sink;

        
        public void BindDependencies() {
            actions = (ActionFactory) Program.Services.GetService(typeof(ActionFactory));
            
            var configManager = Program.Services.GetService<ConfigManager>();
            configManager.ConfigModified += (sender, args) => config = args.Config;
            
            var magingJob = Program.Services.GetService<DofusMagingJob>();
            magingJob.SinkChanged += (sender, args) => sink = args.Sink;
        }

        private ItemMage? ResolveItemMage(Item item) {
            var proposedMage =
                new StandardItemMageResolve(config, item).Resolve() ??
                new StandardItemMageResolve(config, item, 1).Resolve();
                
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
                $"Max of {itemMage.Stat.DisplayName} is {itemMage.MageConfig.Maximum}, target is {itemMage.MageConfig.Target} stat will overmage: {itemMage.WillOvermage}"
                );
            
            return actions.CombineRune(itemMage.Rune, itemMage.Exo);
        }
    }
}
