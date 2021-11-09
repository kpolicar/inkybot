using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using Inkybot.Events;

namespace Inkybot
{
    public partial class WaitingForDofusForm : Form
    {
        public event EventHandler<ProcessEventArgs>? SelectedDofusProcess;
        
        
        public WaitingForDofusForm() {
            InitializeComponent();
        }

        public void UpdateProcessList(Process[] dofusProcesses) {
            foreach (ListViewItem item in processListView.Items) {
                if (!dofusProcesses.Any(process => (item.Tag as Process)!.Id == process.Id))
                {
                    item.Remove();
                }
            }
            
            foreach (var dofusProcess in dofusProcesses) {
                var title = dofusProcess.MainWindowTitle != ""
                    ? dofusProcess.MainWindowTitle
                    : dofusProcess.ProcessName;
                
                var foundExisting = false;
                foreach (ListViewItem lItem in processListView.Items) {
                    if ((lItem.Tag as Process)!.Id == dofusProcess.Id) {
                        lItem.Text = title;
                        foundExisting = true;
                    }
                }

                if (!foundExisting) {
                    var item = new ListViewItem(title) {
                        Tag = dofusProcess
                    };
                    processListView.Items.Add(item);
                }
            }
            processListView.Visible = dofusProcesses.Length > 0;
        }

        private void OnItemActivated(object sender, EventArgs e) {
            SelectedDofusProcess?.Invoke(this, new ProcessEventArgs((processListView.FocusedItem.Tag as Process)!));
        }
    }
}
