using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using WindowsFormsApp.Events;
using WindowsFormsApp.Exceptions;
using WindowsFormsApp.Resources.Api;
using Newtonsoft.Json;

namespace WindowsFormsApp.Api
{
    public class ApiDataProvider
    {
        public ApiConnection? Connection { private set; get; }

        public ApiDataProvider() {
            AuthManager.ConnectionChanged += OnConnectionChanged;
        }

        public event EventHandler<FetchedUserEventArgs> UserFetched;

        private void OnConnectionChanged(object sender, ApiConnectionChangedEventArgs e) {
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
            var response = await client.GetAsync("/api/user");
            response.EnsureSuccessStatusCode();

            var result = response.Content.ReadAsStringAsync().Result;
            var user = JsonConvert.DeserializeObject<User>(result);

            UserFetched?.Invoke(this, new FetchedUserEventArgs(user));
            return user;
        }

        public async Task<VersionDetails> NewestVersion() {
            var client = new HttpClient();
            var response = await client.GetAsync(Server.BaseUrl + "/api/client-version");
            response.EnsureSuccessStatusCode();

            var result = response.Content.ReadAsStringAsync().Result;
            return JsonConvert.DeserializeObject<VersionDetails>(result);
        }
    }
}
