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

namespace Inkybot.Services
{
    public class BasicDofusMagingAI : DofusMagingAI, InjectableService
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
            var proposedMage = new StandardItemMageResolve(config, item).Resolve();
            if (proposedMage == null)
                proposedMage = new StandardItemMageResolve(config, item, 1).Resolve();

            return proposedMage;
        }
        
        private ItemMage? ResolveItemMageForExo(Item item) {
            return new ExoItemMageResolve(config, item).Resolve();
        }

        public IAction ResolveAction(Item item, IAction previousAction) {
            Debug.WriteLine("has this many exos: "+item.Stats.ExoStats.Length);

            var proposedItemMage = ResolveItemMage(item) ?? ResolveItemMageForExo(item);;
            
            if (proposedItemMage == null)
                return actions.Finish();
            
            var itemMage = proposedItemMage.Value;

            Debug.WriteLine(
                $"Max of {itemMage.Stat.DisplayName} is {itemMage.MageConfig.Maximum}, stat will overmage: {itemMage.WillOvermage}"
                );
            
            var selectRune = new Func<IAction>(() => actions.SelectRune(itemMage.Rune));

            if (previousAction == null)
                return selectRune();
            
            if (previousAction is Combine previousCombine) {
                return selectRune();
                if (previousCombine.Exo)
                    return selectRune();
                    
                if (previousCombine.Rune.stat != itemMage.Stat || previousCombine.Rune.type != itemMage.Rune.type)
                    return selectRune();
            }

            return actions.Combine(itemMage.Rune, itemMage.Exo);
        }
    }
}
