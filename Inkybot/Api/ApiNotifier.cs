using System.Diagnostics;
using Inkybot.Actions;
using Inkybot.Contracts;
using Inkybot.Design;
using Inkybot.Events;
using Inkybot.Exceptions;
using Inkybot.Services;

#pragma warning disable 4014
namespace Inkybot.Api
{
    public class ApiNotifier : HasDependencies
    {
        private ApiClient api = null!;

        public void BindDependencies(ServiceContainer serviceContainer) {
            api = serviceContainer.GetService<ApiClient>();
            var actions = serviceContainer.GetService<ActionHandler>();
            var magingJob = serviceContainer.GetService<DofusMagingJob>();
            
            actions!.ActionExecuted += Notify;
            magingJob!.Error += Notify;
            if (magingJob is ScreenReaderDofusMagingJob screenReaderDofusMagingJob)
                screenReaderDofusMagingJob.SensitiveMage += OnSensitiveMage;
        }

        private void OnSensitiveMage(object sender, MagingJobStartedEventArgs e) {
            if (e.Interrupted) {
                _ = api.NotifyActionNeeded();
            }
        }

        public void Notify(object sender, ActionExecutedEventArgs e) {
            if (e.action is Finish) {
                _ = api.NotifyFinished();
            }
        }
        
        public void Notify(object sender, MagingJobErrorEventArgs e) {
            _ = e.exception switch {
                OutOfRunesException exception => api.NotifyOutOfRunes(exception.Rune),
                _ => api.NotifyError(),
            };
        }
    }
}
#pragma warning restore 4014
