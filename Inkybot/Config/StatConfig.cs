using System;
using System.ComponentModel;
using System.Configuration;
using System.Xml.Serialization;

namespace Inkybot.Config
{
    [SettingsSerializeAs(SettingsSerializeAs.Xml)]
    public class StatConfig
    {
        public int ChangeToPaRuneThreshold { get; set; }
        public int ChangeToRaRuneThreshold { get; set; }
        public int MaxValueSmRuneCanHit { get; set; }
        public int MaxValuePaRuneCanHit { get; set; }
        
    }
}
