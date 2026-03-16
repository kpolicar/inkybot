using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Inkybot.Api.Resources;
using Inkybot.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Inkybot.Domain
{
    public static partial class DetectUserGame
    {
        private static string BasePath =>
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        private static T? ReadJsonFile<T>(string path, string fileName) where T : class {
            const int maxAttempts = 3;
            for (int attempt = 1; attempt <= maxAttempts; attempt++) {
                try {
                    string content;
                    using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (var reader = new StreamReader(fs, Encoding.UTF8, detectEncodingFromByteOrderMarks: true))
                        content = reader.ReadToEnd();

                    if (string.IsNullOrWhiteSpace(content)) {
                        FileEventLogger.SystemLogger.Warn($"{fileName} is empty (attempt {attempt}/{maxAttempts}) at: {path}");
                        if (attempt < maxAttempts) { Thread.Sleep(100); continue; }
                        return null;
                    }

                    var result = JsonConvert.DeserializeObject<T>(content);
                    FileEventLogger.SystemLogger.Info($"{fileName} read successfully: {content}");
                    return result;
                } catch (FileNotFoundException) {
                    FileEventLogger.SystemLogger.Warn($"{fileName} not found at: {path}");
                    return null;
                } catch (Exception ex) {
                    FileEventLogger.SystemLogger.Warn($"{fileName} failed to read (attempt {attempt}/{maxAttempts}): {ex.Message}");
                    if (attempt < maxAttempts) { Thread.Sleep(100); continue; }
                }
            }

            FileEventLogger.SystemLogger.Warn($"{fileName} failed after all attempts.");
            return null;
        }

        private static T ReadJsonFile<T>(string path, string fileName, T fallback) where T : struct {
            const int maxAttempts = 3;
            for (int attempt = 1; attempt <= maxAttempts; attempt++) {
                try {
                    string content;
                    using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (var reader = new StreamReader(fs, Encoding.UTF8, detectEncodingFromByteOrderMarks: true))
                        content = reader.ReadToEnd();

                    if (string.IsNullOrWhiteSpace(content)) {
                        FileEventLogger.SystemLogger.Warn($"{fileName} is empty (attempt {attempt}/{maxAttempts}) at: {path}");
                        if (attempt < maxAttempts) { Thread.Sleep(100); continue; }
                        return fallback;
                    }

                    var result = JsonConvert.DeserializeObject<T>(content);
                    FileEventLogger.SystemLogger.Info($"{fileName} read successfully: {content}");
                    return result;
                } catch (FileNotFoundException) {
                    FileEventLogger.SystemLogger.Warn($"{fileName} not found at: {path}");
                    return fallback;
                } catch (Exception ex) {
                    FileEventLogger.SystemLogger.Warn($"{fileName} failed to read (attempt {attempt}/{maxAttempts}): {ex.Message}");
                    if (attempt < maxAttempts) { Thread.Sleep(100); continue; }
                }
            }

            FileEventLogger.SystemLogger.Warn($"{fileName} failed after all attempts, using defaults.");
            return fallback;
        }

        public static Settings? ReadSettings() {
            var path = BasePath + @"\zaap\repositories\production\dofus\dofus3\settings.json";
            return ReadJsonFile<Settings>(path, "settings.json");
        }

        public static Release? ReadRelease() {
            var path = BasePath + @"\zaap\repositories\production\dofus\dofus3\release.json";
            return ReadJsonFile<Release>(path, "release.json");
        }

        private static readonly string[] DofusJsonChannels = { "RELEASE", "INTERNAL" };

        /// <summary>Returns all existing dofus.json paths (RELEASE, INTERNAL, etc.).</summary>
        public static string[] GetAllDofusJsonPaths() {
            var localLow = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                @"AppData\LocalLow");
            return DofusJsonChannels
                .Select(ch => Path.Combine(localLow, "Ankama", "Dofus", ch, "Shared", "dofus.json"))
                .Where(File.Exists)
                .ToArray();
        }

        /// <summary>
        ///     Reads all valid dofus.json files across channels (RELEASE, INTERNAL, etc.).
        ///     Skips files that are empty or can't be deserialized.
        /// </summary>
        public static (string path, DofusPreferences prefs)[] ReadAllDofusPreferences() {
            var results = new System.Collections.Generic.List<(string, DofusPreferences)>();
            foreach (var path in GetAllDofusJsonPaths()) {
                try {
                    string content;
                    using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (var reader = new StreamReader(fs, Encoding.UTF8, detectEncodingFromByteOrderMarks: true))
                        content = reader.ReadToEnd();

                    if (string.IsNullOrWhiteSpace(content)) continue;

                    var result = JsonConvert.DeserializeObject<DofusPreferences>(content);
                    FileEventLogger.SystemLogger.Info($"dofus.json read successfully from: {path}");
                    results.Add((path, result));
                } catch {
                    FileEventLogger.SystemLogger.Warn($"dofus.json could not be read from: {path}");
                }
            }
            return results.ToArray();
        }

        public static bool HasValidAndSupportedLanguage(Settings settings) {
            return new[] {"en", "fr"}.Contains(settings.language.value);
        }

        public static bool HasValidGamePath(Release release) {
            return release.location != "" && File.Exists(release.ExeLocation);
        }
    }
}
