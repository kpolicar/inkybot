using System.Collections.Generic;
using System.Linq;
using WindowsFormsApp.Actions;
using WindowsFormsApp.Contracts;
using static WindowsFormsApp.Item;

namespace WindowsFormsApp
{
    public class BasicDofusMagingAI : DofusMagingAI
    {
        private List<IAction> history;
        private ActionFactory actions;
        private Config config;

        public BasicDofusMagingAI() {
            actions = (ActionFactory) Program.Services.GetService(typeof(ActionFactory));
        }

        public void SetHistory(List<IAction> history) {
            this.history = history;
        }

        public void SetConfig(Config config) {
            this.config = config;
        }

        public IAction ResolveAction(ItemStat[] itemStats) {
            var prioritized = itemStats.OrderByDescending(StatPriority).ToArray();
            var targetStat = prioritized.FirstOrDefault();

            if (targetStat.max <= targetStat.value)
                return actions.Finish();

            var previous = history.LastOrDefault();

            if (previous == null ||
                previous is Combine ||
                previous is Combine && (previous as Combine).target.stat.DisplayName != targetStat.stat.DisplayName)
            {
                var rune = new Rune(targetStat.stat, ResolveRuneType(targetStat));
                
                return actions.SelectRune(rune);
            }
            
            return actions.Combine(targetStat);
        }

        private Rune.Type ResolveRuneType(ItemStat itemStat) {
            if (itemStat.value > config.For(itemStat).ChangeToRaRuneValue) {
                return Rune.Type.Ra;
            }

            if (itemStat.value > config.For(itemStat).ChangeToPaRuneValue) {
                return Rune.Type.Pa;
            }
            
            return Rune.Type.Sm;
        }

        private int StatPriority(ItemStat itemStat) {
            return config.For(itemStat).maximum - itemStat.value;
        }
    }
}