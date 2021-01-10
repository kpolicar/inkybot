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
                return Stat.Stats.Values.First(stat => stat.DisplayName == name);
            } catch (Exception exception) {
                Debug.WriteLine(name);
                throw new CouldNotResolveStatNameException($"Error occured trying to resolve stat name \"{name}\"", exception);
            }
        }
    }
}
