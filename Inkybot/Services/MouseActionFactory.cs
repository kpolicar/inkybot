using System.Windows.Forms;
using Inkybot.Actions;
using Inkybot.Contracts;

namespace Inkybot.Services
{
    public class MouseActionFactory : ActionFactory
    {
        private Control targetControl;

        public void setRelativeToControl(Control targetControl) {
            this.targetControl = targetControl;
        }
        
        public IAction Finish() {
            return new Finish(targetControl);
        }

        public IAction Combine(Rune target) {
            return new Combine(targetControl, target);
        }

        public IAction SelectRune(Rune rune) {
            return new SelectRune(targetControl, rune);
        }
    }
}
