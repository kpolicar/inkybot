using System;
using System.ComponentModel;
using System.Configuration;

namespace Inkybot.Resources
{
    [SettingsSerializeAs(SettingsSerializeAs.Xml)]
    public class ItemPresets
    {
        public ItemPreset[] Presets;
    }
}
