using Inkybot.Actions;
using Inkybot.Design;
using Inkybot.Events;
using Inkybot.Exceptions;

#pragma warning disable 4014
namespace Inkybot.Api
{
    public class ApiNotifier : HasDependencies
    {
        private ApiClient api = null!;

        public void BindDependencies(ServiceContainer serviceContainer) {
            api = serviceContainer.GetService<ApiClient>();
        }
        
        public void Notify(object sender, ActionExecutedEventArgs e) {
            if (e.action is Finish) {
                api.NotifyFinished();
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
