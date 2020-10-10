using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using WindowsFormsApp.Actions;

namespace WindowsFormsApp
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
            var action = DoAction();

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
            var itemHistory = job.history.Analyse(job.dataProvider.History());

            var historyHasChanged = itemHistory.IsDifferentFrom(job.previousHistory);

            if (!historyHasChanged) {
                Thread.Sleep(50);
            } else {
                ChangeSinkFromLastAction(itemHistory);
                job.state = DofusMagingJobState.STANDARD;
            }
        }


        private void ChangeSinkFromLastAction(ItemHistoryAnalysis itemHistory) {
            try {
                job.Sink += itemHistory.history.Last().ChangeInSink;
            } catch (Exception e) {
                var previousCombine = (Combine) job.previousAction;
                job.Sink += itemHistory.history.Last().fell.Sum(statChange => -statChange.SinkModifier) -
                            previousCombine.target.Sink;
                Debug.WriteLine("Could not resolve history's change in sink, defaulting to applied rune!");
                Debug.WriteLine("sink change:" +
                                (itemHistory.history.Last().fell.Sum(statChange => -statChange.SinkModifier) -
                                 previousCombine.target.Sink));
            }

            job.Sink = Math.Max(0f, job.Sink);
        }


        private IAction DoAction() {
            var itemStats = job.dataProvider.Stats();
            var action = job.magus.ResolveAction(itemStats);
            job.actions.Execute(action);
            return action;
        }
    }
}
