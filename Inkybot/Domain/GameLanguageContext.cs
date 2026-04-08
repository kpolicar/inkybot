using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Resources;
using Inkybot.Dofus;
using Inkybot.Helpers;
using Inkybot.Services;

namespace Inkybot.Domain
{
    public class GameLanguageContext
    {
        public CultureInfo Culture { get; }
        public ResourceSet StatDictionary { get; }
        public ResourceSet RuneDictionary { get; }
        public SymSpell SpellCorrect { get; }

        public string FailurePattern { get; }
        public string SinkHasChangedPattern { get; }
        public string HistoryEntryPattern { get; }
        public string HistorySplitPattern { get; }
        public string ItemStatLinePattern { get; }
        public string ItemWeaponEffectStatLinePattern { get; }

        public Responsive.Measurement SinkMeasurement { get; }
        public string TesseractLanguage { get; }

        public GameLanguageContext(CultureInfo culture) {
            Culture = culture;

            var assembly = Assembly.GetExecutingAssembly();

            StatDictionary = new ResourceManager("Inkybot.Resources.StatDictionary", assembly)
                .GetResourceSet(culture, true, true);
            RuneDictionary = new ResourceManager("Inkybot.Resources.RuneDictionary", assembly)
                .GetResourceSet(culture, true, true);

            SpellCorrect = new SymSpell(16, 5);
            LoadDictionaryInto(SpellCorrect, new ResourceManager("Inkybot.Resources.StatDictionary", assembly), culture);
            LoadDictionaryInto(SpellCorrect, new ResourceManager("Inkybot.Resources.MagingDictionary", assembly), culture);

            var regexRm = new ResourceManager("Inkybot.Properties.Regex", assembly);
            FailurePattern = regexRm.GetString("FailurePattern", culture);
            SinkHasChangedPattern = regexRm.GetString("SinkHasChangedPattern", culture);
            HistoryEntryPattern = regexRm.GetString("HistoryEntryPattern", culture);
            HistorySplitPattern = regexRm.GetString("HistorySplitPattern", culture);
            ItemStatLinePattern = regexRm.GetString("ItemStatLinePattern", culture);
            ItemWeaponEffectStatLinePattern = regexRm.GetString("ItemWeaponEffectStatLinePattern", culture);

            TesseractLanguage = culture.ThreeLetterISOLanguageName;

            SinkMeasurement = culture.TwoLetterISOLanguageName == "fr"
                ? Measurements.SinkFrMeasurement
                : Measurements.SinkMeasurement;
        }

        public string GetRuneDisplayName(Rune rune) {
            var runeName = RuneDictionary.GetString(rune.Stat.Identifier)!;
            var format = RuneDictionary.GetString("format")!;

            return rune.Type switch {
                Rune.RuneType.Ra =>
                    format.Replace(":name", runeName)
                        .Replace(":strength", "RA"),
                Rune.RuneType.Pa =>
                    format.Replace(":name", runeName)
                        .Replace(":strength", "PA"),
                Rune.RuneType.Sm =>
                    format.Replace(":name", runeName)
                        .Replace(":strength ", ""),
            };
        }

        private static void LoadDictionaryInto(SymSpell spellCorrect, ResourceManager rm, CultureInfo culture) {
            var resourceSet = rm.GetResourceSet(culture, true, true);
            foreach (DictionaryEntry entry in resourceSet) {
                var stat = entry.Value.ToString();
                spellCorrect.CreateDictionaryEntry(stat, 1);
            }
        }
    }
}
