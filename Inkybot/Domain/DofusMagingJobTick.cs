using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Timers;
using Inkybot.Actions;
using Inkybot.Exceptions;

namespace Inkybot
{
    internal class DofusMagingJobTick
    {
        private readonly DofusMagingJob job;

        public DofusMagingJobTick(DofusMagingJob job) {
            this.job = job;
        }

        public void Execute() {
            job.dataProvider.FetchData();
            switch (job.state) {
                case DofusMagingJobState.DOING_FIRST_COMBINE:
                    DoInitialMageAction();
                    break;
                case DofusMagingJobState.STANDARD:
                    DoMainMageAction();
                    break;
                case DofusMagingJobState.EXECUTING_COMBINE:
                    DoHistoryCheckForChanges();
                    break;
            }
        }

        private void DoInitialMageAction() {
            var action = job.previousAction = DoAction();

            if (action is Combine) job.state = DofusMagingJobState.STANDARD;
            Thread.Sleep(300);
        }

        private void DoMainMageAction() {
            var itemHistory = job.history.Analyse(job.dataProvider.History());
            job.previousHistory = itemHistory;

            var action = job.previousAction = DoAction();

            if (action is Combine) job.state = DofusMagingJobState.EXECUTING_COMBINE;
            Thread.Sleep(300);
        }

        private void DoHistoryCheckForChanges() {
            if (!job.historyCheckTimeout.Enabled) job.historyCheckTimeout.Start();
            
            var itemHistory = job.history.Analyse(job.dataProvider.History());

            var historyHasChanged = itemHistory.IsDifferentFrom(job.previousHistory);

            if (!historyHasChanged) {
                Thread.Sleep(50);
            } else {
                var historyRecord = itemHistory.history.Last();
                ChangeSinkFromLastAction(historyRecord);
                EnforceValidPreviousActionResult(historyRecord);
                job.state = DofusMagingJobState.STANDARD;
                job.historyCheckTimeout.Stop();
            }
        }

        // Todo: We can also check if the expected result is correct by comparing sink change.
        private void EnforceValidPreviousActionResult(MageHistoryRecord lastHistoryRecord) {
            var statLanded = lastHistoryRecord.attempted?.stat;
            if (statLanded == null) return;
            
            var previousCombine = (Combine) job.previousAction;
            var expectedStat = previousCombine.target.stat;

            if (statLanded != expectedStat)
                throw new UnexpectedMageResultException(
                    $"Expected \"{expectedStat.DisplayName}\" to land, not \"{statLanded.DisplayName}\"! " +
                    $"Have you run out of \"{expectedStat.DisplayName}\" runes?");
        }


        private void ChangeSinkFromLastAction(MageHistoryRecord lastHistoryRecord) {
            try {
                job.Sink += lastHistoryRecord.ChangeInSink;
            } catch (CouldNotResolveSinkException e) {
                
                var previousCombine = (Combine) job.previousAction;
                job.Sink += lastHistoryRecord.ChangeInSinkFromFallen - previousCombine.target.Sink;
                
                Debug.WriteLine("Could not resolve history's change in sink, defaulting to applied rune!");
                Debug.WriteLine("sink change:" +
                                (lastHistoryRecord.ChangeInSinkFromFallen - previousCombine.target.Sink));
            }

            job.Sink = Math.Max(0f, job.Sink);
        }


        private IAction DoAction() {
            var itemStats = job.dataProvider.Stats();
            if (itemStats.Length <= 0)
                throw new NoItemToMageFoundException("Could not gather item stats from screen");
            
            var action = job.magus.ResolveAction(itemStats, job.previousAction);
            job.actions.Execute(action);
            return action;
        }
    }
}
