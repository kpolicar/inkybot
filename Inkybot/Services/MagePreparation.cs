using System;
using System.Linq;
using Inkybot.Actions;
using Inkybot.Dofus;

namespace Inkybot.Services
{
    internal static class MagePreparation
    {
        public static Item Prepare(
            MageSession session,
            bool restarting,
            ScreenReaderDataProvider dataProvider,
            ConfigManager configManager,
            Action<decimal> setSink) {

            var previousCheckHadRunOutOfRunes = session.PreviousCheckHadRunOutOfRunes;
            var previousItem = session.PreviousItem;
            session.Reset();
            session.IsPreparing = true;
            session.IsRestarting = restarting;
            session.PreviousCheckHadRunOutOfRunes = previousCheckHadRunOutOfRunes;

            try {
                session.IsMaging = true;
                return ReadItemFromScreen(session, previousItem, dataProvider, configManager, setSink);
            } catch (Exception) {
                session.IsMaging = false;
                throw;
            } finally {
                session.IsPreparing = false;
                session.IsRestarting = false;
            }
        }

        private static Item ReadItemFromScreen(
            MageSession session,
            Item previousItem,
            ScreenReaderDataProvider dataProvider,
            ConfigManager configManager,
            Action<decimal> setSink) {

            if (((session.PreviousAction as CombineRune)?.Exo ?? false) ||
                ((session.PreviousItem?.HasExo ?? false) && !session.PreviousItem.Stats.ExoStats.All(stat => stat.Value < 0))) {
                dataProvider.ResetMinMaxScan();
            }

            dataProvider.Reset(!session.IsRestarting);
            dataProvider.FetchData();
            setSink(dataProvider.Sink() ?? 0);

            var item = dataProvider.Item();
            try {
                session.PreviousHistory = dataProvider.History();
            } catch (Exception) {
                // History may not be available yet — that's fine.
            }

            configManager.EnforceConfigSetForItem(item);
            configManager.RemoveFallenUnconfiguredStats(item);
            session.Runes = dataProvider.Runes();

            session.PreviousItem = (previousItem != null && item.Equals(previousItem)) ? previousItem : null;
            return item;
        }
    }
}
