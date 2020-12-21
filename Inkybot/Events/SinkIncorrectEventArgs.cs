using System;

namespace Inkybot.Events
{
    public class SinkIncorrectEventArgs : EventArgs
    {
        public readonly float Sink;


        public SinkIncorrectEventArgs(float sink) {
            this.Sink = sink;
        }
    }
}
