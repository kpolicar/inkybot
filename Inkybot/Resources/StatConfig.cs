using System;
using System.ComponentModel;
using System.Configuration;

namespace Inkybot.Resources
{
    [SettingsSerializeAs(SettingsSerializeAs.Xml)]
    public class StatConfig
    {
        public string ChangeToPaRuneThreshold { get; set; }
        public string ChangeToRaRuneThreshold { get; set; }
        public string MaxValueAtWhichSmRuneCanLand { get; set; }
        public string MaxValueAtWhichPaRuneCanLand { get; set; }
        
    }
}
