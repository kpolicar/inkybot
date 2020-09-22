using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using WindowsFormsApp.Actions;
using WindowsFormsApp.Contracts;
using WindowsFormsApp.Events;
using WindowsFormsApp.Exceptions;

namespace WindowsFormsApp
{
    public class ItemHistoryAnalysis
    {
        public IEnumerable<MageHistoryRecord> history;
        private IItemHistoryAnalyzer analyzer;

        public ItemHistoryAnalysis(IEnumerable<MageHistoryRecord> history, IItemHistoryAnalyzer analyzer) {
            this.history = history;
            this.analyzer = analyzer;
        }

        public float CalculateSink() {
            return Math.Max(0f, history.Sum(record => analyzer.ResolveSinkChange(record)));
        }

        public bool IsDifferentFrom(ItemHistoryAnalysis analysis) {
            var comparison = history.Zip(analysis.history, (target, comparator) => new { Target = target, Comparator = comparator });
            
            return history.Count() != analysis.history.Count() ||
                comparison.Any(comparison => comparison.Target != comparison.Comparator);
        }
    }
    
    public class ItemHistoryAnalyzer : IItemHistoryAnalyzer
    {
        public ItemHistoryAnalysis Analyse(IEnumerable<MageHistoryRecord> history) {
            return new ItemHistoryAnalysis(history, this);
        }

        public float ResolveSinkChange(MageHistoryRecord record) {
            try {
                Debug.WriteLine("change in sink: "+record.ChangeInSink);
                return record.ChangeInSink;
            }
            catch (InvalidOperationException ex) {
                throw new CouldNotResolveSinkException();
            }
        }
    }
}