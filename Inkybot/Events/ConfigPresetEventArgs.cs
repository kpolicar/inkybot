using System;
using Inkybot.Resources;

namespace Inkybot.Events
{
    public class ConfigPresetEventArgs : EventArgs
    {
        public readonly ConfigPreset? Preset;
        public readonly int? PresetIndex;


        public ConfigPresetEventArgs(ConfigPreset? preset, int? index) {
            (Preset, PresetIndex) = (preset, index);
        }
    }
}
