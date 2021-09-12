using Inkybot.Dofus;
using Inkybot.Domain;
using ItemStatMageConfig = Inkybot.Dofus.MageConfig.ItemStatMageConfig;

namespace Inkybot.Adapters
{
    public class ItemStatConfigAdapter
    {
        private readonly Stat Stat;
        private readonly ItemStatMageConfig ItemConfig;

        public ItemStatConfigAdapter(Stat stat, ItemStatMageConfig itemConfig) {
            Stat = stat;
            ItemConfig = itemConfig;
        }

        public Resources.ItemStatPreset ToSerializable() {
            return new Resources.ItemStatPreset {
                Minimum = ItemConfig.Minimum,
                Maximum = ItemConfig.Maximum,
                Stat = Stat.Identifier,
                Target = ItemConfig.Target,
                TargetMinimum = ItemConfig.TargetMinimum,
                Priority = ItemConfig.Priority
            };
        }
    }
}
