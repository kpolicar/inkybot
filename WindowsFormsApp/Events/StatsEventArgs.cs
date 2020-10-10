using System;

namespace WindowsFormsApp.Events
{
    public class StatsEventArgs : EventArgs
    {
        public readonly Item.ItemStat[] stats;


        public StatsEventArgs(Item.ItemStat[] stats) {
            this.stats = stats;
        }
    }
}
