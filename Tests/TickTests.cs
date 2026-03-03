using Inkybot.Services;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class TickTests
    {
        [Test]
        public void HistoryChanged_WhenBothNull_ReturnsFalse() {
            Assert.IsFalse(Inkybot.Helpers.History.HasChanged(null, null));
        }

        [Test]
        public void HistoryChanged_WhenBothEmpty_ReturnsFalse() {
            Assert.IsFalse(Inkybot.Helpers.History.HasChanged(
                new string[0], new string[0]));
        }

        [Test]
        public void HistoryChanged_WhenSameEntries_ReturnsFalse() {
            var history = new[] { "Agi +1", "Fo +3", "Agi -1" };
            Assert.IsFalse(Inkybot.Helpers.History.HasChanged(history, history));
        }

        [Test]
        public void HistoryChanged_WhenIdenticalContent_ReturnsFalse() {
            var current = new[] { "Agi +1", "Fo +3" };
            var previous = new[] { "Agi +1", "Fo +3" };
            Assert.IsFalse(Inkybot.Helpers.History.HasChanged(current, previous));
        }

        [Test]
        public void HistoryChanged_WhenDifferentContent_ReturnsTrue() {
            var current = new[] { "Agi +1", "Fo +3" };
            var previous = new[] { "Agi +1", "Cha +2" };
            Assert.IsTrue(Inkybot.Helpers.History.HasChanged(current, previous));
        }

        [Test]
        public void HistoryChanged_WhenNewEntryAdded_ReturnsTrue() {
            var current = new[] { "Agi +1", "Fo +3", "Cha +2" };
            var previous = new[] { "Agi +1", "Fo +3" };
            Assert.IsTrue(Inkybot.Helpers.History.HasChanged(current, previous));
        }

        [Test]
        public void HistoryChanged_WhenEntryRemoved_ReturnsTrue() {
            var current = new[] { "Agi +1" };
            var previous = new[] { "Agi +1", "Fo +3" };
            Assert.IsTrue(Inkybot.Helpers.History.HasChanged(current, previous));
        }

        [Test]
        public void HistoryChanged_WhenPreviousNull_CurrentHasEntries_ReturnsTrue() {
            var current = new[] { "Agi +1" };
            Assert.IsTrue(Inkybot.Helpers.History.HasChanged(current, null));
        }

        [Test]
        public void HistoryChanged_WhenPreviousNull_CurrentEmpty_ReturnsFalse() {
            Assert.IsFalse(Inkybot.Helpers.History.HasChanged(new string[0], null));
        }

        [Test]
        public void HistoryChanged_WhenCurrentNull_PreviousHasEntries_ReturnsTrue() {
            var previous = new[] { "Agi +1" };
            // null current vs non-null previous — ZipWithDefault will find differences
            Assert.IsTrue(Inkybot.Helpers.History.HasChanged(null, previous));
        }

        [Test]
        public void HistoryChanged_WhenFirstEntryDiffers_ReturnsTrue() {
            var current = new[] { "Fo +3", "Agi +1" };
            var previous = new[] { "Cha +2", "Agi +1" };
            Assert.IsTrue(Inkybot.Helpers.History.HasChanged(current, previous));
        }
    }
}
