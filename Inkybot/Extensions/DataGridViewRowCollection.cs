using System;
using System.Linq;
using System.Windows.Forms;
using SystemDataGridViewRowCollection=System.Windows.Forms.DataGridViewRowCollection;
using Inkybot.Dofus;

namespace Inkybot.Extensions
{
    public static class DataGridViewRowCollection
    {

        public static DataGridViewRow? FindWithTag<TTagType>(
            this SystemDataGridViewRowCollection rows,
            Func<TTagType, bool> predicate) {
            
            return rows
                .Cast<DataGridViewRow>()
                .FirstOrDefault(row => predicate((TTagType) row.Tag));
        }
    }
}
