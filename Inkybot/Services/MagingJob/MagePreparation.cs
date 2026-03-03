using System;
using Inkybot.Dofus;

namespace Inkybot.Services
{
    internal static class MagePreparation
    {
        public static Item Prepare(
            MageSession session,
            ScreenReaderDataProvider dataProvider,
            ConfigManager configManager) {

            var previousItem = session.PreviousItem;
            session.ResetForNewItem();
            session.IsPreparing = true;

            try {
                session.IsMaging = true;
                return ReadItemFromScreen(session, previousItem, dataProvider, configManager);
            } catch (Exception) {
                session.IsMaging = false;
                throw;
            } finally {
                session.IsPreparing = false;
            }
        }

        private static Item ReadItemFromScreen(
            MageSession session,
            Item previousItem,
            ScreenReaderDataProvider dataProvider,
            ConfigManager configManager) {

            if (session.ShouldResetMinMaxScan())
                dataProvider.ResetMinMaxScan();

            dataProvider.Reset(!session.IsRestarting);
            dataProvider.FetchData();

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
