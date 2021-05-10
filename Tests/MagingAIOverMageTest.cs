using System;
using Inkybot.Actions;
using Inkybot.Dofus;
using Inkybot.Dofus.Repositories;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class MagingAIOverMageTest : Design.MagingAITest
    {
        public Item item;

        [SetUp]
        public void InitSetupItem() {
            item = new Item(new ItemStatRepository(new[] {
                new ItemStat("vitality", 370, 351, 400),
                new ItemStat("wisdom", 39, 31, 40),
                new ItemStat("critical", 6, 4, 6),
                new ItemStat("ap", 1, 1, 1),
                new ItemStat("per_neutral_resistance", 10, 7, 10),
                new ItemStat("per_earth_resistance", 13, 7, 10),
            }));
            Config.ResetConfig(item);
        }

        [Test]
        public void TestOverTarget() {
            Job.Sink = 32;
            Config.ChangeStatConfigTarget(Stat.PerEarthResistance, 14);
            Config.ChangeStatConfigTargetMinimum(Stat.PerEarthResistance, 11);
            
            var action = AI.ResolveAction(item) as CombineRune;
            Assert.AreEqual(Stat.PerEarthResistance, action?.Rune.Stat);
        }

        [Test]
        public void TestOverTargetStopIfReachedMinimum() {
            item = new Item(new ItemStatRepository(new[] {
                new ItemStat("vitality", 370, 351, 400),
                new ItemStat("wisdom", 39, 31, 40),
                new ItemStat("critical", 6, 4, 6),
                new ItemStat("ap", 1, 1, 1),
                new ItemStat("per_neutral_resistance", 10, 7, 10),
                new ItemStat("per_earth_resistance", 13, 7, 10),
            }));
            Config.ResetConfig(item);
            
            Job.Sink = 5;
            Config.ChangeStatConfigTarget(Stat.PerEarthResistance, 14);
            Config.ChangeStatConfigTargetMinimum(Stat.PerEarthResistance, 11);
            
            var action = AI.ResolveAction(item) as CombineRune;
            Assert.AreEqual(null, action?.Rune.Stat);
        }

        [Test]
        public void TestExoTargetWithSink() {
            Job.Sink = 32;
            
            var exoAirPerResConfig = MageConfig.ItemStatMageConfig.MakeExo(
                Stat.PerAirResistance, 
                4, 
                2);
            Config.ChangeStatConfig(Stat.PerAirResistance, exoAirPerResConfig);
            
            var action2 = AI.ResolveAction(item) as CombineRune;
            Assert.AreEqual(Stat.PerAirResistance, action2?.Rune.Stat);
        }

    }
}
