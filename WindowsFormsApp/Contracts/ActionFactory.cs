namespace WindowsFormsApp.Contracts
{
    public interface ActionFactory
    {
        IAction Finish();
        IAction Combine(Item.ItemStat target);
        IAction SelectRune(Rune rune);
    }
}