using System.Collections.Generic;
using Inkybot.Dofus;
using StatConfig = Inkybot.Dofus.Stat.StatConfig;

namespace Inkybot.Contracts
{
    public interface DefaultConfigProvider
    {
        public Dictionary<Stat, StatConfig> DefaultStatConfig();
    }
}
