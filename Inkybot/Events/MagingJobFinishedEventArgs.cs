using System;
using Inkybot.Domain;

namespace Inkybot.Events
{
    public class MagingJobFinishedEventArgs : MagingJobEventArgs
    {
        public MagingJobFinishedEventArgs(Item item) : base(item) {
        }
    }
}
