using System;
using Inkybot.Events;

namespace Inkybot.Dofus.Contracts
{
    public interface MageConfigManager
    {
        public event EventHandler<ConfigModifiedEventArgs>? ConfigModified;
        public event EventHandler<ConfigResetEventArgs>? ConfigReset;
        
        public MageConfig? Config {
            get;
        }
    }
}
