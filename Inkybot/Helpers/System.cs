using System.Security.Principal;
using System.Threading;

namespace Inkybot.Helpers
{
    public static class System
    {
        public static bool IsRunnningAsAdmin() {
            var domain = Thread.GetDomain();
            domain.SetPrincipalPolicy(PrincipalPolicy.WindowsPrincipal);
            var principle = (WindowsPrincipal)Thread.CurrentPrincipal;
            return principle.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}
