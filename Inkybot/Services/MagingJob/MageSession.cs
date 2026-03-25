using System;
using System.Diagnostics;
using System.Linq;
using Inkybot.Actions;
using Inkybot.Dofus;
using Inkybot.Dofus.Domain;
using Inkybot.Domain;

namespace Inkybot.Services
{
    internal class MageSession
    {
        // Run identifier
        public readonly string RunId = Guid.NewGuid().ToString("N").Substring(0, 16);
        public readonly Stopwatch RunDuration = Stopwatch.StartNew();

        // Lifecycle
        public bool IsPreparing { get; set; }
        public bool IsRestarting { get; set; }
        public bool IsMaging { get; set; }
        public bool ManuallyStopped { get; set; }
        public bool HasStartedFired { get; set; }

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

        public bool ShouldResetMinMaxScan() {
            var previousWasExo = (PreviousAction as CombineRune)?.Exo ?? false;
            var itemHasActiveExo = (PreviousItem?.HasExo ?? false)
                && PreviousItem.Stats.ExoStats.Any(stat => stat.Value > 0);
            return previousWasExo || itemHasActiveExo;
        }

        public void ResetForNewItem() {
            PreviousCombineWasExoAttempt = false;
            Sink = 0m;
            Balance = 0;
            PreviousAction = null;
            PreviousHistory = null;
            PreviousItem = null;
            // PreviousCheckHadRunOutOfRunes is intentionally preserved across items
        }
    }
}
