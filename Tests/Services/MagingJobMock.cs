using System;
using Inkybot.Contracts;
using Inkybot.Dofus;
using Inkybot.Events;

namespace Tests.Services
{
    public class MagingJobMock : DofusMagingJob
    {
        public event EventHandler<MagingJobStartedEventArgs>? Started;
        public event EventHandler? Starting;
        public event EventHandler? Preparing;
        public event EventHandler? Stopped;
        public event EventHandler<MagingJobFinishedEventArgs>? Finished;
        public event EventHandler<SinkChangedEventArgs>? SinkChanged;
        public event EventHandler<BalanceChangedEventArgs>? BalanceChanged;
        public event EventHandler<BalanceChangedEventArgs>? BalanceSpent;
        public event EventHandler<RuneQuantityChangedEventArgs>? RuneQuantityChanged;
        public event EventHandler<MagingJobErrorEventArgs>? Error;
        public event EventHandler<MagingJobErrorEventArgs>? Warning;
        
        public void BeginMage() =>
            IsMaging = true;

        public void BeginMage(bool begin) => 
            IsMaging = begin;

        public void StopMage() =>
            IsMaging = false;

        public bool IsMaging { get; private set; }
        public MageHistoryRecord? LastHistoryRecord { get; }

        private decimal sink;
        public decimal Sink {
            get => sink;
            set {
                SinkChanged?.Invoke(this, 
                    new SinkChangedEventArgs(null!, null!, 0m, value));
                sink = value;
            }
        }
    }
}
