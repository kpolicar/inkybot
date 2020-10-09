using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using WindowsFormsApp.Events;
using WindowsFormsApp.Resources.Api;
using Newtonsoft.Json;

namespace WindowsFormsApp.Services
{
    public class ApiDataProvider
    {
        public event EventHandler<FetchedUserEventArgs> UserFetched;
        private Auth auth;

        public ApiDataProvider()
        {
            auth = (Auth) Program.Services.GetService(typeof(Auth));
        }
        
        public async Task<User> User()
        {
            var client = auth.RequestClient();
            var response = await client.GetAsync("/api/user");
            response.EnsureSuccessStatusCode();
            
            var result = response.Content.ReadAsStringAsync().Result;
            var user = JsonConvert.DeserializeObject<User>(result);
            
            UserFetched?.Invoke(this, new FetchedUserEventArgs(user));
            return user;
        }
    }
}