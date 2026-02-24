#if DEBUG
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Inkybot.Debugging
{
    public class ProfilerFileLogger
    {
        private readonly string _filePath;

        public ProfilerFileLogger() {
            var dir = Path.Combine(AppContext.BaseDirectory, "logs");
            Directory.CreateDirectory(dir);
            _filePath = Path.Combine(dir, "profiler.jsonl");
        }

        public void Bind() {
            Profiler.SummaryReady += OnSummaryReady;
        }

        private void OnSummaryReady(IReadOnlyList<(string Section, string Key, long Ms)> entries) {
            var dict = new Dictionary<string, long>();
            foreach (var (section, key, ms) in entries) {
                var k = $"{section}.{key}";
                dict.TryGetValue(k, out var existing);
                dict[k] = existing + ms;
            }

            var record = new Dictionary<string, object> {
                ["ts"] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };
            foreach (var kv in dict)
                record[kv.Key] = kv.Value;

            var line = JsonConvert.SerializeObject(record);
            File.AppendAllText(_filePath, line + Environment.NewLine);
        }
    }
}
#endif
