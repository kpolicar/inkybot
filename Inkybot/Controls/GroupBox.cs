using System.Drawing;
using System.Windows.Forms;
using WindowsGroupBox = System.Windows.Forms.GroupBox;
using SystemRectangle = System.Drawing.Rectangle;

namespace Inkybot.Controls
{
    public class GroupBox : WindowsGroupBox
    {
        public Color BorderColor;
        
        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);
            DrawGroupBox(e.Graphics, ForeColor, BorderColor);
        }
        
        private void DrawGroupBox(Graphics g, Color textColor, Color borderColor) {
            Brush textBrush = new SolidBrush(textColor);
            Brush borderBrush = new SolidBrush(borderColor);
            Pen borderPen = new Pen(borderBrush);
            SizeF strSize = g.MeasureString(Text, Font);
            SystemRectangle rect = new SystemRectangle(ClientRectangle.X,
                ClientRectangle.Y + (int) (strSize.Height / 2),
                ClientRectangle.Width - 1,
                ClientRectangle.Height - (int) (strSize.Height / 2) - 1);

            // Clear text and border
            g.Clear(this.BackColor);

            // Draw text
            g.DrawString(Text, Font, textBrush, Padding.Left, 0);

            // Drawing Border
            //Left
            g.DrawLine(borderPen, rect.Location, new Point(rect.X, rect.Y + rect.Height));
            //Right
            g.DrawLine(borderPen, new Point(rect.X + rect.Width, rect.Y),
                new Point(rect.X + rect.Width, rect.Y + rect.Height));
            //Bottom
            g.DrawLine(borderPen, new Point(rect.X, rect.Y + rect.Height),
                new Point(rect.X + rect.Width, rect.Y + rect.Height));
            //Top1
            g.DrawLine(borderPen, new Point(rect.X, rect.Y), new Point(rect.X + Padding.Left, rect.Y));
            //Top2
            g.DrawLine(borderPen, new Point(rect.X + Padding.Left + (int) (strSize.Width), rect.Y),
                new Point(rect.X + rect.Width, rect.Y));
        }

    }
}
