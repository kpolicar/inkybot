using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Inkybot.Actions;
using Inkybot.Contracts;
using Inkybot.Dofus;
using Inkybot.Dofus.Contracts;
using Inkybot.Dofus.Domain;
using Inkybot.Dofus.Exceptions;
using Inkybot.Domain;
using Inkybot.Events;
using Inkybot.Exceptions;

namespace Inkybot.Services
{
    public partial class ScreenReaderDofusMagingJob
    {
        private class Tick
        {
            private readonly ActionFactory actions;
            private readonly ScreenReaderDofusMagingJob job;
            private static Task? previousTickDeferredExecutionTask;
            private const int MaxHistoryChangedChecks = 5;
            private static int HistoryChangedChecksCount = 0;
            private const int MaxStatsChangedChecks = 3;
            private static int StatsChangedChecksCount = 0;
            private const int MaxStatsShouldHaveChangedChecks = 3;
            internal static int shouldveBeenDifferentCount = 0;

            public Tick(ScreenReaderDofusMagingJob job) {
                this.job = job;
                actions = Program.Services.GetService<ActionFactory>();
            }

            public void Execute() {
                var currentStep = job.state.Step;
                switch (job.state.Step) {
                    case State.JobStep.STANDARD:
                        DoMainMageAction();
                        break;
                    case State.JobStep.EXECUTING_COMBINE:
                        if (job.state.PreviousCombineWasExoAttempt || job.changeTimeout.ElapsedMilliseconds > 1500)
                            DoHistoryCheckForChanges();
                        else
                            DoRuneCheckForChanges();
                        break;
                    case State.JobStep.CALCULATING_SINK_CHANGE:
                        CalculateSinkChange();
                        break;
                    case State.JobStep.CALCULATING_PRICE_CHANGE:
                        CalculatePriceChange();
                        break;
                }

                var nextStep = job.state.Step;
                if (currentStep == State.JobStep.EXECUTING_COMBINE && nextStep != State.JobStep.EXECUTING_COMBINE)
                    job.SuccessfulCombineTick?.Invoke(this, EventArgs.Empty);
            }

            private void DoMainMageAction() {
                StatsChangedChecksCount++;

                try {
                    var action = job.state.PreviousAction = DoAction();

                    if (action is CombineRune combineRune) {
                        job.state.Step = State.JobStep.EXECUTING_COMBINE;
                        job.state.PreviousCombineWasExoAttempt = combineRune.Exo;
                    }
                } catch (ItemHasChangedException exception) {
                    if (MaxStatsChangedChecks >= StatsChangedChecksCount)
                        throw;
                    job.Warning?.Invoke(this, new MagingJobErrorEventArgs(exception, $"Attempt #{StatsChangedChecksCount} out of ${MaxStatsChangedChecks}"));
                    job.dataProvider.FetchData();

                    Thread.Sleep(100);
                    return;
                }

                StatsChangedChecksCount = 0;
                Thread.Sleep(100);
            }

            private void DoRuneCheckForChanges() {
                try {
                    EnforceChangeTimeoutRunningAndNotFinished();
                } catch (ChangeCheckTimeoutException exception) {
                    job.Warning?.Invoke(this, new MagingJobErrorEventArgs(exception, "Rune quantity check failed, looking for history changes."));
                    DoHistoryCheckForChanges();
                    if (job.state.Step != State.JobStep.STANDARD)
                        throw;
                    return;
                }
                
                if (!(job.state.PreviousAction is RuneAction previousAction))
                    throw new SystemException("Cannot check for changes (previous action has no information about rune)");
                
                job.dataProvider.FetchData();

                var newUserRune = job.dataProvider.RuneQuantity(previousAction.Rune);
                var userRune = job
                    .itemInfo
                    .Runes.Find(previousAction.Rune);

                if (newUserRune.Quantity != userRune.Quantity) {
                    job.state.Step = State.JobStep.CALCULATING_SINK_CHANGE;
                    job.changeTimeout.Stop();

                    var runeQuantityChange = new RuneQuantityChangedEventArgs(
                        newUserRune.Rune, userRune.Quantity, newUserRune.Quantity);

                    userRune.Quantity = newUserRune.Quantity;
                    job.RuneQuantityChanged?.Invoke(this, runeQuantityChange);
                    
                } else {
                    Thread.Sleep(30);
                }

            }
            
            private void DoHistoryCheckForChanges() {
                previousTickDeferredExecutionTask?.Wait();
                EnforceChangeTimeoutRunningAndNotFinished();

                job.dataProvider.FetchData();
                var itemHistory = job.history.Analyse(job.dataProvider.History());

                var historyHasChanged = itemHistory.IsDifferentFrom(job.state.PreviousHistory);
                                        ;
                Debug.WriteLine("history has changed: "+ historyHasChanged);

                if (!historyHasChanged) {
                    Thread.Sleep(100);
                } else {
                    var historyRecord = itemHistory.history.First();
                    EnforceValidPreviousActionResult(historyRecord);
                    
                    ChangeSinkFromLastAction(historyRecord);
                    job.state.Step = State.JobStep.STANDARD;
                    job.state.PreviousHistory = itemHistory;
                    job.changeTimeout.Stop();
                }
            }

