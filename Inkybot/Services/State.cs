using System.Collections.Generic;
using Inkybot.Actions;
using Inkybot.Dofus;
using Inkybot.Dofus.Domain;
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
            public bool IsRestarting { get; set; }
            public bool IsMaging { get; set; }
            public JobStep Step;
            public decimal Sink;
            public int Balance;
            public IAction? PreviousAction;
            public string[] PreviousHistory;
            public Item? PreviousItem;
            public bool PreviousCombineWasExoAttempt;
            public Rune? PreviousCheckHadRunOutOfRunes;

            public void Reset() {
                Step = JobStep.STANDARD;
                PreviousCombineWasExoAttempt = false;
                Sink = 0m;
                Balance = 0;
                PreviousAction = null;
                PreviousHistory = null;
                PreviousItem = null;
                PreviousCheckHadRunOutOfRunes = null;
                Tick.shouldveBeenDifferentCount = 0;
            }
        }
    }
}
