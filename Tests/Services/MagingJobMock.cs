using System;
using Inkybot.Contracts;
using Inkybot.Events;

namespace Tests.Services
{
    public class MagingJobMock : DofusMagingJob
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
        
        public void BeginMage() =>
            IsMaging = true;

        public void BeginMage(bool begin) => 
            IsMaging = begin;

        public void StopMage() =>
            IsMaging = false;

        public bool IsMaging { get; private set; }

        private float sink;
        public float Sink {
            get => sink;
            set {
                SinkChanged?.Invoke(this, 
                    new SinkChangedEventArgs(null!, null!, 0f, value));
                sink = value;
            }
        }
    }
}
