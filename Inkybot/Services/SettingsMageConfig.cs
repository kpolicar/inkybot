using Inkybot.Contracts;

namespace Inkybot.Services
{
    public class SettingsMageConfig : MageConfig
    {
        public bool RestoreHighSinkStatsFirst => Properties.Settings.Default.restoreHighSinkStatImmediately;
    }
}
