using Inkybot.Events;

namespace Inkybot.Services
{
    public partial class ScreenReaderDofusMagingJob
    {
        private class Supervisor
        {
            private readonly ScreenReaderDofusMagingJob job;

            public Supervisor(ScreenReaderDofusMagingJob job) {
                this.job = job;
            }
        }
    }
}
