namespace WindowsFormsApp.Contracts
{
    public interface Mouse
    {
        void Click(int x, int y);
        void DoubleClick(int x, int y);
        void MoveCursor(int x, int y);
    }
}
