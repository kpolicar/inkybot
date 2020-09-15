using System.Drawing;
using System.Windows.Forms;

namespace WindowsFormsApp
{
    public partial class IndicatorForm : Form
    {
        public IndicatorForm(Control drawRelativeTo) {
            InitializeComponent();
            
            FormBorderStyle = FormBorderStyle.None;
            Bounds = Screen.PrimaryScreen.Bounds;
            TopMost = true;

            panel1.Location = drawRelativeTo.PointToScreen(new Point(626, 300));
            panel1.Width = 980-626;
            panel1.Height = 507+300;
        }
    }
}