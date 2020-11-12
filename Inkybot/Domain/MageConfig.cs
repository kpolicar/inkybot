namespace Inkybot.Domain
{
    public class MageConfig
    {
        public bool RestoreHighSinkStatsFirst => Properties.Settings.Default.restoreHighSinkStatImmediately;
    }
}
