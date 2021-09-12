using System;
using Inkybot.Resources;

namespace Inkybot.Events
{
    public class PresetEventArgs : EventArgs
    {
        public readonly ItemPreset Preset;
        public readonly int? PresetIndex;


        public PresetEventArgs(ItemPreset preset, int index) {
            (Preset, PresetIndex) = (preset, index);
        }
    }
}
