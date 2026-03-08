using System;
using System.Collections.Generic;
using System.Diagnostics;
using Inkybot;
using System.Globalization;
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
using Enumerable = Inkybot.Helpers.Enumerable;

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

            private static bool hasDoneRuneCheck = false;

            public void Execute() {
#if DEBUG
                Profiler.Reset();
#endif
                var currentStep = job.state.Step;
                //job.magus.SetHistory(job.state.PreviousHistory ?? new List<MageHistoryRecord>());
                switch (job.state.Step) {
                    case State.JobStep.STANDARD:
                        DoMainMageAction();
                        break;
                    case State.JobStep.EXECUTING_COMBINE:
                        DoHistoryCheckForChanges();
                        break;
                    case State.JobStep.CALCULATING_SINK_CHANGE:
                        CalculateSinkChange();
                        break;
                    case State.JobStep.CALCULATING_PRICE_CHANGE:
                        CalculatePriceChange();
                        break;
                }

                var nextStep = job.state.Step;
                if (currentStep == State.JobStep.EXECUTING_COMBINE && nextStep != State.JobStep.EXECUTING_COMBINE) {
                    job.SuccessfulCombineTick?.Invoke(this, EventArgs.Empty);
                    job.unsuccessfulCombineTicks = 0;
                }
#if DEBUG
                Profiler.PrintSummary();
#endif
            }

            private void DoMainMageAction() {
                StatsChangedChecksCount++;

                try {
                    var action = job.state.PreviousAction = DoAction();

                    if (action is CombineRune combineRune) {
                        job.state.Step = State.JobStep.EXECUTING_COMBINE;
                        job.state.PreviousCombineWasExoAttempt = combineRune.Exo;
                        if (combineRune.Exo || combineRune.Rune.Stat.Config.HighSinkStat) {
                            job.dataProvider.ResetMinMaxScan();
                            Thread.Sleep(800);
                        }
                    }
                } catch (Exception exception) {
                    if (MaxStatsChangedChecks >= StatsChangedChecksCount)
                        throw;
                    Debug.WriteLine("trying attempt "+StatsChangedChecksCount+" to recover from error: "+exception.Message);
                    job.Warning?.Invoke(this, new MagingJobErrorEventArgs(exception, $"Attempt #{StatsChangedChecksCount} out of ${MaxStatsChangedChecks}"));
                    job.dataProvider.FetchData();

                    Thread.Sleep(500);
                    return;
                }

                StatsChangedChecksCount = 0;
                //Thread.Sleep(500);
            }

            private void DoRuneCheckForChanges() {
                Debug.WriteLine("checking for rune changes");
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
                hasDoneRuneCheck = true;

                var newUserRune = job.dataProvider.RuneQuantity(previousAction.Rune);
                var userRune = job
                    .itemInfo
                    .Runes.Find(previousAction.Rune);

                if (ReferenceEquals(null, userRune)) {
                    DoHistoryCheckForChanges();
                    return;
                }

                if (newUserRune.Quantity != userRune.Quantity) {
                    Debug.WriteLine("rune quantity changed: "+newUserRune.Quantity+", previous: "+userRune.Quantity);
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
                Debug.WriteLine("checking history for changes");
                previousTickDeferredExecutionTask?.Wait();
                EnforceChangeTimeoutRunningAndNotFinished();

                // Start history + sink + stats OCR in parallel on this scan
                job.dataProvider.Scan!.PrefetchForHistoryCheck();

                var itemHistory = job.dataProvider.History();

                var historyHasChanged = Enumerable.ZipWithDefault(itemHistory, job.state.PreviousHistory, (s, s1) => {
                    return s != s1;
                }).Any(b => b);
                Debug.WriteLine("history has changed: "+ historyHasChanged);

                if (!historyHasChanged) {
                    Debug.WriteLine("fetching history for changes");
                    job.dataProvider.FetchData();
                    Debug.WriteLine("fetched history for changes");
                } else {
                    var historyRecord = itemHistory;
                    //EnforceValidPreviousActionResult(historyRecord);
                    
                    //ChangeSinkFromLastAction(historyRecord);
                    job.state.Step = State.JobStep.CALCULATING_SINK_CHANGE;
                    job.state.PreviousHistory = itemHistory;
                    job.changeTimeout.Stop();
                }
            }

            private void CalculateSinkChange() {
                Debug.WriteLine("calculating sink change");
                EnforceChangeTimeoutRunningAndNotFinished();
                HistoryChangedChecksCount++;

                try {
                    var sink = job.dataProvider.Sink();
                    if (sink == null) {
                        job.dataProvider.FetchData();
                        sink = job.dataProvider.Sink();
                    }
                    Debug.WriteLine("CHANGED SINK TO "+(sink ?? 0).ToString(CultureInfo.InvariantCulture));
                    job.dSink = sink ?? 0;
                    job.state.Step = State.JobStep.CALCULATING_PRICE_CHANGE;
                } catch (Exception ex) {
                    Debug.WriteLine(ex.Message);
                    Debug.WriteLine(ex);
                    Debug.WriteLine(ex.StackTrace);
                }
                
                //
                // // Todo: continue with standard job (calculate sink change async) then wait before AI resolving action for calculation to complete
                // var itemLatestHistory = job.history.Analyse(job.dataProvider.LatestHistory(), false);
                // var latestChange = itemLatestHistory.history.FirstOrDefault();
                //
                // if (latestChange == null) {
                //     job.dataProvider.FetchData();
                //     return;
                // }
                //
                // job.changeTimeout.Stop();
                // try {
                //     EnforceValidPreviousActionResult(latestChange);
                //     EnforceDifferentHistory(itemLatestHistory);
                // } catch (UnexpectedMageResultException exception) {
                //     if (HistoryChangedChecksCount >= MaxHistoryChangedChecks)
                //         throw;
                //     
                //     job.Warning?.Invoke(this, new MagingJobErrorEventArgs(exception, $"Attempt #{HistoryChangedChecksCount} out of ${MaxHistoryChangedChecks}"));
                //     Thread.Sleep(50);
                //     job.dataProvider.FetchData();
                //     return;
                // }
                //
                // ChangeSinkFromLastAction(latestChange);
                // job.dataProvider.ApproveLatestHistoryContinueToNextScanBounds();
                // job.state.Step = State.JobStep.CALCULATING_PRICE_CHANGE;
                // job.state.PreviousHistory = itemLatestHistory;
                // HistoryChangedChecksCount = 0;
            }
            
            private void CalculatePriceChange() {
                 /*Task.Run(() => {
                     var balance = job.dataProvider.AverageItemBalance();
                     if (balance == null) return;
                
                     job.Balance = (int) balance;
                 });*/
                hasDoneRuneCheck = false; // reset
                job.state.Step = State.JobStep.STANDARD;
            }

            private void HandleChangeCheckTimeout() {
                job.changeTimeout.Stop();
                var additional = job.state.PreviousAction is RuneAction runeAction
                    ? "\""+runeAction.Rune.DisplayName+"\" "
                    : "";
                throw new ChangeCheckTimeoutException(
                    "Rune combination was expected to perform within 5 seconds, but did not. " +
                    $"This may be the result of a poor internet connection or you may have run out of {additional}runes.");
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

                if (!ReferenceEquals(statLanded, null) && statLanded != expectedStat)
                    throw new UnexpectedMageResultException(
                        $"Expected \"{expectedStat.DisplayName}\" to land, not \"{statLanded.DisplayName}\"! " +
                        $"Have you run out of \"{expectedStat.DisplayName}\" runes?");
            }

            private void EnforceDifferentHistory(ItemHistoryAnalysis itemHistory) {
                if (!itemHistory.SuitableForCompare)
                    return;
                //if (job.state.PreviousHistory != null && !itemHistory.IsDifferentFrom(job.state.PreviousHistory))
                //    throw new HistoryHasntChangedException(itemHistory, job.state.PreviousHistory);
            }


            private void ChangeSinkFromLastAction(MageHistoryRecord lastHistoryRecord) {
                decimal sink = job.Sink;
                decimal sinkChange;
                decimal attemptedSinkChange;
                
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

                if (sinkChange > 0
                    && -attemptedSinkChange <= sink
                    && lastHistoryRecord.Fell.Any(fallenStat => !(job.state.PreviousItem?.Stats[fallenStat.stat]?.Overmaged ?? false)))
                {
                    job.Warning?.Invoke(this, new MagingJobErrorEventArgs(new SinkIncorrectException(sinkChange, sink, attemptedSinkChange), "Something had to have gone wrong in sink calculation! Resetting sink!"));
                    sink = 0;
                } else {
                    sink += sinkChange;
                }

                if (sink < 0) {
                    job.Warning?.Invoke(this, new MagingJobErrorEventArgs(new SinkNegativeException(sink), ""));
                }
                if (sink > 101) {
                    job.Warning?.Invoke(this, new MagingJobErrorEventArgs(new SinkTooHighException(sink), ""));
                    sink = 0m;
                }

                job.dSink = Math.Max(0m, sink);
            }


            private IAction DoAction() {
                if (((job.state.PreviousAction as CombineRune)?.Exo ?? false) ||
                    ((job.state.PreviousItem?.HasExo ?? false) && !job.state.PreviousItem.Stats.ExoStats.All(stat => stat.Value < 0))) { // Refresh minmax if item has exo that isn't negative
                    job.dataProvider.ResetMinMaxScan();
                }
                
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
                    if (job.unsuccessfulCombineTicks >= 5) {
                        throw new OutOfRunesException(combine.Rune);
                    }
                
                    EnforceHasRunesForCombine(combine);

                    if (combine.Exo) {
                        job.state.PreviousHistory = job.dataProvider.History();
                        //job.state.PreviousHistory = job.history.Analyse(job.dataProvider.History());
                    }
                    RaiseEventIfMagingItemWithHighSinkExo(item);
                }

                
                job.actions.Execute(action);
                Debug.WriteLine("executed action "+action);

                return action;
            }

            private bool PreviousCombineWasExoThatLandedButIsNotVisibleOnItem(Item item, CombineRune currentAction, CombineRune previousAction) {
                return previousAction.Exo
                       && currentAction.Exo
                       && currentAction.Rune == previousAction.Rune
                       && (job.state.PreviousHistory.LastOrDefault()?.Contains(previousAction.Rune.Stat.DisplayName) ?? false) // the last raw line is an exo
                       && (!job.state.PreviousHistory.LastOrDefault()?.StartsWith("-") ?? false) // didn't fall, it landed
                       && ReferenceEquals(item.Stats[previousAction.Rune.Stat], null);
            }

            private bool IsFirstTick() => job.ticks == 1;

            private void RaiseEventIfMagingItemWithHighSinkExo(Item item) {
                var interrupted = false;
                var showSensitiveMageDialogue = IsFirstTick() && item.Stats.Any(itemStat => itemStat.Overmaged);
                
                if (!showSensitiveMageDialogue) {
                    showSensitiveMageDialogue =
                        item.Stats.ExoStats.Any(exoStat => exoStat.Value > 0 && exoStat.Stat.Config.HighSinkStat);
                    if (showSensitiveMageDialogue)
                        interrupted = true;
                }
                
                if (showSensitiveMageDialogue) {
                    job.SensitiveMage?.Invoke(
                        this, 
                        new MagingJobStartedEventArgs(false, item, job.configManager.Config!, interrupted));
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
                // if (job.state.PreviousItem != null) {
                //     var lastHistoryRecord = job.state.PreviousHistory?.history.FirstOrDefault();
                //     
                //     var shouldBeDifferent = lastHistoryRecord?.Changed.Any() ?? true;
                //     if (shouldBeDifferent) {
                //         MarkTickAsShouldBeDifferent(item);
                //     } else {
                //         MarkTickAsShouldNotBeDifferent(item, lastHistoryRecord!);
                //     }
                //     
                //     if (shouldveBeenDifferentCount >= MaxStatsShouldHaveChangedChecks)
                //         throw new ItemHasNotChangedException(item);
                // }
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
                
                if (job.changeTimeout.ElapsedMilliseconds > 15000 || (!job.state.PreviousCombineWasExoAttempt && job.changeTimeout.ElapsedMilliseconds > 5000))
                    HandleChangeCheckTimeout();
            }
        }
    }
}
