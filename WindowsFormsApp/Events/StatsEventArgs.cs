using System;
using System.Collections.Generic;

namespace WindowsFormsApp.Events
{
    public delegate void StatsEventHandler(object sender, StatsEventArgs e);

    public class StatsEventArgs : EventArgs
    {
        public Item.ItemStat[] stats;
        
        
        public StatsEventArgs(Item.ItemStat[] stats) {
            this.stats = stats;
        }
    }
}