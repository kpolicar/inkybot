using System.Drawing;
using System.Windows.Forms;

namespace Inkybot.Controls
{
    public class TransparentPanel : Panel
    {
        protected override CreateParams CreateParams {
            get {
                var cp = base.CreateParams;
                cp.ExStyle |= 0x00000020; // WS_EX_TRANSPARENT
                cp.ExStyle |= 0x00000008; // WS_EX_TOPMOST

                return cp;
            }
        }

        protected override void OnPaint(PaintEventArgs e) {
            e.Graphics.FillRectangle(new SolidBrush(BackColor), ClientRectangle);
            base.OnPaint(e);
        }
    }
}
