using System.Drawing;
using System.Windows.Forms;
using WindowsComboBox = System.Windows.Forms.ComboBox;

namespace Inkybot.Controls
{
    public class ComboBox : WindowsComboBox
    {
        private const int WM_PAINT = 0xF;
        private int buttonWidth = SystemInformation.HorizontalScrollBarArrowWidth;
        Color borderColor = Color.Black;
        
        public Color BorderColor
        {
            get { return borderColor; }
            set { borderColor = value; Invalidate(); }
        }
        
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == WM_PAINT && DropDownStyle != ComboBoxStyle.Simple)
            {
                using (var g = Graphics.FromHwnd(Handle))
                {
                    using (var p = new Pen(BorderColor))
                    {
                        g.DrawRectangle(p, 0, 0, Width - 1, Height - 1);

                        var d = FlatStyle == FlatStyle.Popup ? 1 : 0;
                        g.DrawLine(p, Width - buttonWidth - d,
                            0, Width - buttonWidth - d, Height);
                    }
                }
            }
        }
    }
}
