using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Inkybot.Events;
using Inkybot.Exceptions;
using Inkybot.Resources.Api;
using Newtonsoft.Json;

namespace Inkybot.Api
{
    public class ApiDataProvider
    {
        public ApiConnection? Connection { private set; get; }

        public ApiDataProvider() {
            AuthManager.ConnectionChanged += OnConnectionChanged;
        }

        public event EventHandler<FetchedUserEventArgs> UserFetched;

        private void OnConnectionChanged(object sender, ApiConnectionChangedEventArgs e) {
            Connection?.Terminate();
            Connection = e.connection;
        }

        private async Task WaitForStableConnection() {
            if (Connection == null)
                throw new ApiConnectionNotEstablishedException();
            if (Connection.RefreshTask != null && !Connection.RefreshTask.IsCompleted)
                await Connection.RefreshTask;
        }

        public async Task<User> User() {
            await WaitForStableConnection();

            var client = Connection.Request();
            var response = await client.GetAsync($"{Server.ApiUrl}/user");
            response.EnsureSuccessStatusCode();

            var result = response.Content.ReadAsStringAsync().Result;
            var user = JsonConvert.DeserializeObject<User>(result);

            UserFetched?.Invoke(this, new FetchedUserEventArgs(user));
            return user;
        }

        public async Task<VersionDetails> NewestVersion() {
            var client = new HttpClient();
            var response = await client.GetAsync($"{Server.ApiUrl}/version");
            response.EnsureSuccessStatusCode();

            var result = response.Content.ReadAsStringAsync().Result;
            return JsonConvert.DeserializeObject<VersionDetails>(result);
        }
    }
}
