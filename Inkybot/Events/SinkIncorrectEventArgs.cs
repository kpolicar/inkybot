using System;

namespace Inkybot.Events
{
    public class SinkIncorrectEventArgs : EventArgs
    {
        public readonly decimal Sink;


        public SinkIncorrectEventArgs(decimal sink) {
            this.Sink = sink;
        }
    }
}
