using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using Inkybot.Actions;
using Inkybot.Dofus;
using Inkybot.Dofus.Contracts;
using Inkybot.Dofus.Domain;
using Inkybot.Domain;
using Inkybot.Events;
using Inkybot.Exceptions;

namespace Inkybot.Services
{
    /// <summary>
    /// Represents one complete rune combination cycle:
    /// resolve action → execute → wait for result → read sink.
    /// </summary>
    internal class Tick
    {
        private readonly MageSession session;
        private readonly ScreenReaderDataProvider dataProvider;
        private readonly DofusMagingAI magus;
        private readonly ActionHandler actions;
        private readonly ConfigManager configManager;

        public event EventHandler SuccessfulCombine;
        public event EventHandler<MagingJobStartedEventArgs> SensitiveMage;
        public Action<decimal> SetSink;

        public Tick(
            MageSession session,
            ScreenReaderDataProvider dataProvider,
            DofusMagingAI magus,
            ActionHandler actions,
            ConfigManager configManager) {
            this.session = session;
            this.dataProvider = dataProvider;
            this.magus = magus;
            this.actions = actions;
            this.configManager = configManager;
        }

        public void Execute() {
#if DEBUG
            Profiler.Reset();
#endif
            var action = ResolveAndExecuteAction();
            session.PreviousAction = action;

            if (action is CombineRune combineRune) {
                session.PreviousCombineWasExoAttempt = combineRune.Exo;
                if (combineRune.Exo || combineRune.Rune.Stat.Config.HighSinkStat) {
                    dataProvider.ResetMinMaxScan();
                    Thread.Sleep(800);
                }

                WaitForCombineResult();
                ReadSink();

                SuccessfulCombine?.Invoke(this, EventArgs.Empty);
                session.UnsuccessfulCombineTicks = 0;
            }
#if DEBUG
            Profiler.PrintSummary();
#endif
        }

        private IAction ResolveAndExecuteAction() {
            if (((session.PreviousAction as CombineRune)?.Exo ?? false) ||
                ((session.PreviousItem?.HasExo ?? false) && !session.PreviousItem.Stats.ExoStats.All(stat => stat.Value < 0))) {
                dataProvider.ResetMinMaxScan();
            }

            var item = dataProvider.Item();
            if (item.IsInvalid)
                throw new NoItemToMageFoundException("Could not gather item stats from screen");

            EnforceSameItemAsPreviousTick(item);
            session.PreviousItem = item;
            configManager.EnforceConfigSetForItem(item);
            configManager.RemoveFallenUnconfiguredStats(item);

            var action = magus.ResolveAction(item);

            if (action is CombineRune combine) {
                if (session.UnsuccessfulCombineTicks >= 5)
                    throw new OutOfRunesException(combine.Rune);

                EnforceHasRunesForCombine(combine);

                if (combine.Exo)
                    session.PreviousHistory = dataProvider.History();

                RaiseEventIfSensitiveMage(item);
            }

            actions.Execute(action);
            Debug.WriteLine("executed action " + action);

            return action;
        }

        private void WaitForCombineResult() {
            while (true) {
                EnforceChangeTimeout();

                dataProvider.Scan!.PrefetchForHistoryCheck();
                var itemHistory = dataProvider.History();

                if (Helpers.History.HasChanged(itemHistory, session.PreviousHistory)) {
                    Debug.WriteLine("history has changed");
                    session.PreviousHistory = itemHistory;
                    session.ChangeTimeout.Stop();
                    return;
                }

                Debug.WriteLine("history has not changed, refetching");
                dataProvider.FetchData();
            }
        }

        private void ReadSink() {
            var sink = dataProvider.Sink();
            if (sink == null) {
                dataProvider.FetchData();
                sink = dataProvider.Sink();
            }

            if (sink != null) {
                Debug.WriteLine("CHANGED SINK TO " + sink.Value.ToString(CultureInfo.InvariantCulture));
                SetSink?.Invoke(sink.Value);
            }
        }

        private void RaiseEventIfSensitiveMage(Item item) {
            var interrupted = false;
            var showDialogue = session.Ticks == 1 && item.Stats.Any(s => s.Overmaged);

            if (!showDialogue) {
                showDialogue = item.Stats.ExoStats.Any(s => s.Value > 0 && s.Stat.Config.HighSinkStat);
                if (showDialogue) interrupted = true;
            }

            if (showDialogue) {
                SensitiveMage?.Invoke(this,
                    new MagingJobStartedEventArgs(false, item, configManager.Config!, interrupted));
                if (!session.IsMaging)
                    throw new OperationCanceledException();
            }
        }

        private void EnforceSameItemAsPreviousTick(Item item) {
            if (session.PreviousItem != null && !item.MatchesStandardStatsStructure(session.PreviousItem))
                throw new ItemHasChangedException(item);
        }

        private void EnforceHasRunesForCombine(CombineRune combine) {
            if (session.Runes == null || !session.Runes.TryGetValue(combine.Rune.Stat, out var userRunes))
                return;

            var userRune = userRunes.First(r => r.Rune == combine.Rune);

            if (userRune.Quantity == 0 && session.PreviousCheckHadRunOutOfRunes == userRune.Rune
                && configManager.UserSettings.EnableRuneChecking)
                throw new OutOfRunesException(userRune.Rune);

            session.PreviousCheckHadRunOutOfRunes = userRune.Quantity == 0 ? userRune.Rune : null;
        }

        private void EnforceChangeTimeout() {
            if (!session.ChangeTimeout.IsRunning)
                session.ChangeTimeout.Restart();

            var elapsed = session.ChangeTimeout.ElapsedMilliseconds;
            if (elapsed > 15000 || (!session.PreviousCombineWasExoAttempt && elapsed > 5000)) {
                session.ChangeTimeout.Stop();
                var additional = session.PreviousAction is RuneAction runeAction
                    ? "\"" + runeAction.Rune.DisplayName + "\" "
                    : "";
                throw new ChangeCheckTimeoutException(
                    "Rune combination was expected to perform within 5 seconds, but did not. " +
                    $"This may be the result of a poor internet connection or you may have run out of {additional}runes.");
            }
        }
    }
}
