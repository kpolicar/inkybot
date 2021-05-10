using System.Collections.Generic;
using System.Linq;
using Inkybot.Contracts;
using Inkybot.Design;
using Inkybot.Dofus;
using Inkybot.Dofus.Contracts;
using Inkybot.Helpers;
using StatConfigProviderContract = Inkybot.Dofus.Contracts.StatConfigProvider;

namespace Inkybot.Services
{
    public class StatConfigProvider : StatConfigProviderContract, HasDependencies
    {
        public StatConfigProviderContract Default => DefaultStatConfigProvider.Instance;
        private UserSettingsConfigManager UserDefault = null!;
        
        public StatConfig Config(Stat stat) {
            return UserDefault.Config(stat);
        }
        
        public void BindDependencies(ServiceContainer serviceContainer) {
            UserDefault = serviceContainer.GetService<UserSettingsConfigManager>();
        }

        public Dictionary<Stat, StatConfig> Config() {
            return UserDefault.Config();
        }
    }
}
