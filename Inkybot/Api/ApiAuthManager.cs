using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Inkybot.Events;
using Inkybot.Api.Resources;
using Inkybot.Contracts;
using Inkybot.Design;
using Inkybot.Domain;
using Newtonsoft.Json;

namespace Inkybot.Api
{
    public class ApiAuthManager : AuthManager, HasDependencies
    {
        public event EventHandler<ApiConnectionChangedEventArgs>? ConnectionChanged;

        public User? User {
            private set; get;
        }
        
        public void BindDependencies(ServiceContainer serviceContainer) {
            var apiClient = serviceContainer.GetService<ApiClient>();
            apiClient.UserFetched += (sender, args) => User = args.user;
        }

        public async Task<ApiConnection?> Login(string username, string password) {
            var client = new HttpClient();
            var url =  $"{Server.AuthUrl}/token";

            var form_params = new Dictionary<string, string> {
                {"grant_type", "password"},
                {"username", username},
                {"password", password},
                {"client_id", Program.GrantId},
                {"client_secret", Program.GrantSecret},
                {"scope", ""},
                {"_passport_token_name", Program.InstanceIdentifier},
            };
            var encrypted = Aes256CbcEncrypter.Encrypt(form_params);

            var content = new StringContent(encrypted);
            var response = await client.PostAsync(url, content);
            
            if (!response.IsSuccessStatusCode)
                return null;
            
            var result = await GetResultFromEncryptedResponse(response);
            var authDetails = JsonConvert.DeserializeObject<AuthDetails>(result);
            var connection = new ApiConnection(authDetails);
            System.Diagnostics.Debug.WriteLine("Http response: "+result);

            ConnectionChanged?.Invoke(null, new ApiConnectionChangedEventArgs(connection));
            return connection;
        }

        public void Logout() {
            ConnectionChanged?.Invoke(null, new ApiConnectionChangedEventArgs(null));
        }
        
        private async Task<string> GetResultFromEncryptedResponse(HttpResponseMessage response) =>
            Aes256CbcEncrypter.Decrypt(await response.Content.ReadAsStringAsync());
    }
}
