using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Inkybot.Contracts;
using Inkybot.Dofus.Domain;
using Inkybot.Domain;
using Inkybot.Helpers;
using Inkybot.Services;

namespace Inkybot.Actions
{
    public abstract class InputAction : IAction
    {
        protected Input Input;
        protected ScreenReaderDataProvider screenDataProvider;
        protected Control targetControl;
        protected bool shouldContinueInput = true;
        protected CancellationToken? Cancel;

        public InputAction(Control targetControl) {
            Input = Program.Services.GetService<Input>();
            screenDataProvider = (ScreenReaderDataProvider) Program.Services.GetService<DofusDataProvider>();
            this.targetControl = targetControl;
        }

        internal void Execute(CancellationToken cancel) {
            Cancel = cancel;
            Execute();
        }

        public abstract void Execute();
        
        protected Point GetCursorTarget(Responsive.Measurement measurement) {
           var target = Responsive.ResponsiveRectangle(measurement, targetControl.Width, targetControl.Height);
           
           return new Point(target.X, target.Y);
        }
    }
}
