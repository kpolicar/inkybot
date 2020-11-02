
namespace Inkybot.Adapters
{
    public abstract class OcrResultAdapter
    {
        private static bool init;
        protected static SymSpell spellCorrect;

        public OcrResultAdapter() {
            if (init) return;

            spellCorrect = new SymSpell();
            
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var dictionaryFile = assembly.GetManifestResourceStream("Inkybot.dictionary_dofus.txt");
            
            spellCorrect.LoadDictionary(dictionaryFile, 0, 1);
            init = true;
        }
    }
}
