using Inkybot.Contracts;

namespace Inkybot.Actions
{
    public abstract class MouseAction : IAction
    {
        protected Mouse mouse;
        protected ScreenReaderDataProvider screenDataProvider;

        public MouseAction() {
            mouse = (Mouse) Program.Services.GetService(typeof(Mouse));
            screenDataProvider = (ScreenReaderDataProvider) Program.Services.GetService(typeof(DofusDataProvider));
        }

        public abstract void Execute();
    }
}
