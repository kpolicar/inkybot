using System;
using System.Collections.Generic;
using Inkybot.Dofus;
using Inkybot.Resources;
using Inkybot.Services;
using StatConfig = Inkybot.Dofus.StatConfig;
using StatConfigProvider = Inkybot.Dofus.Contracts.StatConfigProvider;

namespace Inkybot.Contracts
{
    public interface UserSettingsConfigManager : StatConfigProvider
    {
        public bool RestoreHighSinkStats { get; set; }
        public bool AutoRestartBot { get; set; }
        public bool ShowUserWarnings { get; set; }
        public bool PublishExos { get; set; }
        public bool EnableKamasCalculation { get; set; }
        public bool EnableRuneChecking { get; set; }
        public bool EnableMageQueueing { get; set; }
        public decimal CustomResizeMultiplier { get; set; }
        public ItemPresets Presets { get; set; }
        
        public event EventHandler? EnableMageQueueingChanged;
        public event EventHandler? PresetsChanged;
        
        public void SetConfig(Stat stat, in StatConfig config);
    }
}
