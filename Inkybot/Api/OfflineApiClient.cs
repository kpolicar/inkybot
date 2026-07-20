using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using Inkybot.Api.Resources;
using Inkybot.Dofus;

namespace Inkybot.Api
{
    /// <summary>
    ///     Offline stand-in for <see cref="ApiClient"/>. Serves dummy data from
    ///     <see cref="OfflineMockData"/> and turns every reporting call into a no-op, so the app
    ///     runs with no inkybot.me backend and no network. Swapped in for the real client via DI
    ///     in Program.cs when <see cref="Program.OfflineMode"/> is set; point that registration
    ///     back at <see cref="ApiClient"/> to restore online behaviour.
    /// </summary>
    public class OfflineApiClient : ApiClient
    {
        public override Task<User> User() {
            var user = OfflineMockData.MockUser();
            RaiseUserFetched(user);
            return Task.FromResult(user);
        }

        public override Task<FreeTrial> BeginFreeTrial() =>
            Task.FromResult(OfflineMockData.MockFreeTrial());

        public override Task<VersionDetails> NewestVersion() =>
            Task.FromResult(OfflineMockData.MockVersionDetails());

        public override Task SendStatistics(Dictionary<string, string> data) => Task.CompletedTask;

        public override Task Publish(Image image, bool toForum) => Task.CompletedTask;

        public override Task NotifyActionNeeded() => Task.CompletedTask;

        public override Task NotifyFinished() => Task.CompletedTask;

        public override Task NotifyError() => Task.CompletedTask;

        public override Task NotifyOutOfRunes(Rune rune) => Task.CompletedTask;
    }
}
