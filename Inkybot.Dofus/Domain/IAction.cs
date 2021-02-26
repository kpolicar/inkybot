namespace Inkybot.Dofus.Domain
{
    /**
     * <summary>The IAction interface represents a single action the bot can execute.</summary>
     */
    public interface IAction
    {
        /**
         * <summary>Executes the action</summary>
         */
        void Execute();
    }
}
