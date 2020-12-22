using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Inkybot.Events;
using Inkybot.Exceptions;
using Inkybot.Api.Resources;
using Inkybot.Domain;
using Newtonsoft.Json;

namespace Inkybot.Api
{
    public class ApiClient
    {
        public ApiConnection? Connection { private set; get; }

        public ApiClient() {
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
            var response = await client.GetAsync(Server.ApiUrl);
            response.EnsureSuccessStatusCode();

            var result = response.Content.ReadAsStringAsync().Result;
            return JsonConvert.DeserializeObject<VersionDetails>(result);
        }

        public async Task SendStatistics(IEnumerable<KeyValuePair<string, string>> data) {
            await WaitForStableConnection();
            Connection?.Request()
                .PostAsync($"{Server.ApiUrl}/statistics", new FormUrlEncodedContent(data));
        }

        public async Task NotifyFinished() {
            await WaitForStableConnection();
            Connection?.Request()
                .PostAsync($"{Server.ApiUrl}/notify/finished", new StringContent(""));
        }

        public async Task NotifyError() {
            await WaitForStableConnection();
            Connection?.Request()
                .PostAsync($"{Server.ApiUrl}/notify/error", new StringContent(""));
        }

        public async Task NotifyOutOfRunes(Rune rune) {
            var data = new[] {
                new KeyValuePair<string, string>("rune", rune.ToString()), 
            };
            await WaitForStableConnection();
            Connection?.Request()
                .PostAsync($"{Server.ApiUrl}/notify/runes", new FormUrlEncodedContent(data));
        }
    }
}
