using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Rect = System.Drawing.Rectangle;

namespace Inkybot.Controls
{
    public partial class Crosshair : UserControl
    {
        
        public Crosshair()
        {
            InitializeComponent();
            Paint += Rectangle_Paint;
            ResizeRedraw = true;
            Rectangle_Paint(this, EventArgs.Empty);
        }

        private void Rectangle_Paint(object sender, EventArgs e)
        {
            var path = new GraphicsPath();
            path.AddRectangle(new Rect(
                new Point(Width/2-2, 0), new Size(4, Height)));
            path.AddRectangle(new Rect(
                new Point(0, Height/2-2), new Size(Width, 4)));
            Region = new Region(path);
        }
        
        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified) {
            x -= width/2;
            y -= height/2;
            base.SetBoundsCore(x, y, width, height, specified);
        }
    }
}

