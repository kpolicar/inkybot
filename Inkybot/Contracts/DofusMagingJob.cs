using System;
using Inkybot.Dofus;
using Inkybot.Dofus.Contracts;
using Inkybot.Events;

namespace Inkybot.Contracts
{
    public interface DofusMagingJob : DofusSinkProvider
    {
        public event EventHandler Starting;
        public event EventHandler Enqueued;
        public event EventHandler<MagingJobStartedEventArgs>? Started;
        public event EventHandler? Preparing;
        public event EventHandler? Stopped;
        public event EventHandler<MagingJobFinishedEventArgs>? Finished;
        public event EventHandler<SinkChangedEventArgs>? SinkChanged;
        public event EventHandler<BalanceChangedEventArgs>? BalanceChanged;
        public event EventHandler<BalanceChangedEventArgs>? BalanceSpent;
        public event EventHandler<RuneQuantityChangedEventArgs>? RuneQuantityChanged;
        public event EventHandler<MagingJobErrorEventArgs>? Error;
        public event EventHandler<MagingJobErrorEventArgs>? Warning;
        
        public void BeginMage();
        public void BeginMage(bool begin);
        public void EnqueueMage();
        public void StopMage();
        
        public bool IsMaging { get; }
        MageHistoryRecord? LastHistoryRecord { get; }
    }
}
