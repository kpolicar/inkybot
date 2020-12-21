using System;
using System.Collections.Generic;
using Inkybot.Api;
using Inkybot.Contracts;
using Inkybot.Design;
using Inkybot.Events;
using StatisticsManagerContract = Inkybot.Contracts.StatisticsManager;

namespace Inkybot.Services
{
    public class StatisticsManager : StatisticsManagerContract, InjectableService
    {
        private ApiClient api;
        private int changesCount = 0;
        const int MinChangesToSendCount = 10;
        
        private int balanceDifference = 0;

        public void BindDependencies() {
            api = (ApiClient) Program.Services.GetService(typeof(ApiClient));
            var magus = (DofusMagingJob) Program.Services.GetService(typeof(DofusMagingJob));
            if (magus != null) {
                magus.BalanceChanged += OnBalanceChanged;
                magus.Stopped += (sender, args) => Send();
            }
        }

        private void OnBalanceChanged(object sender, BalanceChangedEventArgs e) {
            changesCount++;
            balanceDifference += e.OldBalance - e.Balance;
            balanceDifference = Math.Max(balanceDifference, 0);

            if (changesCount >= MinChangesToSendCount)
                Send();
        }

        private void Send() {
            var data = new[] {
                new KeyValuePair<string, string>("expend", balanceDifference.ToString()), 
            };
            changesCount = 0;
            balanceDifference = 0;
            _ = api.SendStatistics(data);
        }
    }
}
