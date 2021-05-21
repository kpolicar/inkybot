using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using ImageMagick;
using Inkybot.Events;
using Inkybot.Exceptions;
using Inkybot.Api.Resources;
using Inkybot.Contracts;
using Inkybot.Design;
using Inkybot.Dofus;
using Inkybot.Domain;
using Inkybot.Helpers;
using Inkybot.Services;
using Newtonsoft.Json;
using Debug = System.Diagnostics.Debug;

namespace Inkybot.Api
{
    public class ApiClient : HasDependencies
    {
        private AuthManager auth = null!;
        public ApiConnection? Connection { private set; get; }

        public event EventHandler<FetchedUserEventArgs>? UserFetched;

        public void BindDependencies(ServiceContainer serviceContainer) {
            auth = serviceContainer.GetService<AuthManager>();
            auth.ConnectionChanged += OnConnectionChanged;
        }
        
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

            var client = Connection!.Request();
            var response = await client.GetAsync($"{Server.ApiUrl}/user");
            response.EnsureSuccessStatusCode();
            var result = await GetResultFromEncryptedResponse(response);

            Debug.WriteLine("Http response: "+result);
            var user = JsonConvert.DeserializeObject<User>(result);

            UserFetched?.Invoke(this, new FetchedUserEventArgs(user));
            return user;
        }
        
        public async Task<FreeTrial> BeginFreeTrial() {
            await WaitForStableConnection();
            
            var response = await Connection!.Request()
                .PostAsync($"{Server.ApiUrl}/trial/begin", new StringContent(""));
            response.EnsureSuccessStatusCode();
            var result = await GetResultFromEncryptedResponse(response);
            
            return JsonConvert.DeserializeObject<FreeTrial>(result);
        }

        public async Task<VersionDetails> NewestVersion() {
            var client = new HttpClient();
            var response = await client.GetAsync(Server.ApiUrl);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsStringAsync();

            Debug.WriteLine("Http response: "+result);
            return JsonConvert.DeserializeObject<VersionDetails>(result);
        }

        public async Task SendStatistics(Dictionary<string,string> data) {
            if (!auth.User?.canCreateStatistics ?? false)
                return;
            Debug.WriteLine("Sending statistics to server:"+string.Join("; ", data));
            var encrypted = Aes256CbcEncrypter.Encrypt(data);
            var content = new StringContent(encrypted);
            
            await WaitForStableConnection();
            
            await Connection!.Request()
                .PostAsync($"{Server.ApiUrl}/statistics", content);
        }

        public async Task Publish(Image image, bool toForum) {
            if (!auth.User?.canPublishExos ?? false)
                return;
            using var ms = new MemoryStream();
            image.Save(ms, ImageFormat.Bmp);
            ms.Position = 0;
            
            var b =
                Responsive.ResponsiveRectangle(Measurements.MagingTable, image.Width, image.Height);
            var optimizer = new ImageOptimizer();
            using var compressedImage = new MagickImage(ms);
            compressedImage.Crop(new MagickGeometry(b.X, b.Y, b.Width, b.Height));
            compressedImage.SetCompression(CompressionMethod.JPEG);
            compressedImage.Resize(new MagickGeometry(908,750));
            compressedImage.Write(ms, MagickFormat.Jpeg);
            ms.Position = 0;
            optimizer.LosslessCompress(ms);
            ms.Position = 0;

            using var formData = new MultipartFormDataContent();
            
            using var fileStreamContent = new StreamContent(ms);
            fileStreamContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
            var name = $"{DateTime.Now:yyyy-MM-dd_hh-mm-ss}.jpg";

            using var publishToForum = new StringContent(toForum ? "1" : "0");
            formData.Add(fileStreamContent, "image", name);
            formData.Add(publishToForum, "publish_to_forum");
            
            await WaitForStableConnection();
            
            var client = Connection?.Request();
            client?.DefaultRequestHeaders.Add("Authorization-Signature", Server.Signature);
            
            await client?.PostAsync($"{Server.ApiUrl}/publish", formData)!;
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

        private async Task<string> GetResultFromEncryptedResponse(HttpResponseMessage response) =>
            Aes256CbcEncrypter.Decrypt(await response.Content.ReadAsStringAsync());
    }
}
