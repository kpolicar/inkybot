using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Inkybot.Dofus;

namespace Inkybot.Domain
{
    public static class GameLanguageDetector
    {
        public static GameLanguageContext EnglishContext { get; set; } = null!;
        public static GameLanguageContext FrenchContext { get; set; } = null!;
        public static GameLanguageContext Current { get; set; } = null!;
        public static bool IsLocked { get; private set; }

        public static void Initialize(CultureInfo initialGuess) {
            EnglishContext = new GameLanguageContext(new CultureInfo("en"));
            FrenchContext = new GameLanguageContext(new CultureInfo("fr"));
            Current = initialGuess.TwoLetterISOLanguageName == "fr" ? FrenchContext : EnglishContext;
        }

        public static bool DetectAndLock(string[] ocrStatNames) {
            if (IsLocked) return false;

            var enScore = ScoreLanguage(EnglishContext, ocrStatNames);
            var frScore = ScoreLanguage(FrenchContext, ocrStatNames);

            Debug.WriteLine($"Language detection - EN score: {enScore}, FR score: {frScore}");

            var detected = enScore <= frScore ? EnglishContext : FrenchContext;
            var changed = detected != Current;

            Current = detected;
            IsLocked = true;

            Debug.WriteLine($"Game language locked to: {Current.Culture.TwoLetterISOLanguageName}" +
                            (changed ? " (changed from initial guess)" : ""));

            return changed;
        }

        private static int ScoreLanguage(GameLanguageContext context, string[] statNames) {
            var totalDistance = 0;
            foreach (var name in statNames) {
                if (string.IsNullOrWhiteSpace(name)) continue;
                var suggestions = context.SpellCorrect.Lookup(name.Trim(), SymSpell.Verbosity.Closest);
                if (suggestions.Count > 0) {
                    totalDistance += suggestions[0].distance;
                } else {
                    totalDistance += 10;
                }
            }
            return totalDistance;
        }
    }
}
