using WindowsFormsApp.Contracts;

namespace WindowsFormsApp.Services
{
    public class MouseCommandIssuer : DofusCommandIssuer
    {
        private Mouse mouse;

        public MouseCommandIssuer() {
            mouse = (Mouse) Program.Services.GetService(typeof(Mouse));
        }
        
        public void SelectRune(int row, int column) {
            mouse.DoubleClick(1065 + column*55, 320 + row*39);
        }

        public void Combine() {
            mouse.Click(1065, 320-39);
        }
    }
}