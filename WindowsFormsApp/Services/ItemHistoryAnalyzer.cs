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
        private IEnumerable<MageHistoryRecord> history;
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
        private Combine previousCombine;
        
        protected Dictionary<MageHistoryRecord, Combine> magingActionHistory = new Dictionary<MageHistoryRecord, Combine>();
        
        private ActionHandler actionHandler;

        public ItemHistoryAnalyzer() {
            actionHandler = (ActionHandler) Program.Services.GetService(typeof(ActionHandler));
            actionHandler.ActionExecuted += onActionExecuted;
        }

        private void onActionExecuted(object sender, ActionExecutedEventArgs e) {
            if (!(e.action is Combine)) return;

            previousCombine = (Combine) e.action;
        }

        private void onReadHistory(MageHistoryRecord newRecord) {
            magingActionHistory[newRecord] = previousCombine;
            previousCombine = null;
        }

        public ItemHistoryAnalysis Analyse(IEnumerable<MageHistoryRecord> history) {
            return new ItemHistoryAnalysis(history, this);
        }

        public float ResolveSinkChange(MageHistoryRecord record) {
            try {
                return record.ChangeInSink;
            }
            catch (InvalidOperationException ex) {
                return FindSinkForRecordFromCombineHistory(record);
            }
        }

        private float FindSinkForRecordFromCombineHistory(MageHistoryRecord record) {
            try {
                return magingActionHistory[record].target.Sink;
            }
            catch (Exception ex) {
                throw new CouldNotResolveSinkException();
            }
        }
    }
}