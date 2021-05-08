using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using Inkybot.Events;
using Inkybot.Api.Resources;
using Inkybot.Contracts;
using Inkybot.Design;
using Inkybot.Dofus.Contracts;
using Inkybot.Domain;
using Inkybot.Exceptions;
using Newtonsoft.Json;

namespace Inkybot.Api
{
    public class ApiAuthManager : AuthManager, HasDependencies
    {
        public event EventHandler<ApiConnectionChangedEventArgs>? ConnectionChanged;

        private DofusMagingJob magingJob = null!;
        private MageConfigManager configManager;

        public User? User {
            private set; get;
        }
        
        public void BindDependencies(ServiceContainer serviceContainer) {
            var apiClient = serviceContainer.GetService<ApiClient>();
            apiClient.UserFetched += (sender, args) => User = args.user;
            magingJob = serviceContainer.GetService<DofusMagingJob>();
            configManager = serviceContainer.GetService<MageConfigManager>();
            magingJob.Starting += OnMagingJobStart;
            magingJob.Started += OnMagingJobStart;
        }

        private void OnMagingJobStart(object sender, EventArgs e) {
            if ((User?.canMageExos ?? true) || User?.numberOfExoMagesLeftInPlan >= 1) {
                return;
            }
            
            if (configManager.Config?.Exos.Any() ?? false) {
                EnforceUserHasPermissionToMageExo();
            }
        }

        public void EnforceUserHasPermissionToMageExo(string? text = null, string? caption = null) {
            if (User != null && (!User.canMageExos || User.numberOfExoMagesLeftInPlan < 1)) {
                text ??= (User.canMageExos, User.is_free_trial, User.numberOfExoMagesLeftInPlan) switch {
                    (_, true, _) => "This feature is restricted to subscribed users! Visit the official Inkybot website to subscribe your account.",
                    (_, false, < 1) => "You have reached the limit for the number of exos you can mage with your pricing plan. Visit the official Inkybot website to upgrade plans.",
                    _ => "This feature is not available on your current pricing plan. Visit the official Inkybot website to upgrade plans.",
                };
                caption ??= "Feature Restricted";
                
                MessageBox.Show(
                    text,
                    caption,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                
                throw new UserForbiddenException(text);
            }
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
