using System.Diagnostics;
using Inkybot.Dofus;
using Inkybot.Dofus.Domain;
using Inkybot.Domain;

namespace Inkybot.Services
{
    internal class MageSession
    {
        // Lifecycle
        public bool IsPreparing { get; set; }
        public bool IsRestarting { get; set; }
        public bool IsMaging { get; set; }

        // Tracked values
        public decimal Sink;
        public int Balance;

        // Per-tick tracking
        public IAction PreviousAction;
        public string[] PreviousHistory;
        public Item PreviousItem;
        public bool PreviousCombineWasExoAttempt;
        public Rune PreviousCheckHadRunOutOfRunes;

        // Counters
        public int Ticks;
        public int UnsuccessfulCombineTicks;
        public Stopwatch ChangeTimeout = new Stopwatch();

        // Item info
        public UserRunes Runes;

        public void Reset() {
            PreviousCombineWasExoAttempt = false;
            Sink = 0m;
            Balance = 0;
            PreviousAction = null;
            PreviousHistory = null;
            PreviousItem = null;
            PreviousCheckHadRunOutOfRunes = null;
        }
    }
}
