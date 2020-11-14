using System;
using System.ComponentModel;
using DataGridViewElement = System.Windows.Forms.DataGridView;
using System.ComponentModel;
using System.Windows.Forms;

namespace Inkybot.Helpers
{
    public static class DataGridView
    {
        public static void OnValidatingDataGridViewCellNumeric(object sender, DataGridViewCellValidatingEventArgs e) {
            if (e.ColumnIndex == 0) return;
            
            var isNumber = int.TryParse(e.FormattedValue.ToString(), out _)
                           || e.FormattedValue.ToString() == "-";
            e.Cancel = !isNumber;
        }
    }
}
