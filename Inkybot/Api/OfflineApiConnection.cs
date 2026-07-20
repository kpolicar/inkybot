using System.Threading.Tasks;
using Inkybot.Api.Resources;

namespace Inkybot.Api
{
    /// <summary>
    ///     Offline <see cref="ApiConnection"/> whose token refresh is a no-op, so the periodic
    ///     refresh timer never hits the (dead) OAuth endpoint. Created by
    ///     <see cref="OfflineApiAuthManager"/>. The base constructor still records the (fabricated)
    ///     <see cref="ApiConnection.AuthDetails"/> that a few call sites read.
    /// </summary>
    public class OfflineApiConnection : ApiConnection
    {
        public OfflineApiConnection(AuthDetails authDetails) : base(authDetails) { }

        public override Task<bool> RefreshToken() => Task.FromResult(true);
    }
}
