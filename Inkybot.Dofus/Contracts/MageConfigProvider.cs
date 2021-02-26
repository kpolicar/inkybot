using System.Collections.Generic;

namespace Inkybot.Dofus.Contracts
{
    /**
     * <summary>
     * The **MageConfigProvider** interface specifies configuration options related to the maging
     * process, independant of the active item.
     * </summary>
     */
    public interface MageConfigProvider
    {
        /**
         * <summary>
         * A configuration detail provided by the ConfigManager which determines whether the AI
         * should prioritize the restoration of high sink stats immediately.
         * </summary>
         */
        public bool RestoreHighSinkStatsImmediately {
            get;
        }
    }
}
