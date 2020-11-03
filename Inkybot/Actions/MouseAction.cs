using System.Drawing;
using System.Windows.Forms;
using Inkybot.Contracts;
using Inkybot.Helpers;

namespace Inkybot.Actions
{
    public abstract class MouseAction : IAction
    {
        protected Mouse mouse;
        protected ScreenReaderDataProvider screenDataProvider;
        protected Control targetControl;

        public MouseAction(Control targetControl) {
            mouse = (Mouse) Program.Services.GetService(typeof(Mouse));
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
