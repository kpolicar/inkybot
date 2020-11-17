using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Inkybot.Actions;
using Inkybot.Contracts;
using Inkybot.Domain;
using Inkybot.Exceptions;

namespace Inkybot.Services
{
    public partial class ScreenReaderDofusMagingJob
    {
        internal class Tick
        {
            private readonly ActionFactory actions;
            private readonly ScreenReaderDofusMagingJob job;

            public Tick(ScreenReaderDofusMagingJob job) {
                this.job = job;
                actions = (ActionFactory) Program.Services.GetService(typeof(ActionFactory));
            }

            public void Execute() {
                job.dataProvider.FetchData();

                switch (job.state) {
                    case State.STANDARD:
                        DoMainMageAction();
                        break;
                    case State.EXECUTING_COMBINE:
                        DoRuneCheckForChanges();
                        break;
                    case State.CALCULATING_SINK_CHANGE:
                        DoHistoryCheckForChanges();
                        break;
                }
            }

            private void DoMainMageAction() {
                var action = job.previousAction = DoAction();

                // Have to check if user has stopped maging during this sleep
                if (!job.IsMaging)
                    return;
                if (action is Combine combine) {
                    job.state = State.EXECUTING_COMBINE;
                    if (!combine.Exo)
                        PersistRuneOnTable(combine);
                }
            }

            private void PersistRuneOnTable(Combine action) {
                job.actions.Execute(actions.SelectRune(action.Rune));
            }

            private void DoRuneCheckForChanges() {
                if (!(job.previousAction is RuneAction previousAction)) return;

                var userRune = job.dataProvider.RuneQuantity(previousAction.Rune);
                var previousUserRune = job
                    .itemInfo
                    .Runes[previousAction.Rune.stat]
                    .First(userRune => userRune.Rune == previousAction.Rune);

                Debug.WriteLine($"current: {userRune.Quantity}, previous: {previousUserRune.Quantity}");
                if (userRune.Quantity == previousUserRune.Quantity) {
                    Debug.WriteLine("it's the same boi!");
                } else {
                    Debug.WriteLine("it's different!");
                    job.state = State.CALCULATING_SINK_CHANGE;
                }

                Thread.Sleep(30);
            }

            private void DoHistoryCheckForChanges() {
                if (!job.historyCheckTimeout.IsRunning)
                    job.historyCheckTimeout.Restart();
                if (job.historyCheckTimeout.ElapsedMilliseconds > 5000)
                    HandleHistoryCheckTimeout();

                var itemHistory = job.history.Analyse(job.dataProvider.History());

                var historyHasChanged = itemHistory.IsDifferentFrom(job.previousHistory);

                if (!historyHasChanged) {
                    Thread.Sleep(300);
                } else {
                    var historyRecord = itemHistory.history.Last();
                    ChangeSinkFromLastAction(historyRecord);
                    EnforceValidPreviousActionResult(historyRecord);
                    job.state = State.STANDARD;
                    job.historyCheckTimeout.Stop();
                }
            }

            private void HandleHistoryCheckTimeout() {
                job.historyCheckTimeout.Stop();
                throw new HistoryChangeCheckTimeoutException(
                    "Mage history was expected to change within 5 seconds, but did not. " +
                    "This may be the result of a poor internet connection or you may have run out of runes.");
            }

            // Todo: We can also check if the expected result is correct by comparing sink change.
            private void EnforceValidPreviousActionResult(MageHistoryRecord lastHistoryRecord) {
                var attempted = lastHistoryRecord.attempted;
                var statLanded = attempted.stat;
                if (attempted.Equals(default(StatChanged)) || statLanded == null) {
                    return;
                }

                var previousCombine = (Combine) job.previousAction;
                var expectedStat = previousCombine.Rune.stat;

                if (statLanded != expectedStat)
                    throw new UnexpectedMageResultException(
                        $"Expected \"{expectedStat.DisplayName}\" to land, not \"{statLanded.DisplayName}\"! " +
                        $"Have you run out of \"{expectedStat.DisplayName}\" runes?");
            }


            private void ChangeSinkFromLastAction(MageHistoryRecord lastHistoryRecord) {
                var sink = job.Sink;
                try {
                    sink += lastHistoryRecord.ChangeInSink;
                } catch (CouldNotResolveSinkException e) {

                    var previousCombine = (Combine) job.previousAction;
                    sink += lastHistoryRecord.ChangeInSinkFromFallen - previousCombine.Rune.Sink;

                    Debug.WriteLine("Could not resolve history's change in sink, defaulting to applied rune!");
                    Debug.WriteLine("sink change:" +
                                    (lastHistoryRecord.ChangeInSinkFromFallen - previousCombine.Rune.Sink));
                }

                job.Sink = Math.Max(0f, sink);
            }


            private IAction DoAction() {
                var item = job.dataProvider.Item();
                if (item.IsInvalid)
                    throw new NoItemToMageFoundException("Could not gather item stats from screen");

                var action = job.magus.ResolveAction(item, job.previousAction);

                job.actions.Execute(action);

                if (action is Combine) {
                    var itemHistory = job.history.Analyse(job.dataProvider.History());
                    job.previousHistory = itemHistory;
                }

                return action;
            }
        }
    }
}
