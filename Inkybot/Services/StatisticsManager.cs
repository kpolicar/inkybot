using System;
using System.Collections.Generic;
using System.Linq;
using Inkybot.Actions;
using Inkybot.Api;
using Inkybot.Contracts;
using Inkybot.Design;
using Inkybot.Domain;
using Inkybot.Events;
using Newtonsoft.Json;
using StatisticsManagerContract = Inkybot.Contracts.StatisticsManager;

namespace Inkybot.Services
{
    public class StatisticsManager : StatisticsManagerContract, InjectableService
    {
        private ApiClient api = null!;
        private int changesCount = 0;
        const int MinChangesToSendCount = 10;
        
        private int balanceDifference = 0;
        private Dictionary<Stat, int> exoAttempts = new Dictionary<Stat, int>();
        private Dictionary<Stat, int> exoSuccesses = new Dictionary<Stat, int>();
        private IAction? previousAction;

        public void BindDependencies(ServiceContainer serviceContainer) {
            api = serviceContainer.GetService<ApiClient>();
            
            var actionHandler = serviceContainer.GetService<ActionHandler>();
            var magus = serviceContainer.GetService<DofusMagingJob>();
            magus.BalanceChanged += OnBalanceChanged;
            magus.Stopped += (sender, args) => Send();

            actionHandler.ActionExecuted += OnMagingAction;
        }

        private void OnMagingAction(object sender, ActionExecutedEventArgs e) {
            if (e.action is Finish finish &&
                previousAction is CombineRune previousCombine &&
                previousCombine.Exo &&
                finish.Item.Stats.ExoStats.Any(itemStat => itemStat.stat == previousCombine.Rune.stat)) {
                
                var stat = previousCombine.Rune.stat;
                if (exoSuccesses.ContainsKey(stat))
                    exoSuccesses[stat] += 1;
                else
                    exoSuccesses[stat] = 1;
            }

            if (e.action is CombineRune combine && combine.Exo) {
                var stat = combine.Rune.stat;
                if (exoAttempts.ContainsKey(stat))
                    exoAttempts[stat] += 1;
                else
                    exoAttempts[stat] = 1;
            }
            previousAction = e.action;
        }

        private void OnBalanceChanged(object sender, BalanceChangedEventArgs e) {
            changesCount++;
            balanceDifference += e.OldBalance - e.Balance;
            balanceDifference = Math.Max(balanceDifference, 0);

            if (changesCount >= MinChangesToSendCount)
                Send();
        }

        private void Send() {
            var exoAttemptsByIdentifier =
                exoAttempts.Select(pair => new KeyValuePair<string, int>(pair.Key.Identifier, pair.Value))
                    .ToDictionary(x => x.Key, x => x.Value);
            var exoSuccessesByIdentifier =
                exoSuccesses.Select(pair => new KeyValuePair<string, int>(pair.Key.Identifier, pair.Value))
                    .ToDictionary(x => x.Key, x => x.Value);
            
            var data = new[] {
                new KeyValuePair<string, string>("expend", balanceDifference.ToString()), 
                new KeyValuePair<string, string>("attempts_exo", JsonConvert.SerializeObject(exoAttemptsByIdentifier)), 
                new KeyValuePair<string, string>("successes_exo", JsonConvert.SerializeObject(exoSuccessesByIdentifier)), 
            };
            changesCount = 0;
            balanceDifference = 0;
            exoSuccesses = new Dictionary<Stat, int>();
            exoAttempts = new Dictionary<Stat, int>();
            _ = api.SendStatistics(data);
        }
    }
}
