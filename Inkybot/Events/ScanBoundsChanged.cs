using System;
using Inkybot.Helpers;

namespace Inkybot.Events
{
    public class ScanBoundsChanged : EventArgs
    {
        public readonly Responsive.Measurement ScanBounds;

        public ScanBoundsChanged(Responsive.Measurement scanBounds) {
            this.ScanBounds = scanBounds;
        }
    }
}
