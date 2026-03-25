using NLog;

namespace Inkybot.Services
{
    public static class MetricsLogger
    {
        private static readonly Logger Logger = LogManager.GetLogger("mage");

        public static void Track(string metric, object fields = null)
        {
            var ev = new LogEventInfo(LogLevel.Info, "mage", metric);
            ev.Properties["metric"] = metric;
            ev.Properties["session_id"] = Program.InstanceIdentifier;
            if (fields != null)
                foreach (var prop in fields.GetType().GetProperties())
                    ev.Properties[prop.Name] = prop.GetValue(fields);
            Logger.Log(ev);
        }
    }
}
