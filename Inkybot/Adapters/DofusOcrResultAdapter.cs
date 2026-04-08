using System;
using System.Diagnostics;
using System.Linq;
using Inkybot.Dofus;
using Inkybot.Domain;
using Inkybot.Exceptions;

namespace Inkybot.Adapters
{
    public abstract class DofusOcrResultAdapter : OcrResultAdapter
    {
        protected static string SpellCorrectStatName(string name) {
            
            var spellCorrected = spellCorrect.Lookup(name, SymSpell.Verbosity.Closest).First().term;

            if (spellCorrected != name)
                Debug.WriteLine($"OCR error, original: {name}, fixed: {spellCorrected}");
            return spellCorrected;
        }
        
        protected Stat GetStatFromName(string name) {
            try {
                name = SpellCorrectStatName(name);
                var gameDict = GameLanguageDetector.Current.StatDictionary;
                return Stat.Stats.Values.First(stat =>
                    stat.Mageable
                        ? gameDict.GetString(stat.Identifier) == name
                        : stat.Identifier == name);
            } catch (Exception exception) {
                throw new CouldNotResolveStatNameException($"Error occured trying to resolve stat name \"{name}\"", exception);
            }
        }
    }
}
