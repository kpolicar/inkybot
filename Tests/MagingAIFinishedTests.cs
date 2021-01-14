using Inkybot.Actions;
using Inkybot.Dofus;
using Inkybot.Dofus.Repositories;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class MagingAIFinishedTest : Design.MagingAITest
    {
        public Item item;

        [SetUp]
        public void InitSetupItem() {
            item = new Item(new ItemStatRepository(new[] {
                new ItemStat("vitality", 370, 301, 400),
                new ItemStat("strength", 93, 81, 100),
                new ItemStat("wisdom", 38, 31, 40),
                new ItemStat("critical", 5, 4, 5),
                new ItemStat("range", 1, 1, 1),
                new ItemStat("neutral_damage", 18, 16, 20),
                new ItemStat("earth_damage", 18, 16, 20),
                new ItemStat("per_neutral_resistance", 10, 7, 10),
                new ItemStat("per_earth_resistance", 10, 7, 10),
                new ItemStat("initiative", -10, 0, 0),
                new ItemStat("ap", 1, 0, 0),
            }));
            Config.ResetConfig(item);
        }

        [Test]
        public void TestSmallerExoBeforeLargerOnlyIfSink() {
            var action = AI.ResolveAction(item) as CombineRune;
            Assert.Null(action);
            
            // Initiative was a targeted exo stat, but it was not necessary to land
            var exoIniConfig = MageConfig.ItemStatMageConfig.MakeExo(
                Stat.Initiative, 
                0, 
                null);
            Config.ChangeStatConfig(Stat.Initiative, exoIniConfig);
            action = AI.ResolveAction(item) as CombineRune;
            Assert.Null(action);

            Job.Sink = 7;
        }

    }
}
