using System;

namespace Inkybot.Events
{
    public class SinkChangedEventArgs : EventArgs
    {
        public readonly float sink;


        public SinkChangedEventArgs(float sink) {
            this.sink = sink;
        }
    }
}
