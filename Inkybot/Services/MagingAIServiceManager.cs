using System;
using Inkybot.Api;
using Inkybot.Design;
using Inkybot.Events;
using DofusMagingAIContract = Inkybot.Contracts.DofusMagingAI;

namespace Inkybot.Services
{
    public class MagingAIServiceManager : InjectableService
    {
        private ServiceContainer serviceContainer;
        private bool? previousUserFetchedIsFreeTrial;
        public event EventHandler<MagingAIChangedEventArgs> MagingAIChanged; 

        public void BindDependencies(ServiceContainer serviceContainer) {
            var apiClient = serviceContainer.GetService<ApiClient>();
            apiClient.UserFetched += OnUserFetched;
            this.serviceContainer = serviceContainer;
        }

        private void OnUserFetched(object sender, FetchedUserEventArgs e) {
            if (!e.user.is_free_trial && !e.user.is_subscribed)
                return;
            if (previousUserFetchedIsFreeTrial != null && previousUserFetchedIsFreeTrial == e.user.is_free_trial)
                return;
            var magus = !e.user.is_free_trial
                ? (DofusMagingAIContract) new DofusMagingAI()
                : (DofusMagingAIContract) new DofusStandardStatsMagingAI();

            if (magus is InjectableService dependant) {
                dependant.BindDependencies(serviceContainer);
            }

            serviceContainer.ReplaceService<DofusMagingAIContract>(magus);
            previousUserFetchedIsFreeTrial = e.user.is_free_trial;
            MagingAIChanged?.Invoke(this, new MagingAIChangedEventArgs(magus));
        }
    }
}
