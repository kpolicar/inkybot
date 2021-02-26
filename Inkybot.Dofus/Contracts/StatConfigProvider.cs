using System.Collections.Generic;

namespace Inkybot.Dofus.Contracts
{
    /**
     * <summary>
     * The **StatConfigProvider** interface specifies stat-specfic configuration options.
     * </summary>
     */
    public interface StatConfigProvider
    {
        /**
         * <summary>Provide configuration for the specified stat.</summary>
         */
        public StatConfig Config(Stat stat);
        
        /**
         * <summary>Provide a mapping of all the stat configuration.</summary>
         */
        public Dictionary<Stat,StatConfig> Config();
    }
}
