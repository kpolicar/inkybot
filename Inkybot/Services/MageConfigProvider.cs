using System.Collections.Generic;
using System.Linq;
using Inkybot.Contracts;
using Inkybot.Design;
using Inkybot.Dofus;
using Inkybot.Dofus.Contracts;
using Inkybot.Helpers;
using MageConfigProviderContract = Inkybot.Dofus.Contracts.MageConfigProvider;

namespace Inkybot.Services
{
    public class MageConfigProvider : MageConfigProviderContract, HasDependencies
    {
        private MageConfigProviderContract Default => DefaultMageConfigProvider.Instance;
        private UserSettingsConfigManager UserDefault;
        
        public void BindDependencies(ServiceContainer serviceContainer) {
            UserDefault = serviceContainer.GetService<UserSettingsConfigManager>();
        }

        public bool RestoreHighSinkStatsImmediately =>
            UserDefault.RestoreHighSinkStats;
    }
}
