using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Inkybot.Contracts;
using Inkybot.Domain;
using Inkybot.Exceptions;

namespace Inkybot
{
    public class ItemHistoryAnalysis
    {
        private readonly IItemHistoryAnalyzer analyzer;
        public IEnumerable<MageHistoryRecord> history;
        private bool fullHistory;
        public bool SuitableForCompare => fullHistory || history.Count() >= 3;

        public ItemHistoryAnalysis(IEnumerable<MageHistoryRecord> history, IItemHistoryAnalyzer analyzer, bool fullHistory=true) {
            this.history = history;
            this.analyzer = analyzer;
            this.fullHistory = fullHistory;
        }

        public float CalculateSink() {
            return Math.Max(0f, history.Sum(record => analyzer.ResolveSinkChange(record)));
        }

        public bool IsDifferentFrom(ItemHistoryAnalysis analysis) {
            var comparison = history.Zip(analysis.history,
                (target, comparator) => new {Target = target, Comparator = comparator});

            return history.Count() != analysis.history.Count() ||
                   comparison.Any(comparison => comparison.Target != comparison.Comparator);
        }

        public override string ToString() {
            return string.Join("\n", history.Select(mageRecord => mageRecord.ToString()));
        }
    }

    public class ItemHistoryAnalyzer : IItemHistoryAnalyzer
    {
        public ItemHistoryAnalysis Analyse(IEnumerable<MageHistoryRecord> history, bool fullHistory=true) {
            return new ItemHistoryAnalysis(history, this, fullHistory);
        }

        public float ResolveSinkChange(MageHistoryRecord record) {
            try {
                Debug.WriteLine("change in sink: " + record.ChangeInSink);
                return record.ChangeInSink;
            } catch (InvalidOperationException ex) {
                throw new CouldNotResolveSinkException("", ex);
            }
        }
    }
}
