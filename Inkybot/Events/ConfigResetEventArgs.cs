using System;
using Inkybot.Dofus;

namespace Inkybot.Events
{
    public class ConfigResetEventArgs : EventArgs
    {
        public readonly Item Item;
        public readonly MageConfig Config;
        public readonly MageConfig? PreviousConfig;

        public ConfigResetEventArgs(Item item, MageConfig config, MageConfig? previousConfig) =>
            (Item, Config, PreviousConfig) = (item, config, previousConfig);
    }
}
