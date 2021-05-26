using System;

namespace Inkybot.Contracts
{
    public interface AnalyticsReporter
    {
        public event EventHandler? ExoAttempt;
    }
}