            private void CalculateSinkChange() {
                EnforceChangeTimeoutRunningAndNotFinished();
                HistoryChangedChecksCount++;
                
                // Todo: continue with standard job (calculate sink change async) then wait before AI resolving action for calculation to complete
                var itemLatestHistory = job.history.Analyse(job.dataProvider.LatestHistory(), false);
                var latestChange = itemLatestHistory.history.FirstOrDefault();

                if (latestChange == null) {
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
                job.state.Step = State.JobStep.CALCULATING_PRICE_CHANGE;
                job.state.PreviousHistory = itemLatestHistory;
                HistoryChangedChecksCount = 0;
            }
            
            private void CalculatePriceChange() {
                Task.Run(() => {
                    var balance = job.dataProvider.AverageItemBalance();
                    if (balance == null) return;

                    job.Balance = (int) balance;
                });
                job.state.Step = State.JobStep.STANDARD;
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
                var attempted = lastHistoryRecord.Landed;
                if (!attempted.HasValue) {
                    return;
                }
                var statLanded = lastHistoryRecord.Landed?.stat;

                var previousCombine = (CombineRune) job.state.PreviousAction!;
                var expectedStat = previousCombine.Rune.Stat;

                if (statLanded != null && statLanded != expectedStat)
                    throw new UnexpectedMageResultException(
                        $"Expected \"{expectedStat.DisplayName}\" to land, not \"{statLanded.DisplayName}\"! " +
                        $"Have you run out of \"{expectedStat.DisplayName}\" runes?");
            }

            private void EnforceDifferentHistory(ItemHistoryAnalysis itemHistory) {
                if (!itemHistory.SuitableForCompare)
                    return;
                if (job.state.PreviousHistory != null && !itemHistory.IsDifferentFrom(job.state.PreviousHistory))
                    throw new HistoryHasntChangedException(itemHistory, job.state.PreviousHistory);
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
                } catch (CouldNotResolveSinkException) {

                    var previousCombine = (CombineRune) job.state.PreviousAction!;
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
                
                EnforceSameItemAsPreviousTick(item);
                EnforceStatsChanged(item);
                job.state.PreviousItem = item;
                job.configManager.EnforceConfigSetForItem(item);
                job.configManager.RemoveFallenUnconfiguredStats(item);

                previousTickDeferredExecutionTask?.Wait();
                var action = job.magus.ResolveAction(item);

                if (action is CombineRune combine) {
                    
                    EnforceHasRunesForCombine(combine);

                    if (combine.Exo) {
                        job.state.PreviousHistory = job.history.Analyse(job.dataProvider.History());
                    }
                    RaiseEventIfMagingItemWithHighSinkExo(item);
                }

                
                job.actions.Execute(action);
                Debug.WriteLine("executed action "+action);

                return action;
            }

            private void RaiseEventIfMagingItemWithHighSinkExo(Item item) {
                if (item.Stats.ExoStats.Any(exoStat => exoStat.Value > 0 && exoStat.Stat.Config.HighSinkStat)) {
                    job.SensitiveMage?.Invoke(
                        this, 
                        new MagingJobStartedEventArgs(false, item, job.configManager.Config!));
                    if (!job.IsMaging)
                        throw new OperationCanceledException();
                }
            }

            private void EnforceSameItemAsPreviousTick(Item item) {
                if (job.state.PreviousItem != null && !item.MatchesStandardStatsStructure(job.state.PreviousItem))
                    throw new ItemHasChangedException(item);
            }

            private void EnforceHasRunesForCombine(CombineRune combine) {
                var hasRune = job
                    .itemInfo
                    .Runes.TryGetValue(combine.Rune.Stat, out var userRunes);
                if (!hasRune)
                    return;
                
                var userRune = userRunes.First(userRune => userRune.Rune == combine.Rune);
                
                if (userRune.Quantity == 0 && job.state.PreviousCheckHadRunOutOfRunes == userRune.Rune && job.configManager.UserSettings.EnableRuneChecking)
                    throw new OutOfRunesException(userRune.Rune);
                job.state.PreviousCheckHadRunOutOfRunes =
                    userRune.Quantity == 0 ? userRune.Rune : null;
            }

            private void EnforceStatsChanged(Item item) {
                if (job.state.PreviousItem != null) {
                    var lastHistoryRecord = job.state.PreviousHistory?.history.First();
                    
                    var shouldBeDifferent = lastHistoryRecord?.Changed.Any() ?? true;
                    if (shouldBeDifferent) {
                        MarkTickAsShouldBeDifferent(item);
                    } else {
                        MarkTickAsShouldNotBeDifferent(item, lastHistoryRecord!);
                    }
                    
                    if (shouldveBeenDifferentCount >= MaxStatsShouldHaveChangedChecks)
                        throw new ItemHasNotChangedException(item);
                }
            }

            private void MarkTickAsShouldBeDifferent(Item item) {
                var areDifferent = item.HasDifferentStatValues(job.state.PreviousItem!);
                if (!areDifferent)
                    shouldveBeenDifferentCount++;
                else
                    shouldveBeenDifferentCount = 0;
            }

            private void MarkTickAsShouldNotBeDifferent(Item item, MageHistoryRecord lastHistoryRecord) {
                var areDifferent = item.HasDifferentStatValues(job.state.PreviousItem!);
                if (areDifferent)
                    shouldveBeenDifferentCount++;
                else
                    shouldveBeenDifferentCount = 0;
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
