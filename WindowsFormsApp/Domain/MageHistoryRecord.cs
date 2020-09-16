using System.Collections.Generic;
using System.Linq;

namespace WindowsFormsApp
{
    public struct StatChanged
    {
        public Stat stat;
        public int value;

        public float SinkModifier => stat?.sinkValue * value ?? 0;

        public StatChanged(Stat stat, int value) {
            this.stat = stat;
            this.value = value;
        }
    }
    
    public class MageHistoryRecord
    {
        private IEnumerable<StatChanged> changed;
        private bool sinkChanged;

        public float ChangeInSink =>
            !sinkChanged ? 0f :
            fell.Sum(statChange => -statChange.SinkModifier)
            - landed.SinkModifier;

        public StatChanged landed =>
            changed.FirstOrDefault(change => change.value > 0);
        
        public StatChanged[] fell =>
            changed.Where(change => change.value < 0).ToArray();
        
        public MageHistoryRecord(IEnumerable<StatChanged> changed, bool sinkChanged) {
            this.changed = changed;
            this.sinkChanged = sinkChanged;
        }
    }
}