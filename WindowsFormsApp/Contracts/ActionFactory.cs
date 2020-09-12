namespace WindowsFormsApp.Contracts
{
    public interface ActionFactory
    {
        IAction Combine(Item.ItemStat target);
        IAction SelectRune(Rune rune);
    }
}