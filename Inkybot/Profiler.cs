using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Inkybot
{
    public static class Profiler
    {
        private static readonly List<(string Section, string Key, long Ms)> _entries = new();
        private static readonly object _lock = new();

        public static void Reset() {
            lock (_lock) { _entries.Clear(); }
        }

        public static void Record(string section, string key, long ms) {
            lock (_lock) { _entries.Add((section, key, ms)); }
        }

        public static void PrintSummary() {
            List<(string Section, string Key, long Ms)> snapshot;
            lock (_lock) { snapshot = new List<(string, string, long)>(_entries); }

            var sb = new StringBuilder();
            sb.AppendLine("[Profiler] Tick summary:");

            foreach (var sectionGroup in snapshot.GroupBy(e => e.Section)) {
                sb.AppendLine($"  [{sectionGroup.Key}]");
                foreach (var keyGroup in sectionGroup.GroupBy(e => e.Key)) {
                    var count = keyGroup.Count();
                    var total = keyGroup.Sum(e => e.Ms);
                    var countStr = count > 1 ? $" x{count}" : "";
                    sb.AppendLine($"    {keyGroup.Key}: {total}ms{countStr}");
                }
            }

            Debug.Write(sb.ToString());
        }
    }
}
