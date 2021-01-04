using System.Windows.Forms;

namespace Inkybot
{
    public class Example
    {
        public int Add(int a, int b) {
            var result = MessageBox.Show((a+b).ToString(), "Yoyo");
            return a + b;
        }
    }
}
