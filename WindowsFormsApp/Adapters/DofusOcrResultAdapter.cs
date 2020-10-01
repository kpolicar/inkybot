using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace WindowsFormsApp.Adapters
{
    abstract public class DofusOcrResultAdapter : OcrResultAdapter
    {
        protected static string SpellCorrectStatName(string name)
        {
            var terms = Regex.Split(name, " ")
                .Select(term => Regex.IsMatch(term, @"[\-\+\%]") ?
                    term :
                    spellCorrect.Lookup(term, SymSpell.Verbosity.Top).First().term)
                .ToArray();

            if (string.Join(" ", terms) != name)
            {
                Debug.WriteLine("OCR error, original:"+name+", fixed:"+string.Join(" ", terms));
            }
            return string.Join(" ", terms);
        }
    }
}