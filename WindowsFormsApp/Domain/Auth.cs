using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using WindowsFormsApp.Resources.Api;
using Newtonsoft.Json;

namespace WindowsFormsApp
{
    public class Auth
    {
        private AuthDetails authDetails;

        public async Task<bool> Login(string username, string password)
        {
            var client = new HttpClient();
            const string url = Server.BaseUrl+ "/oauth/token";

            var form_params = new Dictionary<string,string>(){
                {"grant_type", "password"},
                {"username", username},
                {"password", password},
                {"client_id","2"},
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

        public async void User()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", authDetails.access_token);
            const string url = Server.BaseUrl+ "/api/user";
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();    // Throw if not a success code.
            
            Debug.WriteLine(response.Content.ReadAsStringAsync().Result);
        }
    }
}