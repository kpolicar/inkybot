using System.Collections.Generic;
using Inkybot.Dofus.Contracts;

namespace Inkybot.Dofus
{
    /**
     * <summary>A mage config provider with subjectively-reasonable default values</summary>
     */
    public class DefaultMageConfigProvider : MageConfigProvider
    {
        private static DefaultMageConfigProvider? _instance;
        public static DefaultMageConfigProvider Instance => _instance ??= new DefaultMageConfigProvider();

        private DefaultMageConfigProvider() {
        }

        public bool RestoreHighSinkStatsImmediately => true;
    }
}
