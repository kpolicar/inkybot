using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Rect = System.Drawing.Rectangle;

namespace Inkybot.Controls
{
    public partial class Rectangle : UserControl
    {
        public int BorderWidth = 2;
        
        
        public Rectangle()
        {
            InitializeComponent();
            Paint += Rectangle_Paint;
            ResizeRedraw = true;
            Rectangle_Paint(this, EventArgs.Empty);
        }

        private void Rectangle_Paint(object sender, EventArgs e)
        {
            var path = new GraphicsPath();
            // add the main rectangle:
            path.AddRectangle(new Rect(new Point(0, 0), this.Size));
            // punch some holes in our main rectangle
            // this will make a standard "windowpane" with four panes
            // and a border width of ten pixels
            var sz = new Size(Width-BorderWidth*2, Height-BorderWidth*2);
            path.FillMode = FillMode.Alternate;
            path.AddRectangle(new Rect(new Point(BorderWidth, BorderWidth), sz));
            // build a region from our path and set the forms region to that:
            Region = new Region(path);
        }
    }
}

