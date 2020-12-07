using System;
using System.ComponentModel;
using System.Configuration;

namespace Inkybot.Resources
{
    [SettingsSerializeAs(SettingsSerializeAs.Xml)]
    public class ItemPreset
    {
        public string Name { get; set; }
        public ItemStatPreset[] Stats { get; set; }
        
    }
}
