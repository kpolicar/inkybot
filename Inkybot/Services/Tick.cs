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
            private static Task previousTickDeferredExecutionTask;

            public Tick(ScreenReaderDofusMagingJob job) {
                this.job = job;
                actions = (ActionFactory) Program.Services.GetService(typeof(ActionFactory));
            }

            public void Execute() {
                switch (job.state) {
                    case State.STANDARD:
                        DoMainMageAction();
                        break;
                    case State.EXECUTING_COMBINE:
                        if (job.previousAction is Combine previousCombine && previousCombine.Exo)
                            DoHistoryCheckForChanges();
                        else
                            DoRuneCheckForChanges();
                        break;
                    case State.CALCULATING_SINK_CHANGE:
                        CalculateSinkChange();
                        break;
                }
            }

            private void DoMainMageAction() {
                var action = job.previousAction = DoAction();

                if (action is Combine)
                    job.state = State.EXECUTING_COMBINE;
                
                previousTickDeferredExecutionTask = Task.Run(() => {
                    Thread.Sleep(300);
                    // Have to check if user has stopped maging during this sleep
                    if (!job.IsMaging)
                        return;
                    if (action is Combine combine && !combine.Exo) {
                        PersistRuneOnTable(combine);
                    }
                    Thread.Sleep(200);
                });
            }

            private void PersistRuneOnTable(Combine action) {
                job.actions.Execute(actions.SelectRune(action.Rune));
            }

            private void DoRuneCheckForChanges() {
                if (!job.changeTimeout.IsRunning)
                    job.changeTimeout.Restart();
                if (job.changeTimeout.ElapsedMilliseconds > 5000)
                    HandleChangeCheckTimeout();
                
                if (!(job.previousAction is RuneAction previousAction))
                    throw new SystemException("Cannot check for changes (previous action has no information about rune)");
                
                job.dataProvider.FetchData();

                var userRune = job.dataProvider.RuneQuantity(previousAction.Rune);
                var previousUserRune = job
                    .itemInfo
                    .Runes[previousAction.Rune.stat]
                    .First(userRune => userRune.Rune == previousAction.Rune);

                if (userRune.Quantity != previousUserRune.Quantity) {
                    job.state = State.CALCULATING_SINK_CHANGE;
                    job.changeTimeout.Stop();
                }

                Thread.Sleep(30);
            }
            
            private void DoHistoryCheckForChanges() {
                previousTickDeferredExecutionTask?.Wait();
                
                if (!job.changeTimeout.IsRunning)
                    job.changeTimeout.Restart();
                if (job.changeTimeout.ElapsedMilliseconds > 5000)
                    HandleChangeCheckTimeout();

                job.dataProvider.FetchData();
                var itemHistory = job.history.Analyse(job.dataProvider.History());

                var historyHasChanged = itemHistory.IsDifferentFrom(job.previousHistory);

                if (!historyHasChanged) {
                    Thread.Sleep(300);
                } else {
                    var historyRecord = itemHistory.history.Last();
                    ChangeSinkFromLastAction(historyRecord);
                    EnforceValidPreviousActionResult(historyRecord);
                    job.state = State.STANDARD;
                    job.changeTimeout.Stop();
                }
            }

            private void CalculateSinkChange() {
                if (!job.changeTimeout.IsRunning)
                    job.changeTimeout.Restart();
                
                MageHistoryRecord latestChange;
                do {
                    if (job.changeTimeout.ElapsedMilliseconds > 5000)
                        HandleChangeCheckTimeout();
                    if (!job.IsMaging)
                        return;
                    
                    // Todo: continue with standard job (calculate sink change async) then wait before AI resolving action for calculation to complete
                    job.dataProvider.FetchData();
                    var itemLatestHistory = job.history.Analyse(job.dataProvider.LatestHistory());
                    latestChange = itemLatestHistory.history.LastOrDefault();
                    Debug.WriteLine(latestChange);
                } while (latestChange == default);
                
                job.changeTimeout.Stop();
                EnforceValidPreviousActionResult(latestChange);

                ChangeSinkFromLastAction(latestChange);
                job.state = State.STANDARD;
            }

            private void HandleChangeCheckTimeout() {
                job.changeTimeout.Stop();
                throw new ChangeCheckTimeoutException(
                    "Rune combination was expected to perform within 5 seconds, but did not. " +
                    "This may be the result of a poor internet connection or you may have run out of runes.");
            }

            // Todo: We can also check if the expected result is correct by comparing sink change.
            private void EnforceValidPreviousActionResult(MageHistoryRecord lastHistoryRecord) {
                var attempted = lastHistoryRecord?.attempted;
                var statLanded = attempted?.stat;
                if (attempted == null || statLanded == null) {
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

                    Debug.WriteLine("sink change:" +
                                    (lastHistoryRecord.ChangeInSinkFromFallen - previousCombine.Rune.Sink));
                }

                job.Sink = Math.Max(0f, sink);
            }


            private IAction DoAction() {
                var item = job.dataProvider.Item();
                if (item.IsInvalid)
                    throw new NoItemToMageFoundException("Could not gather item stats from screen");

                previousTickDeferredExecutionTask?.Wait();
                var action = job.magus.ResolveAction(item, job.previousAction);

                if (action is Combine combine && combine.Exo) {

                    previousTickDeferredExecutionTask = Task.Run(() => {
                        job.previousHistory = job.history.Analyse(job.dataProvider.History());
                    });
                }

                job.actions.Execute(action);

                return action;
            }
        }
    }
}
