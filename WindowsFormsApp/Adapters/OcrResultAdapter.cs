using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace WindowsFormsApp.Adapters
{
    abstract public class OcrResultAdapter
    {
        private static bool init = false;
        protected static SymSpell spellCorrect;

        public OcrResultAdapter()
        {
            if (init) return;
            
            spellCorrect = new SymSpell();
            spellCorrect.LoadDictionary(@"A:\Projects\RiderProjects\bot\WindowsFormsApp\frequency_dictionary_en_82_765.txt", 0, 1);
            init = true;
        }
    }
}