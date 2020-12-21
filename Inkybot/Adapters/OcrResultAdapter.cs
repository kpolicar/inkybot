using Inkybot.Domain;

namespace Inkybot.Adapters
{
    public abstract class OcrResultAdapter
    {
        private static bool init;
        protected static SymSpell spellCorrect;

        public OcrResultAdapter() {
            if (init) return;

            spellCorrect = new SymSpell(16, 3);
            Dictionary.LoadInto(spellCorrect);
            
            init = true;
        }
    }
}
