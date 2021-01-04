using System.Threading;

namespace Inkybot.Contracts
{
    public interface Input
    {
        void Drag(int x, int y, int tX, int tY);
        void Click(int x, int y);
        void DoubleClick(int x, int y);
        void CtrlDoubleClick(int x, int y);
        void SelectAll();
        void TypeMessage(string message, CancellationToken? cancel=null);
    }
}
