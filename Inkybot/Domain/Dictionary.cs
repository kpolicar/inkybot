using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Resources;

namespace Inkybot
{
    public static class Dictionary
    {
        public static void LoadInto(SymSpell spellCorrect) {
            var rm1 = new ResourceManager("Inkybot.Resources.StatDictionary", Assembly.GetExecutingAssembly());
            var rm2 = new ResourceManager("Inkybot.Resources.MagingDictionary", Assembly.GetExecutingAssembly());
            LoadResourceInto(rm1, spellCorrect);
            LoadResourceInto(rm2, spellCorrect);
        }

        private static void LoadResourceInto(ResourceManager rm, SymSpell spellCorrect) {
            var resourceSet =
                rm.GetResourceSet(CultureInfo.CurrentUICulture, true, true);

            foreach (DictionaryEntry entry in resourceSet)
            {
                var stat = entry.Value.ToString();

                spellCorrect.CreateDictionaryEntry(stat, 1);
            }
        }
    }
}
