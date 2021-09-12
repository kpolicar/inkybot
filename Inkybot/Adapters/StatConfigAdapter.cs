using Inkybot.Dofus;
using Inkybot.Domain;
using Inkybot.Helpers;
using ItemStatMageConfig = Inkybot.Dofus.MageConfig.ItemStatMageConfig;

namespace Inkybot.Adapters
{
    public class StatConfigAdapter
    {
        private readonly Stat Stat;
        private readonly StatConfig Config;

        public StatConfigAdapter(Stat stat, StatConfig config) {
            Stat = stat;
            Config = config;
        }

        public Resources.StatConfigPreset ToSerializable() {
            return new Resources.StatConfigPreset {
                Stat = Stat.Identifier,
                UseSmRunes = Config.UseSmRunes,
                UsePaRunes = Config.UsePaRunes,
                UseRaRunes = Config.UseRaRunes,
                ChangeToPaRuneThreshold = Numbers.ToString(Config.ChangeToPaRuneThreshold),
                ChangeToRaRuneThreshold = Numbers.ToString(Config.ChangeToRaRuneThreshold),
                MaxValueAtWhichSmRuneCanLand = Numbers.ToString(Config.MaxValueAtWhichSmRuneCanHit),
                MaxValueAtWhichPaRuneCanLand = Numbers.ToString(Config.MaxValueAtWhichPaRuneCanHit),
            };
        }
    }
}
