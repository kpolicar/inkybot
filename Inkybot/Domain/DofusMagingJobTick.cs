using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Timers;
using Inkybot.Actions;
using Inkybot.Contracts;
using Inkybot.Exceptions;

namespace Inkybot.Domain
{
    internal class DofusMagingJobTick
    {
        private readonly ActionFactory actions;
        private readonly DofusMagingJob job;

        public DofusMagingJobTick(DofusMagingJob job) {
            this.job = job;
            actions = (ActionFactory) Program.Services.GetService(typeof(ActionFactory));
        }

        public void Execute() {
            job.dataProvider.FetchData();

            switch (job.state) {
                case DofusMagingJobState.STANDARD:
                    DoMainMageAction();
                    break;
                case DofusMagingJobState.EXECUTING_COMBINE:
                    DoHistoryCheckForChanges();
                    break;
            }
        }

        private void DoMainMageAction() {
            var action = job.previousAction = DoAction();

            // Have to check if user has stopped maging during this sleep
            if (action is Combine combine) {
                job.state = DofusMagingJobState.EXECUTING_COMBINE;
                if (!combine.Exo)
                    PersistRuneOnTable(combine);
            }
        }

        private void PersistRuneOnTable(Combine action) {
            job.actions.Execute(actions.SelectRune(action.target));
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
                job.state = DofusMagingJobState.STANDARD;
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
            var expectedStat = previousCombine.target.stat;

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
                sink += lastHistoryRecord.ChangeInSinkFromFallen - previousCombine.target.Sink;
                
                Debug.WriteLine("Could not resolve history's change in sink, defaulting to applied rune!");
                Debug.WriteLine("sink change:" +
                                (lastHistoryRecord.ChangeInSinkFromFallen - previousCombine.target.Sink));
            }

            job.Sink = Math.Max(0f, sink);
        }


        private IAction DoAction() {
            var item = job.dataProvider.Item();
            if (item.IsInvalid)
                throw new NoItemToMageFoundException("Could not gather item stats from screen");

            var action = job.magus.ResolveAction(item, job.previousAction);
            
            if (action is Combine) {
                var itemHistory = job.history.Analyse(job.dataProvider.History());
                job.previousHistory = itemHistory;
            }
            
            job.actions.Execute(action);
            return action;
        }
    }
}
