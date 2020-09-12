namespace WindowsFormsApp.Contracts
{
    public interface Mouse
    {
        void DoubleClick(int x, int y);
        void MoveCursor(int x, int y);
    }
}