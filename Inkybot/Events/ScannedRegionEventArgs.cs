using System;

namespace Inkybot.Events
{
    public class ScannedRegionEventArgs : EventArgs
    {
        public readonly string[] Lines;

        public ScannedRegionEventArgs(string[] lines) {
            this.Lines = lines;
        }
    }
}
