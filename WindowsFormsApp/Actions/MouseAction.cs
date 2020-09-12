using WindowsFormsApp.Contracts;
using WindowsFormsApp.Services;

namespace WindowsFormsApp.Actions
{
    public abstract class MouseAction : IAction
    {
        protected MouseCommandIssuer command;
        protected ScreenReaderDataProvider screenDataProvider;

        public MouseAction() {
            command = (MouseCommandIssuer) Program.Services.GetService(typeof(DofusCommandIssuer));
            screenDataProvider = (ScreenReaderDataProvider) Program.Services.GetService(typeof(DofusDataProvider));
        }

        public abstract void Execute();
    }
}