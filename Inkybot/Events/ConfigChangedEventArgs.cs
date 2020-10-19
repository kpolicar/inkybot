using System;

namespace Inkybot.Events
{
    public class ConfigChangedEventArgs : EventArgs
    {
        public readonly Config config;

        public ConfigChangedEventArgs(Config config) {
            this.config = config;
        }
    }
}
