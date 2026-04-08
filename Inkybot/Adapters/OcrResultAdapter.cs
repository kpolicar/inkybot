using Inkybot.Domain;

namespace Inkybot.Adapters
{
    public abstract class OcrResultAdapter
    {
        protected static SymSpell spellCorrect => GameLanguageDetector.Current.SpellCorrect;
    }
}
