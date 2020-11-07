using System;
using Inkybot.Domain.Repositories;

namespace Inkybot.Events
{
    public class StatsEventArgs : EventArgs
    {
        public readonly ItemStatRepository stats;


        public StatsEventArgs(ItemStatRepository stats) {
            this.stats = stats;
        }
    }
}
