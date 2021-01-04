using System.Configuration;

namespace Inkybot.Resources
{
    #pragma warning disable 8618
    [SettingsSerializeAs(SettingsSerializeAs.Xml)]
    public class StatConfig
    {
        public string ChangeToPaRuneThreshold { get; set; }
        public string ChangeToRaRuneThreshold { get; set; }
        public string MaxValueAtWhichSmRuneCanLand { get; set; }
        public string MaxValueAtWhichPaRuneCanLand { get; set; }
        public bool CanUsePaRunes { get; set; }
        public bool CanUseRaRunes { get; set; }
        
    }
    #pragma warning restore 8618
}
