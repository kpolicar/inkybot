using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Rect = System.Drawing.Rectangle;

namespace Inkybot.Controls
{
    public class EnqueueRectangle : Rectangle
    {
        private Button button;
        public string Tooltip = "Show options";
        public ToolStripMenuItem AddToQueueMenuItem { get; }
        public ToolStripMenuItem EditConfigMenuItem { get; }
        public ToolStripMenuItem RemoveFromQueueMenuItem { get; }
        
        
        public EnqueueRectangle() {
            ParentChanged += Rectangle_OnParentChanged;
            AddToQueueMenuItem = new ToolStripMenuItem("Add to queue");
            EditConfigMenuItem = new ToolStripMenuItem("Edit config") {
                Visible = false
            };
            RemoveFromQueueMenuItem = new ToolStripMenuItem("Remove from queue") {
                Visible = false
            };
        }

        public void NewOnLocationChanged(EventArgs e) {
            OnLocationChanged(e);
        }

        protected void Rectangle_OnParentChanged(object sender, EventArgs e) {
            var menu = new ContextMenuStrip() {
                Items = {
                    AddToQueueMenuItem, EditConfigMenuItem, RemoveFromQueueMenuItem
                },
                AutoSize = true,
                ShowCheckMargin = false,
                ShowImageMargin = false,
                ShowItemToolTips = false,
            };
            button = new MenuButton() {
                Menu = menu,
                Visible = Visible
            };
            VisibleChanged += (_, _) => button!.Visible = Visible;
            button.FlatStyle = FlatStyle.Flat;
            button.Location = Location;
            button.Font = new Font("Calibri", 9.75f, FontStyle.Bold);
            button.BackColor = BackColor;
            button.ForeColor = ForeColor;
            button.Size = new Size(16, 16);
            button.Padding = Padding.Empty;
            var drawBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Black);
            button.Paint += (_, args) => {
                args.Graphics.DrawString("+", button.Font, drawBrush, 2, 0);
            };
            LocationChanged += (_, _) => {
                button.Location = Location + new Size(Width - button.Width, Height - button.Height) - new Size(BorderWidth, BorderWidth);
            };
            var tooltip = new System.Windows.Forms.ToolTip();
            tooltip.ShowAlways = true;
            tooltip.SetToolTip(button, Tooltip);
            Parent.Controls.Add(button);
            button.BringToFront();
        }
    }
}

