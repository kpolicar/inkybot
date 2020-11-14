using System;

namespace Inkybot.Events
{
    public class SinkChangedEventArgs : EventArgs
    {
        public readonly float Sink;
        public readonly float OldSink;


        public SinkChangedEventArgs(float oldSink, float sink) {
            this.Sink = sink;
            this.OldSink = oldSink;
        }
    }
}
