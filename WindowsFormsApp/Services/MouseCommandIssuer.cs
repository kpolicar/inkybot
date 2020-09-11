namespace WindowsFormsApp.Services
{
    public class MouseCommandIssuer : DofusCommandIssuer
    {
        private Mouse mouse;

        public MouseCommandIssuer() {
            mouse = (Mouse) Program.Services.GetService(typeof(Mouse));
        }
        
        public void SelectRune(int row, int column) {
            mouse.DoubleClick(row, column);
        }
    }
}