using Inkybot.Domain;

namespace Inkybot.Adapters
{
    public abstract class OcrResultAdapter
    {
        private static bool init;
        protected static readonly SymSpell spellCorrect = new SymSpell(16, 3);

        public OcrResultAdapter() {
            if (init) return;

            Dictionary.LoadInto(spellCorrect);
            init = true;
        }
    }
}
