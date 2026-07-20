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
using ImageMagick.Drawing;
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
        private MageQueueManager mageQueue = null!;
        public ApiConnection? Connection { private set; get; }

        public event EventHandler<FetchedUserEventArgs>? UserFetched;

        public void BindDependencies(ServiceContainer serviceContainer) {
            auth = serviceContainer.GetService<AuthManager>();
            mageQueue = serviceContainer.GetService<MageQueueManager>();
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

        // Raises UserFetched so offline/mock subclasses (which cannot invoke a base-class event
        // directly) can notify listeners after producing a User without a network round-trip.
        protected void RaiseUserFetched(User user) {
            UserFetched?.Invoke(this, new FetchedUserEventArgs(user));
        }

        public virtual async Task<User> User() {
            await WaitForStableConnection();

            var client = Connection!.Request();
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            var response = await client.GetAsync($"{Server.ApiUrl}/user");
            response.EnsureSuccessStatusCode();
            var result = await GetResultFromEncryptedResponse(response);

            Debug.WriteLine("Http response: "+result);
            var user = JsonConvert.DeserializeObject<User>(result);

            UserFetched?.Invoke(this, new FetchedUserEventArgs(user));
            return user;
        }

        public virtual async Task<FreeTrial> BeginFreeTrial() {
            await WaitForStableConnection();

            var response = await Connection!.Request()
                .PostAsync($"{Server.ApiUrl}/trial/begin", new StringContent(""));
            response.EnsureSuccessStatusCode();
            var result = await GetResultFromEncryptedResponse(response);

            return JsonConvert.DeserializeObject<FreeTrial>(result);
        }

        public virtual async Task<VersionDetails> NewestVersion() {
            var client = new HttpClient();
            var response = await client.GetAsync(Server.ApiUrl);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsStringAsync();

            Debug.WriteLine("Http response: "+result);
            return JsonConvert.DeserializeObject<VersionDetails>(result);
        }

        public virtual async Task SendStatistics(Dictionary<string,string> data) {
            if (!auth.User?.canCreateStatistics ?? false)
                return;
            Debug.WriteLine("Sending statistics to server:"+string.Join("; ", data));
            var encrypted = Aes256CbcEncrypter.Encrypt(data);
            var content = new StringContent(encrypted);

            await WaitForStableConnection();

            await Connection!.Request()
                .PostAsync($"{Server.ApiUrl}/statistics", content);
        }

        public virtual async Task Publish(Image image, bool toForum) {
            if (!auth.User?.canPublishExos ?? false)
                return;
            using var ms = new MemoryStream();
            image.Save(ms, ImageFormat.Bmp);
            ms.Position = 0;

            var b =
                Responsive.ResponsiveRectangle(Measurements.MagingTable, image.Width, image.Height);
            var characterDetails =
                Responsive.ResponsiveRectangle(Measurements.MagingTableCharacterDetails, image.Width, image.Height);
            var optimizer = new ImageOptimizer();
            using var compressedImage = new MagickImage(ms);
            var fillerColor = compressedImage.GetPixels().GetPixel(characterDetails.X, characterDetails.Y).ToColor() ?? new MagickColor("#393D58");
            var drawables = new Drawables()
                .FillColor(fillerColor) // Set the fill color
                .Rectangle(characterDetails.X, characterDetails.Y, characterDetails.X+characterDetails.Width, characterDetails.Y+characterDetails.Height); // Draw a rectangle (top-left: 50, 50; bottom-right: 200, 200)
            drawables.Draw(compressedImage);
            compressedImage.Crop(new MagickGeometry(b.X, b.Y, (uint)b.Width, (uint)b.Height));
            compressedImage.SetCompression(CompressionMethod.JPEG);
            compressedImage.Resize(849,750);
            compressedImage.Extent(849,750, Gravity.Center, new MagickColor("#000000"));
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

        public virtual async Task NotifyActionNeeded() {
            await WaitForStableConnection();
            Connection?.Request()
                .PostAsync($"{Server.ApiUrl}/notify/actionneeded", new StringContent(""));
        }

        public virtual async Task NotifyFinished() {
            var data = new[] {
                new KeyValuePair<string,string>("continueQueue", (mageQueue.Count > 1).ToString())
            };
            await WaitForStableConnection();
            Connection?.Request()
                .PostAsync($"{Server.ApiUrl}/notify/finished", new FormUrlEncodedContent(data));
        }

        public virtual async Task NotifyError() {
            await WaitForStableConnection();
            Connection?.Request()
                .PostAsync($"{Server.ApiUrl}/notify/error", new StringContent(""));
        }

        public virtual async Task NotifyOutOfRunes(Rune rune) {
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
