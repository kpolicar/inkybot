using Inkybot.Dofus;
using Inkybot.Dofus.Contracts;

namespace Inkybot.Contracts
{
    public interface IMagingConfigManager : MageConfigManager
    {
        void EnforceConfigSetForItem(Item item);
        void RemoveFallenUnconfiguredStats(Item item);
        bool ConfigIsSetForItem(Item item);
        UserSettingsConfigManager UserSettings { get; }
    }
}
