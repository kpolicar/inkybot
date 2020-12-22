using Inkybot.Actions;
using Inkybot.Api;
using Inkybot.Events;
using Inkybot.Exceptions;

#pragma warning disable 4014
namespace Inkybot.Api
{
    public class ApiNotifier
    {
        private ApiClient api;

        public ApiNotifier() {
            api = (ApiClient) Program.Services.GetService(typeof(ApiClient));
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
