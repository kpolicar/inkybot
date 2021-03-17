using System.Linq;
using Inkybot.Adapters;
using Inkybot.Dofus;
using NUnit.Framework;

namespace Tests
{
    public class DofusHistoryOcrResultTests
    {
        [Test]
        public void TestFailure() {
            var historyLine = "Failure";
            var adapter = new DofusHistoryOcrResultAdapter(new[] {historyLine});
            
            var historyLine2 = "-sink";
            var adapter2 = new DofusHistoryOcrResultAdapter(new[] {historyLine2});
            
            var historyRecord = adapter.ToMageHistoryRecords().First();
            var historyRecord2 = adapter2.ToMageHistoryRecords().First();
            
            Assert.AreEqual(historyRecord, MageHistoryRecord.Failure);
            Assert.True(historyRecord == MageHistoryRecord.Failure);
            Assert.AreNotEqual(historyRecord2, MageHistoryRecord.Failure);
            Assert.True(historyRecord2 != MageHistoryRecord.Failure);
        }
    }
}
