using System.Configuration;

namespace Inkybot.Resources
{
    #pragma warning disable 8618
    [SettingsSerializeAs(SettingsSerializeAs.Xml)]
    public class ItemPreset
    {
        public string Name { get; set; }
        public ItemStatPreset[] Stats { get; set; }
    }
    #pragma warning restore 8618
}
