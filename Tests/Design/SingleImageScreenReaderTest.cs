using Inkybot.Contracts;

namespace Tests.Design
{
    public abstract class SingleImageScreenReaderTest : ScreenReaderTest
    {
        protected abstract string Path {
            get;
        }

        protected override void AddServices() {
            var screen = new FileScreenCapture(Path);
            _services[typeof(ScreenCapture)] = screen;
        }
    }
}
