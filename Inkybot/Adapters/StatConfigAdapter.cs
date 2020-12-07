using Inkybot.Domain;

namespace Inkybot.Adapters
{
    public class StatConfigAdapter
    {
        private readonly Stat Stat;
        private readonly StatConfig StatConfig;

        public StatConfigAdapter(Stat stat, StatConfig statConfig) {
            Stat = stat;
            StatConfig = statConfig;
        }

        public Resources.ItemStatPreset ToSerializable() {
            return new Resources.ItemStatPreset {
                Maximum = StatConfig.Maximum,
                Stat = Stat.DisplayName,
                Target = StatConfig.Target
            };
        }
    }
}
