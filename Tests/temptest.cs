using System;
using Inkybot.Actions;
using Inkybot.Dofus;
using Inkybot.Dofus.Repositories;
using NUnit.Framework;

namespace Tests
{
    // this test is meant for testing whether or not the ResolveExcludingStats(Stat.Ap) works
    [TestFixture]
    public class TempTest : Design.MagingAITest
    {
        public Item item;

        [SetUp]
        public void InitSetupItem() {
            item = new Item(new ItemStatRepository(new[] {
                new ItemStat("vitality", 390, 351, 400),
                new ItemStat("wisdom", 39, 31, 40),
                new ItemStat("critical", 6, 4, 6),
                new ItemStat("ap", 0, 1, 1),
                new ItemStat("per_neutral_resistance", 10, 7, 10),
                new ItemStat("per_earth_resistance", 10, 7, 10),
            }));
            Config.ResetUserSettings(item);
        }

        [Test]
        public void TestOverTarget() {
            var exoPerRes = MageConfig.ItemStatMageConfig.MakeExo(
                Stat.PerAirResistance, 
                1, 
                1,
                0);
            Config.ChangeStatConfig(Stat.PerAirResistance, exoPerRes);
            Job.Sink = 32;
            
            var action = AI.ResolveAction(item) as CombineRune;
            //Assert.AreEqual(Stat.PerAirResistance, action?.Rune.Stat);
        }
    }
}
