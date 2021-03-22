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
        
        private DofusSinkProvider SinkProvider = null!;
        
        private MageConfigManager MageConfigManager = null!;

        protected int Sink => (int) SinkProvider.Sink;
        
        /**
         * <summary>
         * The current item being resolved. Used internally to provide helper methods without the need
         * to pass arguments.
         * </summary>
         */
        protected Item Item = null!;

        public virtual void BindDependencies(ServiceContainer serviceContainer) =>
            (Action, ConfigProvider, SinkProvider, MageConfigManager) = 
            (serviceContainer.GetService<ActionFactory>(),
                serviceContainer.GetService<StatConfigProvider>(),
                serviceContainer.GetService<DofusSinkProvider>(),
                serviceContainer.GetService<MageConfigManager>());

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
            if (!Item!.HasStat(stat))
                return Combine(stat.StrongestRune);
            
            var runeType = ResolveRuneType(stat);
            return Combine(new Rune(stat, runeType));
        }

        /**
         * <summary>A helper method used to provide a combine action for the specified rune.</summary>
         */
        public IAction Combine(Rune rune) =>
            Action.CombineRune(rune, !Item.HasStat(rune.Stat));

        
        /**
         * <summary>A helper method used to provide a finish action.</summary>
         */
        public IAction Finish() =>
            Action.Finish(Item);

        
        /**
         * <summary>Return the next action that should be taken for the specified item.</summary>
         */
        protected abstract IAction Resolve();

        
        /**
         * <summary>Return the next action that should be taken for the specified item.</summary>
         */
        public IAction ResolveAction(Item item) {
            Item = item;
            var result = Resolve();
            Item = null!;
            return result;
        }

        protected ItemMage? ItemMage(Stat stat) =>
            ItemMage(new Rune(stat, ResolveRuneType(stat)));
        
        protected ItemMage? ItemMage(Rune rune) =>
            new ItemMage(Item,
                rune,
                MageConfigManager.Config?[rune.Stat] ?? MageConfig.ItemStatMageConfig.Default(rune.Stat));
        
        /**
         * <returns>Resolve the rune type that should be used for the specified stat.</returns>
         */
        private Rune.RuneType ResolveRuneType(Stat stat) {
            var itemStat = Item.Stats[stat]!;
            var itemConfig = ConfigProvider.Config(stat);
            
            if (itemConfig.ShouldUseRaRunes && itemStat.Value >= itemConfig.ChangeToRaRuneThreshold) return Rune.RuneType.Ra;
            if (itemConfig.ShouldUsePaRunes && itemStat.Value >= itemConfig.ChangeToPaRuneThreshold) return Rune.RuneType.Pa;

            return Rune.RuneType.Sm;
        }
    }
}
