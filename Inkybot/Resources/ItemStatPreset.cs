using System.Configuration;

namespace Inkybot.Resources
{
    #pragma warning disable 8618
    [SettingsSerializeAs(SettingsSerializeAs.Xml)]
    public class ItemStatPreset
    {
        public string Stat { get; set; }
        public int? Target { get; set; }
        public int? TargetMinimum { get; set; }
        public int Maximum { get; set; }
        public int Minimum { get; set; }
    }
    #pragma warning restore 8618
}
