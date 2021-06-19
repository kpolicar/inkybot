using Inkybot.Dofus;

namespace Inkybot.Events
{
    public class SinkChangedEventArgs : MagingJobEventArgs
    {
        public readonly decimal Sink;
        public readonly decimal OldSink;


        public SinkChangedEventArgs(Item item, MageConfig config, decimal oldSink, decimal sink) : base(item, config) {
            this.Sink = sink;
            this.OldSink = oldSink;
        }
    }
}
