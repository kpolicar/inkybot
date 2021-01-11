using System.Collections.Generic;
using Inkybot.Dofus.Contracts;

namespace Inkybot.Dofus
{
    public class DefaultMageConfigProvider : MageConfigProvider
    {
        private static DefaultMageConfigProvider? _instance;
        public static DefaultMageConfigProvider Instance => _instance ??= new DefaultMageConfigProvider();

        private DefaultMageConfigProvider() {
        }

        public bool RestoreHighSinkStatsImmediately => true;
    }
}
