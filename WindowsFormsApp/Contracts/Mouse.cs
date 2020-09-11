namespace WindowsFormsApp.Services
{
    public interface Mouse
    {
        void DoubleClick(int x, int y);
        void MoveCursor(int x, int y);
    }
}