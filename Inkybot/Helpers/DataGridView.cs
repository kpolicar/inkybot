using DataGridViewElement = System.Windows.Forms.DataGridView;
using System.Windows.Forms;

namespace Inkybot.Helpers
{
    public static class DataGridView
    {
        public static void OnValidatingDataGridViewCellNumeric(object sender, DataGridViewCellValidatingEventArgs e) {
            var isNumber = int.TryParse(e.FormattedValue.ToString(), out _)
                           || e.FormattedValue.ToString() == "-";
            e.Cancel = !isNumber;
        }
    }
}
