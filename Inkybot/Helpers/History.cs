using System.Linq;

namespace Inkybot.Helpers
{
    public static class History
    {
        public static bool HasChanged(string[] current, string[] previous) {
            if (previous == null) return current != null && current.Length > 0;
            if (current == null) return previous.Length > 0;
            return Enumerable.ZipWithDefault(current, previous, (s, s1) => s != s1)
                .Any(b => b);
        }
    }
}
