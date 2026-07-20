using System.Threading.Tasks;

namespace Inkybot.Api
{
    /// <summary>
    ///     Offline <see cref="ApiAuthManager"/>: logs in instantly against a fabricated connection,
    ///     with no network, credentials, or encryption. Everything else — the UserFetched → User
    ///     wiring, exo-permission enforcement, and the (toggleable) OTLP telemetry hook — is
    ///     inherited unchanged from the base class. Swapped in via DI in Program.cs when
    ///     <see cref="Program.OfflineMode"/> is set.
    /// </summary>
    public class OfflineApiAuthManager : ApiAuthManager
    {
        public override Task<ApiConnection?> Login(string username, string password) {
            var connection = new OfflineApiConnection(OfflineMockData.MockAuthDetails());
            RaiseConnectionChanged(connection);
            return Task.FromResult<ApiConnection?>(connection);
        }
    }
}
