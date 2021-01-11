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
                {"scope", ""}
            };
            var content = new FormUrlEncodedContent(form_params);
            var response = await client.PostAsync(url, content);
            
            if (!response.IsSuccessStatusCode)
                return null;
            
            var result = response.Content.ReadAsStringAsync().Result;
            var authDetails = JsonConvert.DeserializeObject<AuthDetails>(result);
            var connection = new ApiConnection(authDetails);

            ConnectionChanged?.Invoke(null, new ApiConnectionChangedEventArgs(connection));
            return connection;
        }

        public void Logout() {
            ConnectionChanged?.Invoke(null, new ApiConnectionChangedEventArgs(null));
        }
    }
}
