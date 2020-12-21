using System;
using Inkybot.Domain;

namespace Inkybot.Events
{
    public class SinkChangedEventArgs : MagingJobEventArgs
    {
        public readonly float Sink;
        public readonly float OldSink;


        public SinkChangedEventArgs(Item item, float oldSink, float sink) : base(item) {
            this.Sink = sink;
            this.OldSink = oldSink;
        }
    }
}
