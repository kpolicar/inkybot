using System.Drawing;
using System.Windows.Forms;
using Inkybot.Contracts;
using Inkybot.Domain;
using Inkybot.Helpers;
using Inkybot.Services;

namespace Inkybot.Actions
{
    public abstract class MouseAction : IAction
    {
        protected Input Input;
        protected ScreenReaderDataProvider screenDataProvider;
        protected Control targetControl;

        public MouseAction(Control targetControl) {
            Input = (Input) Program.Services.GetService(typeof(Input));
            screenDataProvider = (ScreenReaderDataProvider) Program.Services.GetService(typeof(DofusDataProvider));
            this.targetControl = targetControl;
        }

        public abstract void Execute();
        
        protected Point GetCursorTarget(Responsive.Measurement measurement) {
           var target = Responsive.ResponsiveRectangle(measurement, targetControl.Width, targetControl.Height);
           
           return new Point(target.X, target.Y);
        }
    }
}
