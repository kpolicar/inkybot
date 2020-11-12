using Inkybot.Domain;

namespace Inkybot.Contracts
{
    public interface ActionFactory
    {
        IAction Finish();
        IAction Combine(Rune target, bool exo);
        IAction SelectRune(Rune rune);
    }
}
