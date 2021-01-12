using System;
using System.IO;
using System.Linq;
using Inkybot.Api.Resources;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Inkybot.Domain
{
    public static partial class DetectUserGame
    {
        private static string BasePath => 
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        
        public static Settings? ReadSettings() {
            try {
                return JsonConvert
                    .DeserializeObject<Settings>(
                        File.ReadAllText(BasePath + @"\zaap\repositories\production\dofus\main\settings.json"));
            } catch (Exception) {
                return null;
            }
        }
        
        public static Release? ReadRelease() {
            try {
                return JsonConvert
                    .DeserializeObject<Release>(
                        File.ReadAllText(BasePath + @"\zaap\repositories\production\dofus\main\release.json"));
            } catch (Exception) {
                return null;
            }
        }

        public static bool HasValidAndSupportedLanguage(Settings settings) {
            return new[] {"en", "fr"}.Contains(settings.language.value);
        }

        public static bool HasValidGamePath(Release release) {
            return release.location != "" && File.Exists(release.ExeLocation);
        }
    }
}
