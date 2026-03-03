using Inkybot.Actions;

namespace Inkybot.Contracts
{
    public interface IActionExecutor
    {
        void Execute(IAction action);
    }
}
