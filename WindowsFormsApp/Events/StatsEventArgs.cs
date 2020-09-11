using System;
using System.Collections.Generic;

namespace WindowsFormsApp.Events
{
    public delegate void StatsEventHandler(object sender, StatsEventArgs e);

    public class StatsEventArgs : EventArgs
    {
        public Dictionary<string, string> stats;
        
        
        public StatsEventArgs(Dictionary<string, string> stats) {
            this.stats = stats;
        }
    }
}