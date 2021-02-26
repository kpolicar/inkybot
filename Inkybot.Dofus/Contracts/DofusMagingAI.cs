using System;
using System.Linq;
using Inkybot.Design;
using Inkybot.Dofus;
using Inkybot.Dofus.Domain;

namespace Inkybot.Dofus.Contracts
{
    /**
     * <summary>
     * The DofusMagingAI abstract class represents a maging AI instance.
     * The maging AI is used to determine what action to take next in regards
     * to the current item stats.
     * </summary>
     */
    public abstract class DofusMagingAI : HasDependencies
    {
        /**
         * <summary>An action factory instance providing all possible actions the maging bot can execute.</summary>
         */
        private ActionFactory Action = null!;
        
        /**
         * <summary>A stat config provider instance providing the active stat configuration.</summary>
         */
        private StatConfigProvider ConfigProvider = null!;
        
        /**
         * <summary>
         * The current item being resolved. Used internally to provide helper methods without the need
         * to pass arguments.
         * </summary>
         */
        private Item resolving = null!;

        public virtual void BindDependencies(ServiceContainer serviceContainer) =>
            (Action, ConfigProvider) = 
            (serviceContainer.GetService<ActionFactory>(), serviceContainer.GetService<StatConfigProvider>());

        /**
         * <summary>
         * Initialize the Maging AI.
         * The method is called on creation, after the dependencies have been injected
         * </summary>
         */
        public virtual void Init() {
        }

        
        /**
         * <summary>A helper method used to provide a combine action for the specified stat.</summary>
         */
        public IAction Combine(Stat stat) {
            if (!resolving!.HasStat(stat))
                return Combine(stat.StrongestRune);
            
            var runeType = ResolveRuneType(stat);
            return Combine(new Rune(stat, runeType));
        }

        /**
         * <summary>A helper method used to provide a combine action for the specified rune.</summary>
         */
        public IAction Combine(Rune rune) =>
            Action.CombineRune(rune, !resolving.HasStat(rune.Stat));

        
        /**
         * <summary>A helper method used to provide a finish action.</summary>
         */
        public IAction Finish() =>
            Action.Finish(resolving);

        
        /**
         * <summary>Return the next action that should be taken for the specified item.</summary>
         */
        protected abstract IAction Resolve(Item item);

        
        /**
         * <summary>Return the next action that should be taken for the specified item.</summary>
         */
        public IAction ResolveAction(Item item) {
            resolving = item;
            var result = Resolve(item);
            resolving = null!;
            return result;
        }
        
        /**
         * <returns>Resolve the rune type that should be used for the specified stat.</returns>
         */
        private Rune.RuneType ResolveRuneType(Stat stat) {
            var itemStat = resolving.Stats[stat]!;
            var itemConfig = ConfigProvider.Config(stat);
            
            if (itemConfig.ShouldUseRaRunes && itemStat.Value >= itemConfig.ChangeToRaRuneThreshold) return Rune.RuneType.Ra;
            if (itemConfig.ShouldUsePaRunes && itemStat.Value >= itemConfig.ChangeToPaRuneThreshold) return Rune.RuneType.Pa;

            return Rune.RuneType.Sm;
        }
    }
}
