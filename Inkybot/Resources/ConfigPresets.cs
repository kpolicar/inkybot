using System.Configuration;

namespace Inkybot.Resources
{
    #pragma warning disable 8618
    [SettingsSerializeAs(SettingsSerializeAs.Xml)]
    public class ConfigPresets
    {
        public ConfigPreset[] Presets = {};
    }
    #pragma warning restore 8618
}
