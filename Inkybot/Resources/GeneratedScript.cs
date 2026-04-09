using System;
using System.Configuration;

namespace Inkybot.Resources
{
    [SettingsSerializeAs(SettingsSerializeAs.Xml)]
    public class GeneratedScript
    {
        public int Number { get; set; }
        public string Code { get; set; } = "";
        public string FirstPrompt { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsApplied { get; set; }
    }

    [SettingsSerializeAs(SettingsSerializeAs.Xml)]
    public class GeneratedScripts
    {
        public GeneratedScript[] Scripts { get; set; } = new GeneratedScript[0];
    }
}
