using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Timers;
using WindowsFormsApp.Resources.Api;
using Newtonsoft.Json;

namespace WindowsFormsApp.Api
{
    public class ApiConnection
    {
        private AuthDetails authDetails;
        private readonly Timer refreshTokenTimer;

        public ApiConnection(AuthDetails authDetails) {
            this.authDetails = authDetails;
            refreshTokenTimer = new Timer(60000D);
            refreshTokenTimer.Elapsed += OnRefreshTokenTimer;
        }

        public Task RefreshTask { private set; get; }

        public HttpClient Request() {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", authDetails.access_token);
            client.BaseAddress = new Uri(Server.BaseUrl);
            return client;
        }

        private void OnRefreshTokenTimer(object sender, ElapsedEventArgs e) {
            RefreshTask = RefreshToken();
            RefreshTask.Start();
        }

        public async Task<bool> RefreshToken() {
            var client = new HttpClient();
            const string url = Server.BaseUrl + "/oauth/token";

            var form_params = new Dictionary<string, string> {
                {"grant_type", "refresh_token"},
                {"refresh_token", authDetails.refresh_token},
                {"client_id", "2"},
                {"client_secret", "***REMOVED***"},
                {"scope", ""}
            };
            var content = new FormUrlEncodedContent(form_params);
            var response = await client.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
                return false;

            var result = response.Content.ReadAsStringAsync().Result;
            authDetails = JsonConvert.DeserializeObject<AuthDetails>(result);
            return true;
        }
    }
}
