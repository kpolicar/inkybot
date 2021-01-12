using Inkybot.Actions;
using Inkybot.Dofus;
using Inkybot.Domain;

namespace Inkybot.Services
{
    public partial class ScreenReaderDofusMagingJob
    {
        private struct State
        {
            public enum JobStep
            {
                STANDARD,
                EXECUTING_COMBINE,
                CALCULATING_SINK_CHANGE,
                CALCULATING_PRICE_CHANGE
            }

            public bool IsPreparing { get; set; }
            public bool IsMaging { get; set; }
            public JobStep Step;
            public float Sink;
            public int Balance;
            public IAction? PreviousAction;
            public ItemHistoryAnalysis? PreviousHistory;
            public Item? PreviousItem;
            public bool PreviousCombineWasExoAttempt;
            public bool PreviousCheckHadRunOutOfRunes;

            public void Reset() {
                Step = JobStep.STANDARD;
                PreviousCombineWasExoAttempt = false;
                Sink = 0f;
                Balance = 0;
                PreviousAction = null;
                PreviousHistory = null;
                PreviousItem = null;
                PreviousCheckHadRunOutOfRunes = false;
            }
        }
    }
}
