using System.Configuration;

namespace Inkybot.Resources
{
    #pragma warning disable 8618
    [SettingsSerializeAs(SettingsSerializeAs.Xml)]
    public class StatConfigPreset : StatConfig
    {
        public string Stat { get; set; }
    }
    #pragma warning restore 8618
}
