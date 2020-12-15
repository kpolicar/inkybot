using System.Windows.Forms;
using Inkybot.Actions;
using Inkybot.Contracts;
using Inkybot.Domain;

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

        public IAction CombineRune(Rune rune, bool exo) {
            return new CombineRune(targetControl, rune, exo);
        }
    }
}
