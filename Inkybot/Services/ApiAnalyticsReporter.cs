using System;
using System.Collections.Generic;
using System.Linq;
using Inkybot.Actions;
using Inkybot.Api;
using Inkybot.Contracts;
using Inkybot.Design;
using Inkybot.Dofus;
using Inkybot.Events;
using Newtonsoft.Json;
using IAction = Inkybot.Domain.IAction;

namespace Inkybot.Services
{
    public class ApiAnalyticsReporter : AnalyticsReporter, HasDependencies
    {
        private ApiClient api = null!;
        private int changesCount = 0;
        const int MinChangesToSendCount = 10;
        private const int MaxReasonableBalanceDifference = 300000;
        
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
                finish.Item.Stats.ExoStats.Any(itemStat => itemStat.Stat == previousCombine.Rune.Stat)) {
                
                var stat = previousCombine.Rune.Stat;
                if (exoSuccesses.ContainsKey(stat))
                    exoSuccesses[stat] += 1;
                else
                    exoSuccesses[stat] = 1;
            }

            if (e.action is CombineRune combine && combine.Exo) {
                var stat = combine.Rune.Stat;
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
            if (balanceDifference > MaxReasonableBalanceDifference)
                balanceDifference = 0;

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
