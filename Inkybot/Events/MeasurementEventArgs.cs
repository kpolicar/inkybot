using System;
using Inkybot.Dofus;
using Inkybot.Helpers;

namespace Inkybot.Events
{
    public class MeasurementEventArgs : EventArgs
    {
        public readonly Responsive.Measurement Measurement;

        public MeasurementEventArgs(Responsive.Measurement measurement) =>
            Measurement = measurement;
    }
}
