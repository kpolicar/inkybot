namespace Inkybot.Adapters
{
    public abstract class OcrResultAdapter
    {
        private static bool init;
        protected static SymSpell spellCorrect;

        public OcrResultAdapter() {
            if (init) return;

            spellCorrect = new SymSpell();
            spellCorrect.LoadDictionary(
                @"A:\Projects\RiderProjects\bot\Inkybot\frequency_dictionary_en_82_765.txt", 0, 1);
            init = true;
        }
    }
}
