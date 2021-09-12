using Inkybot.Actions;
using Inkybot.Dofus;
using Inkybot.Dofus.Repositories;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class MagingAIRangeExoTest : Design.MagingAITest
    {
        public Item item;

        [SetUp]
        public void InitSetupItem() {
            item = new Item(new ItemStatRepository(new[] {
                new ItemStat("vitality", 380, 301, 400),
                new ItemStat("strength", 93, 81, 100),
                new ItemStat("wisdom", 38, 31, 40),
                new ItemStat("critical", 5, 4, 5),
                new ItemStat("range", 1, 1, 1),
                new ItemStat("neutral_damage", 18, 16, 20),
                new ItemStat("earth_damage", 18, 16, 20),
                new ItemStat("per_neutral_resistance", 10, 7, 10),
                new ItemStat("per_earth_resistance", 10, 7, 10),
                new ItemStat("mp", 1, 1, 1),
                new ItemStat("per_air_resistance", 1, 0, 0),
            }));
            Config.ResetUserSettings(item);
        }

        [Test]
        public void TestSmallerExoBeforeLargerOnlyIfSink() {
            var exoAirRes = MageConfig.ItemStatMageConfig.MakeExo(
                Stat.PerAirResistance, 
                4, 
                1,
                0);
            Config.ChangeStatConfig(Stat.PerAirResistance, exoAirRes);
            var exoRange = MageConfig.ItemStatMageConfig.MakeExo(
                Stat.Range, 
                1, 
                1,
                0);
            Config.ChangeStatConfig(Stat.Range, exoRange);
            Job.Sink = 60;
            
            var action = AI.ResolveAction(item) as CombineRune;
            Assert.AreEqual(new Rune(Stat.PerAirResistance, Rune.RuneType.Sm), action?.Rune);
        }
    }
}
