using System.Drawing;

namespace Inkybot.Contracts
{
    public interface Mouse
    {
        void Drag(int x, int y, int tX, int tY);
        void Click(int x, int y);
        void DoubleClick(int x, int y);
        void CtrlDoubleClick(int x, int y);
    }
}
