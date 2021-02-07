using System;
using System.Collections.Generic;
using Inkybot.Dofus;
using Inkybot.Dofus.Contracts;

namespace Inkybot.Contracts
{
    public interface UserSettingsConfigManager : StatConfigProvider
    {
        public bool RestoreHighSinkStats { get; set; }
        public bool AutoRestartBot { get; set; }
        public bool ShowUserWarnings { get; set; }
        public bool PublishExos { get; set; }
        public bool EnableRuneChecking { get; set; }
        
        public void SetConfig(Stat stat, in StatConfig config);
    }
}
