using System;
using System.Diagnostics;
using System.Threading.Tasks;
using WindowsFormsApp.Events;
using WindowsFormsApp.Resources.Api;
using Newtonsoft.Json;

namespace WindowsFormsApp.Api
{
    public class ApiDataProvider
    {
        private ApiConnection connection;

        public ApiDataProvider() {
            Auth.ConnectionChanged += OnConnectionChanged;
        }

        public event EventHandler<FetchedUserEventArgs> UserFetched;

        private void OnConnectionChanged(object sender, ApiConnectionChangedEventArgs e) {
            connection = e.connection;
        }

        public async Task<User> User() {
            if (connection.RefreshTask != null && !connection.RefreshTask.IsCompleted) {
                Debug.WriteLine("waiting for refresh task");
                await connection.RefreshTask;
                Debug.WriteLine("done waiting for refresh task");
            }

            var client = connection.Request();
            var response = await client.GetAsync("/api/user");
            response.EnsureSuccessStatusCode();

            var result = response.Content.ReadAsStringAsync().Result;
            var user = JsonConvert.DeserializeObject<User>(result);

            UserFetched?.Invoke(this, new FetchedUserEventArgs(user));
            return user;
        }
    }
}
