using System.Collections.Generic;
using System.Configuration;

namespace Inkybot.Resources
{
    #pragma warning disable 8618
    [SettingsSerializeAs(SettingsSerializeAs.Xml)]
    public class ConfigPreset
    {
        public string Name { get; set; }
        public string? CustomScriptPath { get; set; }
        public StatConfigPreset[] Configs { get; set; }
    }
    #pragma warning restore 8618
}
