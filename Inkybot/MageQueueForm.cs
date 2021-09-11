using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
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


        public MageQueueForm() {
            InitializeComponent();
            mageQueue = Program.Services.GetService<MageQueueManager>();
            mageQueue.Enqueued += OnMagingEnqueued;
            mageQueue.Dequeued += OnMagingDequeuedOrRemoved;
            mageQueue.Removed += OnMagingDequeuedOrRemoved;
        }

        private void OnMagingEnqueued(object sender, MageQueueEventArgs e) =>
            BeginInvoke(new MethodInvoker(() => {
                var control = BuildMageQueueGroupBox(e.QueueItem.ItemPreview);
                SuspendLayout();
                mageQueueGroupBoxesPanel.Controls.Add(control);
                ResumeLayout();
                mageQueueGroupBoxes[e.QueueItem] = control;
            }));
        
        private void OnMagingDequeuedOrRemoved(object sender, MageQueueEventArgs e) {
            SuspendLayout();
            mageQueueGroupBoxesPanel.Controls.Remove(mageQueueGroupBoxes[e.QueueItem]);
            ResumeLayout();
            mageQueueGroupBoxes.Remove(e.QueueItem);
        }
    }
}

