using System.Globalization;
using System.Reflection;
using System.Resources;
using Inkybot.Dofus;
using Inkybot.Resources;

namespace Inkybot.Extensions
{
    public static class Presentable
    {
        public static ResourceSet? _statDictionary;
        public static ResourceSet StatDictionary =>
            _statDictionary ??=
                new ResourceManager("Inkybot.Resources.StatDictionary", Assembly.GetExecutingAssembly())
                    .GetResourceSet(CultureInfo.CurrentUICulture, true, true);
        public static ResourceSet? _runeDictionary;
        public static ResourceSet RuneDictionary =>
            _runeDictionary ??=
                new ResourceManager("Inkybot.Resources.RuneDictionary", Assembly.GetExecutingAssembly())
                    .GetResourceSet(CultureInfo.CurrentUICulture, true, true);
        
        public static string DisplayName(this Stat stat) {
            return StatDictionary.GetString(stat.Identifier)!;
        }
        
        public static string RuneName(this Stat stat) {
            return RuneDictionary.GetString(stat.Identifier)!;
        }
        
        public static string DisplayName(this Rune rune) {
            var runeName = rune.stat.DisplayName();
            var format = RuneDictionary.GetString("format")!;
            
            return rune.type switch {
                Rune.Type.Ra =>
                    format.Replace(":name", runeName)
                        .Replace(":strength", "RA"),
                Rune.Type.Pa =>
                    format.Replace(":name", runeName)
                        .Replace(":strength", "PA"),
                Rune.Type.Sm =>
                    format.Replace(":name", runeName)
                        .Replace(":strength ", ""),
            };
        }
    }
}
