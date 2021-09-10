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
        public event ControlEventHandler? AddToQueue;

        public EnqueueRectangle()
        {
            ParentChanged += Rectangle_OnParentChanged;
            VisibleChanged += (_, _) => button!.Visible = Visible;
        }

        protected void Rectangle_OnParentChanged(object sender, EventArgs e) {
            var toolstripItem = new ToolStripMenuItem("Add to queue");
            toolstripItem.Click += (_, _) => {
                BeginInvoke(new MethodInvoker(delegate {
                    ForeColor = BackColor = Color.ForestGreen;
                    BorderWidth = 4;
                    OnLocationChanged(EventArgs.Empty);
                    BringToFront();
                }));
                AddToQueue?.Invoke(this, new ControlEventArgs(this));
            };
            var menu = new ContextMenuStrip() {
                Items = {toolstripItem},
                AutoSize = true,
                ShowCheckMargin = false,
                ShowImageMargin = false,
                ShowItemToolTips = false,
            };
            button = new MenuButton() {
                Menu = menu,
            };
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
            tooltip.SetToolTip(button, "Show options");
            Parent.Controls.Add(button);
            button.BringToFront();
        }
    }
}

