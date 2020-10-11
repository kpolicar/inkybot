namespace Inkybot.Contracts
{
    public interface ActionFactory
    {
        IAction Finish();
        IAction Combine(Rune target);
        IAction SelectRune(Rune rune);
    }
}
