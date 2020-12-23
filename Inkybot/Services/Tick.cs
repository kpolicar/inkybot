using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Inkybot.Actions;
using Inkybot.Contracts;
using Inkybot.Design;
using Inkybot.Domain;
using Inkybot.Events;
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
            private const int MaxHistoryChangedChecks = 5;
            private static int HistoryChangedChecksCount = 0;

            public Tick(ScreenReaderDofusMagingJob job) {
                this.job = job;
                actions = Program.Services.GetService<ActionFactory>();
            }

            public void Execute() {
                switch (job.state) {
                    case State.STANDARD:
                        DoMainMageAction();
                        break;
                    case State.EXECUTING_COMBINE:
                        if (job.previousAction is CombineRune previousCombine && previousCombine.Exo)
                            DoHistoryCheckForChanges();
                        else
                            DoRuneCheckForChanges();
                        break;
                    case State.CALCULATING_SINK_CHANGE:
                        CalculateSinkChange();
                        break;
                    case State.CALCULATING_PRICE_CHANGE:
                        CalculatePriceChange();
                        break;
                }
            }

            private void DoMainMageAction() {
                //job.dataProvider.FetchData();
                var action = job.previousAction = DoAction();

                if (action is CombineRune) {
                    job.state = State.EXECUTING_COMBINE;
                }
                
                Thread.Sleep(100);
            }

            private void DoRuneCheckForChanges() {
                EnforceChangeTimeoutRunningAndNotFinished();
                
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
                    previousUserRune.Quantity = userRune.Quantity;
                } else {
                    Thread.Sleep(30);
                }
                job.RuneQuantityChanged?.Invoke(this, new RuneQuantityChangedEventArgs(userRune.Rune, previousUserRune.Quantity, userRune.Quantity));

            }
            
            private void DoHistoryCheckForChanges() {
                previousTickDeferredExecutionTask?.Wait();
                EnforceChangeTimeoutRunningAndNotFinished();

                job.dataProvider.FetchData();
                var itemHistory = job.history.Analyse(job.dataProvider.History());

                var historyHasChanged = itemHistory.IsDifferentFrom(job.previousHistory) ||
                                        (job.previousHistory == null && itemHistory.history.Count() > 0);

                if (!historyHasChanged) {
                    Thread.Sleep(100);
                } else {
                    var historyRecord = itemHistory.history.First();
                    EnforceValidPreviousActionResult(historyRecord);
                    
                    ChangeSinkFromLastAction(historyRecord);
                    job.state = State.STANDARD;
                    job.previousHistory = itemHistory;
                    job.changeTimeout.Stop();
                }
            }

            private void CalculateSinkChange() {
                EnforceChangeTimeoutRunningAndNotFinished();
                HistoryChangedChecksCount++;
                
                // Todo: continue with standard job (calculate sink change async) then wait before AI resolving action for calculation to complete
                var itemLatestHistory = job.history.Analyse(job.dataProvider.LatestHistory(), false);
                var latestChange = itemLatestHistory.history.FirstOrDefault();

                if (latestChange == default) {
                    job.dataProvider.FetchData();
                    return;
                }
                
                job.changeTimeout.Stop();
                try {
                    EnforceValidPreviousActionResult(latestChange);
                    EnforceDifferentHistory(itemLatestHistory);
                } catch (UnexpectedMageResultException exception) {
                    if (HistoryChangedChecksCount >= MaxHistoryChangedChecks)
                        throw;
                    
                    job.Warning?.Invoke(this, new MagingJobErrorEventArgs(exception, $"Attempt #{HistoryChangedChecksCount} out of ${MaxHistoryChangedChecks}"));
                    Thread.Sleep(50);
                    job.dataProvider.FetchData();
                    return;
                }

                ChangeSinkFromLastAction(latestChange);
                job.dataProvider.ApproveLatestHistoryContinueToNextScanBounds();
                job.state = State.CALCULATING_PRICE_CHANGE;
                job.previousHistory = itemLatestHistory;
                HistoryChangedChecksCount = 0;
            }
            
            private void CalculatePriceChange() {
                Task.Run(() => {
                    var balance = job.dataProvider.AverageItemBalance();
                    if (balance == null) return;

                    job.Balance = (int) balance;
                });
                job.state = State.STANDARD;
            }

            private void HandleChangeCheckTimeout() {
                job.changeTimeout.Stop();
                throw new ChangeCheckTimeoutException(
                    "Rune combination was expected to perform within 5 seconds, but did not. " +
                    "This may be the result of a poor internet connection or you may have run out of runes.");
            }

            // Todo: We can also check if the expected result is correct by comparing sink change.
            // Todo: the previous history is sometimes missing the last mage record: take a screenshot
            private void EnforceValidPreviousActionResult(MageHistoryRecord lastHistoryRecord) {
                Debug.WriteLine("it is null: ");
                var attempted = lastHistoryRecord.Landed;
                if (!attempted.HasValue) {
                    return;
                }
                var statLanded = lastHistoryRecord.Landed?.stat;

                var previousCombine = (CombineRune) job.previousAction;
                var expectedStat = previousCombine.Rune.stat;

                if (statLanded != expectedStat)
                    throw new UnexpectedMageResultException(
                        $"Expected \"{expectedStat.DisplayName}\" to land, not \"{statLanded.DisplayName}\"! " +
                        $"Have you run out of \"{expectedStat.DisplayName}\" runes?");
            }

            private void EnforceDifferentHistory(ItemHistoryAnalysis itemHistory) {
                if (!itemHistory.SuitableForCompare)
                    return;
                if (job.previousHistory != null && !itemHistory.IsDifferentFrom(job.previousHistory))
                    throw new HistoryHasntChangedException(itemHistory, job.previousHistory);
            }


            private void ChangeSinkFromLastAction(MageHistoryRecord lastHistoryRecord) {
                var sink = job.Sink;
                float sinkChange;
                float attemptedSinkChange;
                
                try {
                    sinkChange = lastHistoryRecord.ChangeInSink;
                    
                    if (sinkChange != 0)
                        attemptedSinkChange = lastHistoryRecord.Landed!.Value.SinkModifier;
                    else
                        attemptedSinkChange = 0;
                    
                    Debug.WriteLine("sink change:" +
                                    lastHistoryRecord.ChangeInSink);
                } catch (CouldNotResolveSinkException e) {

                    var previousCombine = (CombineRune) job.previousAction;
                    sinkChange = lastHistoryRecord.ChangeInSinkFromFallen - previousCombine.Rune.Sink;
                    
                    attemptedSinkChange = -previousCombine.Rune.Sink;

                    Debug.WriteLine("sink change:" +
                                    (lastHistoryRecord.ChangeInSinkFromFallen - previousCombine.Rune.Sink));
                }

                if (sinkChange > 0 && -attemptedSinkChange <= sink) {
                    job.Warning?.Invoke(this, new MagingJobErrorEventArgs(new SinkIncorrectException(sinkChange, sink, attemptedSinkChange), "Something had to have gone wrong in sink calculation! Resetting sink!"));
                    sink = 0;
                } else {
                    sink += sinkChange;
                }

                if (sink < 0) {
                    job.Warning?.Invoke(this, new MagingJobErrorEventArgs(new SinkNegativeException(sink), ""));
                }

                job.Sink = Math.Max(0f, sink);
            }


            private IAction DoAction() {
                var item = job.dataProvider.Item();
                if (item.IsInvalid)
                    throw new NoItemToMageFoundException("Could not gather item stats from screen");
                
                EnforceStatsChanged(item);
                job.previousItem = item;

                previousTickDeferredExecutionTask?.Wait();
                var action = job.magus.ResolveAction(item);

                if (action is CombineRune combine) {
                    EnforceHasRunesForCombine(combine);

                    if (combine.Exo) {
                        previousTickDeferredExecutionTask = Task.Run(() => {
                            job.previousHistory = job.history.Analyse(job.dataProvider.History());
                        });
                    }
                }
                
                job.actions.Execute(action);
                Debug.WriteLine("executed action "+action);

                return action;
            }

            private void EnforceHasRunesForCombine(CombineRune combine) {
                var hasRune = job
                    .itemInfo
                    .Runes.TryGetValue(combine.Rune.stat, out var previousUserRunes);
                if (!hasRune)
                    return;
                
                var previousUserRune = 
                    previousUserRunes.First(userRune => userRune.Rune == combine.Rune);
                if (previousUserRune.Quantity == 0) {
                    if (job.previousCheckHadRunOutOfRunes)
                        throw new OutOfRunesException(previousUserRune.Rune);
                    job.previousCheckHadRunOutOfRunes = true;
                } else {
                    job.previousCheckHadRunOutOfRunes = false;
                }
            }

            private void EnforceStatsChanged(Item item) {
                if (job.previousItem != null) {
                    var areDifferent =
                        job.previousHistory?.history.First().Landed == null ||
                        job.previousItem != item;
                
                    if (!areDifferent)
                        throw new UnexpectedMageResultException("Expected Stats to change but didn't");
                }
            }

            private void EnforceChangeTimeoutRunningAndNotFinished() {
                if (!job.changeTimeout.IsRunning)
                    job.changeTimeout.Restart();
                
                if (job.changeTimeout.ElapsedMilliseconds > 5000)
                    HandleChangeCheckTimeout();
            }
        }
    }
}
