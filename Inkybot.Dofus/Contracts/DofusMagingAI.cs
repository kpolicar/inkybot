using Inkybot.Dofus;
using Inkybot.Dofus.Domain;

namespace Inkybot.Dofus.Contracts
{
    public abstract class DofusMagingAI
    {
        private ActionFactory Action = null!;
        private StatConfigProvider ConfigProvider = null!;
        private Item resolving = null!;

        public void AddServices(ActionFactory actions, StatConfigProvider statConfigProvider) =>
            (Action, ConfigProvider) = (actions, statConfigProvider);

        public virtual void Init() {
        }

        public IAction Combine(Stat stat) {
            if (!resolving!.HasStat(stat))
                return Combine(stat.StrongestRune);
            
            var runeType = ResolveRuneType(stat);
            return Combine(new Rune(stat, runeType));
        }

        public IAction Combine(Rune rune) =>
            Action.CombineRune(rune, !resolving.HasStat(rune.Stat));

        public IAction Finish() =>
            Action.Finish(resolving);

        public abstract IAction Resolve(Item item);

        public IAction ResolveAction(Item item) {
            resolving = item;
            var result = Resolve(item);
            resolving = null!;
            return result;
        }

        private Rune.RuneType ResolveRuneType(Stat stat) {
            var itemStat = resolving.Stats[stat]!;
            var itemConfig = ConfigProvider.Config(stat);
            
            if (itemConfig.ShouldUseRaRunes && itemStat.Value >= itemConfig.ChangeToRaRuneThreshold) return Rune.RuneType.Ra;
            if (itemConfig.ShouldUsePaRunes && itemStat.Value >= itemConfig.ChangeToPaRuneThreshold) return Rune.RuneType.Pa;

            return Rune.RuneType.Sm;
        }
    }
}
