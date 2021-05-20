using System;
using Inkybot.Actions;
using Inkybot.Dofus;
using Inkybot.Dofus.Repositories;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class MagingAIPreserveSinkForOvermage : Design.MagingAITest
    {
        public Item item;

        [SetUp]
        public void InitSetupItem() {
            item = new Item(new ItemStatRepository(new[] {
                new ItemStat("vitality", 380, 351, 400),
                new ItemStat("wisdom", 39, 31, 40),
                new ItemStat("critical", 6, 4, 6),
                new ItemStat("ap", 1, 1, 1),
                new ItemStat("per_neutral_resistance", 4, 7, 10),
                new ItemStat("per_earth_resistance", 10, 7, 10),
            }));
            Config.ResetConfig(item);
        }

        [Test]
        public void TestKeepPerfectingDontPreserveSinkYet() {
            Job.Sink = 30; // more than x2 what is needed to reach target minimum
            Config.ChangeStatConfigTarget(Stat.PerNeutralResistance, 4);
            Config.ChangeStatConfigTarget(Stat.PerEarthResistance, 14);
            Config.ChangeStatConfigTargetMinimum(Stat.PerEarthResistance, 12);
            
            var action = AI.ResolveAction(item) as CombineRune;
            Assert.AreEqual(Stat.PerNeutralResistance, action?.Rune.Stat);
        }

        [Test]
        public void TestPreserveSinkBeginOvermage() {
            Job.Sink = 11; // less than x2 what is needed to reach target minimum
            Config.ChangeStatConfigTarget(Stat.PerNeutralResistance, 4);
            Config.ChangeStatConfigTarget(Stat.PerEarthResistance, 14);
            Config.ChangeStatConfigTargetMinimum(Stat.PerEarthResistance, 12);
            
            var action = AI.ResolveAction(item) as CombineRune;
            Assert.AreEqual(Stat.PerEarthResistance, action?.Rune.Stat);
        }

        [Test]
        public void TestDontPreserveSinkBeginOvermage() {
            item = new Item(new ItemStatRepository(new[] {
                new ItemStat("vitality", 380, 351, 400),
                new ItemStat("wisdom", 39, 31, 40),
                new ItemStat("critical", 6, 4, 6),
                new ItemStat("ap", 1, 1, 1),
                new ItemStat("per_neutral_resistance", 4, 7, 10),
                new ItemStat("per_earth_resistance", 10, 7, 10),
                new ItemStat("earth_resistance", 1, 1, 2),
            }));
            Config.ResetConfig(item);
            Job.Sink = 14; // we need 12 sink
            Config.ChangeStatConfigTarget(Stat.PerNeutralResistance, 4);
            Config.ChangeStatConfigTarget(Stat.PerEarthResistance, 14);
            Config.ChangeStatConfigTargetMinimum(Stat.PerEarthResistance, 12);
            
            var action = AI.ResolveAction(item) as CombineRune;
            Assert.AreEqual(Stat.EarthResistance, action?.Rune.Stat);
        }

    }
}
