using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Inkybot.Exceptions;

namespace Inkybot.Adapters
{
    public abstract class DofusOcrResultAdapter : OcrResultAdapter
    {
        protected static string SpellCorrectStatName(string name) {
            var terms = Regex.Split(name, " ")
                .Select(term =>
                    Regex.IsMatch(term, @"[\-\+\%]")
                        ? term
                        : spellCorrect.Lookup(term, SymSpell.Verbosity.Top).First().term)
                .ToArray();

            if (string.Join(" ", terms) != name)
                Debug.WriteLine("OCR error, original:" + name + ", fixed:" + string.Join(" ", terms));
            return string.Join(" ", terms);
        }
        
        protected Stat GetStatFromName(string name) {
            try {
                name = SpellCorrectStatName(name);
                return Stat.Stats.First(statData => statData.DisplayName == name);
            } catch (Exception exception) {
                throw new CouldNotResolveStatNameException("", exception);
            }
        }
    }
}
