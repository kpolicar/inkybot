using System;
using System.ComponentModel;
using System.Configuration;

namespace Inkybot.Resources
{
    [SettingsSerializeAs(SettingsSerializeAs.Xml)]
    public class ItemStatPreset
    {
        public string Stat { get; set; }
        public int Target { get; set; }
        public int Maximum { get; set; }
    }
}
