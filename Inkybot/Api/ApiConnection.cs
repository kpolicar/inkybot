using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Inkybot.Api.Resources;
using Inkybot.Domain;
using Newtonsoft.Json;
using Timer = System.Windows.Forms.Timer;

namespace Inkybot.Api
{
    public class ApiConnection
    {
        private const int AuthCheckRequestMaxAttempts = 3;
        private int AuthCheckRequestAttempts = 0;
        
        public static AuthDetails AuthDetails {
            get; private set;
        }
        private static Timer? refreshTokenTimer;

        public ApiConnection(AuthDetails authDetails) {
            AuthDetails = authDetails;
            refreshTokenTimer?.Dispose();
            refreshTokenTimer = new Timer();
            refreshTokenTimer.Interval = 53000;
            refreshTokenTimer.Tick += OnRefreshTokenTimer;
            refreshTokenTimer.Start();
        }

        public void Terminate() {
            //refreshTokenTimer?.Dispose();
        }

        public Task? RefreshTask { private set; get; }

        public HttpClient Request() {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", AuthDetails.access_token);
            client.BaseAddress = new Uri(Server.BaseUrl);
            return client;
        }

        private void OnRefreshTokenTimer(object sender, EventArgs eventArgs) {
            RefreshTask = RefreshToken();
        }

        public async Task<bool> RefreshToken() {
            var client = new HttpClient();
            var url = $"{Server.AuthUrl}/token";

            var form_params = new Dictionary<string, string> {
                {"grant_type", "refresh_token"},
                {"refresh_token", AuthDetails.refresh_token},
                {"client_id", Program.GrantId},
                {"client_secret", Program.GrantSecret},
                {"scope", ""},
                {"_passport_token_name", Program.InstanceIdentifier},
            };
            AuthCheckRequestAttempts++;
            
            try {
                var encrypted = Aes256CbcEncrypter.Encrypt(form_params);

                var content = new StringContent(encrypted);
                var response = await client.PostAsync(url, content);
                
                response.EnsureSuccessStatusCode();

                var result = await GetResultFromEncryptedResponse(response);
                Debug.WriteLine("Http response: " + result);
                AuthDetails = JsonConvert.DeserializeObject<AuthDetails>(result);
            } catch (Exception) {
                if (AuthCheckRequestAttempts < AuthCheckRequestMaxAttempts) {
                    Debug.WriteLine("RefreshToken reattempt "+AuthCheckRequestAttempts);
                    await Task.Delay(1741);
                    return await RefreshToken();
                }
                return false;
            }
            
            AuthCheckRequestAttempts = 0;
            return true;
        }
        
        private async Task<string> GetResultFromEncryptedResponse(HttpResponseMessage response) =>
            Aes256CbcEncrypter.Decrypt(await response.Content.ReadAsStringAsync());
    }
}
