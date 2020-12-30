using System;
using Inkybot.Events;

namespace Inkybot.Contracts
{
    public interface DofusMagingJob
    {
        public event EventHandler<MagingJobEventArgs>? Started;
        public event EventHandler? Preparing;
        public event EventHandler? Stopped;
        public event EventHandler<MagingJobFinishedEventArgs>? Finished;
        public event EventHandler<SinkChangedEventArgs>? SinkChanged;
        public event EventHandler<BalanceChangedEventArgs>? BalanceChanged;
        public event EventHandler<RuneQuantityChangedEventArgs>? RuneQuantityChanged;
        public event EventHandler<MagingJobErrorEventArgs>? Error;
        public event EventHandler<MagingJobErrorEventArgs>? Warning;
        
        public void BeginMage();
        public void BeginMage(bool begin);
        public void StopMage();
        
        public bool IsMaging { get; }
    }
}
