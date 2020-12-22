using System.Security.Principal;
using System.Threading;

namespace Inkybot.Helpers
{
    public static class Numbers
    {
        public static int? Parse(string number) {
            if (number == "-")
                return null;
            return int.Parse(number);
        }
    }
}
