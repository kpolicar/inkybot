using System.Collections.Generic;

namespace Inkybot.Dofus.Contracts
{
    public interface StatConfigProvider
    {
        public StatConfig Config(Stat stat);
        public Dictionary<Stat,StatConfig> Config();
    }
}
