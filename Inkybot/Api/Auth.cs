using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Inkybot.Events;
using Inkybot.Resources.Api;
using Newtonsoft.Json;
using static System.Configuration.ConfigurationManager;

namespace Inkybot.Api
{
    public class AuthManager
    {
        public static event EventHandler<ApiConnectionChangedEventArgs> ConnectionChanged;

        public static async Task<ApiConnection?> Login(string username, string password) {
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

        public static void Logout() {
            ConnectionChanged?.Invoke(null, new ApiConnectionChangedEventArgs(null));
        }
    }
}
