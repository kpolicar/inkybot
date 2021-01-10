using System.Collections.Generic;

namespace Inkybot.Dofus.Contracts
{
    public interface MageConfigProvider
    {
        public bool RestoreHighSinkStatsImmediately {
            get;
        }
    }
}
