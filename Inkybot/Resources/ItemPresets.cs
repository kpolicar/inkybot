using System.Configuration;

namespace Inkybot.Resources
{
    #pragma warning disable 8618
    [SettingsSerializeAs(SettingsSerializeAs.Xml)]
    public class ItemPresets
    {
        public ItemPreset[] Presets;
    }
    #pragma warning restore 8618
}
