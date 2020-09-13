using WindowsFormsApp.Actions;
using WindowsFormsApp.Contracts;

namespace WindowsFormsApp.Services
{
    public class MouseActionFactory : ActionFactory
    {
        public IAction Finish() {
            return new Finish();
        }
        
        public IAction Combine(Item.ItemStat target) {
            return new Combine(target);
        }

        public IAction SelectRune(Rune rune) {
            return new SelectRune(rune);
        }
    }
}