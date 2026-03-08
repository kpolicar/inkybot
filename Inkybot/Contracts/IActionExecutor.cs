using Inkybot.Dofus.Domain;

namespace Inkybot.Contracts
{
    public interface IActionExecutor
    {
        void Execute(IAction action);
    }
}
