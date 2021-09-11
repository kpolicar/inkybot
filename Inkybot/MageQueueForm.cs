using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Inkybot.Dofus.Contracts;
using Inkybot.Events;
using Inkybot.Resources;
using Inkybot.Services;
using GroupBox = Inkybot.Controls.GroupBox;

namespace Inkybot
{
    public partial class MageQueueForm : Form
    {
        private Dictionary<MageQueueManager.MageQueueItem, GroupBox> mageQueueGroupBoxes
            = new Dictionary<MageQueueManager.MageQueueItem, GroupBox>();

        
        private MageQueueManager mageQueue;
        private ConfigManager configManager;


        public MageQueueForm() {
            InitializeComponent();
            configManager = (ConfigManager) Program.Services.GetService<MageConfigManager>();
            mageQueue = Program.Services.GetService<MageQueueManager>();
            mageQueue.Enqueued += OnMagingEnqueued;
            mageQueue.Dequeued += OnMagingDequeuedOrRemoved;
            mageQueue.Removed += OnMagingDequeuedOrRemoved;
            mageQueue.Moved += OnMagingMoved;
        }

        private void UpdateControlsVisibility() {
            emptyLabel.Visible = mageQueue.Empty;
            mageQueueGroupBoxesPanel.Visible = !mageQueue.Empty;
        }

        private void OnMagingMoved(object sender, MageQueueMovedEventArgs e) =>
            BeginInvoke(new MethodInvoker(() => {
                var groupBox = mageQueueGroupBoxes[e.QueueItem];
                mageQueueGroupBoxesPanel.Controls.SetChildIndex(groupBox, e.Index);
            }));

        private void OnMagingEnqueued(object sender, MageQueueEventArgs e) =>
            BeginInvoke(new MethodInvoker(() => {
                var control = BuildMageQueueGroupBox(e.QueueItem);
                SuspendLayout();
                mageQueueGroupBoxesPanel.Controls.Add(control);
                ResumeLayout();
                mageQueueGroupBoxes[e.QueueItem] = control;
                UpdateControlsVisibility();
            }));
        
        private void OnMagingDequeuedOrRemoved(object sender, MageQueueEventArgs e) {
            SuspendLayout();
            mageQueueGroupBoxesPanel.Controls.Remove(mageQueueGroupBoxes[e.QueueItem]);
            ResumeLayout();
            mageQueueGroupBoxes.Remove(e.QueueItem);
            UpdateControlsVisibility();
        }

        private void OnMageQueueItemRemove(object sender, MageQueueEventArgs e) =>
            mageQueue.Remove(e.QueueItem);

        private void OnMageQueueItemMoveUp(object sender, MageQueueEventArgs e) =>
            mageQueue.MoveForward(e.QueueItem);

        private void OnMageQueueItemMoveDown(object sender, MageQueueEventArgs e) =>
            mageQueue.MoveBack(e.QueueItem);
        
        private void MageQueueForm_Closing(object sender, CancelEventArgs cancelEventArgs) {
            cancelEventArgs.Cancel = true;
            Hide();
        }

        public async Task Highlight(MageQueueManager.MageQueueItem mageQueueItem, int delay=1500) {
            var index = mageQueue.Queue.IndexOf(mageQueueItem);
            var control = mageQueueGroupBoxesPanel.Controls[index] as GroupBox;
            if (control == null)
                return;

            Invoke(new MethodInvoker(() => {
                control.Focus();
            }));
            
            delay = 800;
            var currentColor = control.BorderColor;
            for (int i = 0; i < 3; i++) {
                Invoke(new MethodInvoker(() => {
                    control.BorderColor = Color.ForestGreen;
                    control.Invalidate();
                }));
            
                await Task.Delay(delay/12);
            
                Invoke(new MethodInvoker(() => {
                    control.BorderColor = currentColor;
                    control.Invalidate();
                }));
                
                await Task.Delay(delay/6);
            }
        }
    }
}

