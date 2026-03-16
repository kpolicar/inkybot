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

        public static Settings? ReadSettings() {
            var path = BasePath + @"\zaap\repositories\production\dofus\dofus3\settings.json";
            try {
                var content = File.ReadAllText(path);
                var result = JsonConvert.DeserializeObject<Settings>(content);
                FileEventLogger.SystemLogger.Info($"settings.json read successfully: {content}");
                return result;
            } catch (FileNotFoundException) {
                FileEventLogger.SystemLogger.Warn($"settings.json not found at: {path}");
                return null;
            } catch (Exception ex) {
                FileEventLogger.SystemLogger.Warn($"settings.json failed to read: {ex.Message}");
                return null;
            }
        }

        public static Release? ReadRelease() {
            var path = BasePath + @"\zaap\repositories\production\dofus\dofus3\release.json";
            try {
                var content = File.ReadAllText(path);
                var result = JsonConvert.DeserializeObject<Release>(content);
                FileEventLogger.SystemLogger.Info($"release.json read successfully: {content}");
                return result;
            } catch (FileNotFoundException) {
                FileEventLogger.SystemLogger.Warn($"release.json not found at: {path}");
                return null;
            } catch (Exception ex) {
                FileEventLogger.SystemLogger.Warn($"release.json failed to read: {ex.Message}");
                return null;
            }
        }

        public static DofusPreferences ReadDofusPreferences() {
            var localLow = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                @"AppData\LocalLow");
            var path = localLow + @"\Ankama\Dofus\RELEASE\Shared\dofus.json";

            const int maxAttempts = 3;
            for (int attempt = 1; attempt <= maxAttempts; attempt++) {
                try {
                    string content;
                    using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (var reader = new StreamReader(fs, Encoding.UTF8, detectEncodingFromByteOrderMarks: true))
                        content = reader.ReadToEnd();

                    if (string.IsNullOrWhiteSpace(content)) {
                        FileEventLogger.SystemLogger.Warn($"dofus.json is empty (attempt {attempt}/{maxAttempts}) at: {path}");
                        if (attempt < maxAttempts) { Thread.Sleep(100); continue; }
                        return DofusPreferences.Ideal;
                    }

                    var result = JsonConvert.DeserializeObject<DofusPreferences>(content);
                    FileEventLogger.SystemLogger.Info($"dofus.json read successfully: {content}");
                    return result;
                } catch (FileNotFoundException) {
                    FileEventLogger.SystemLogger.Warn($"dofus.json not found at: {path}");
                    return DofusPreferences.Ideal;
                } catch (Exception ex) {
                    FileEventLogger.SystemLogger.Warn($"dofus.json failed to read (attempt {attempt}/{maxAttempts}): {ex.Message}");
                    if (attempt < maxAttempts) { Thread.Sleep(100); continue; }
                }
            }

            FileEventLogger.SystemLogger.Warn("dofus.json failed after all attempts, using defaults.");
            return DofusPreferences.Ideal;
        }

        public static bool HasValidAndSupportedLanguage(Settings settings) {
            return new[] {"en", "fr"}.Contains(settings.language.value);
        }

        public static bool HasValidGamePath(Release release) {
            return release.location != "" && File.Exists(release.ExeLocation);
        }
    }
}
