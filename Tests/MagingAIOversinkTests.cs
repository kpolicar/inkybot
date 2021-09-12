using Inkybot.Actions;
using Inkybot.Dofus;
using Inkybot.Dofus.Repositories;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class MagingAIOversinkTests : Design.MagingAITest
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
                new ItemStat("per_earth_resistance", 11, 7, 10),
            }));
            Config.ResetUserSettings(item);
        }

        [Test]
        public void TestWillReduceStatToSatisfyOversink() {
            var exoMpConfig = MageConfig.ItemStatMageConfig.MakeExo(
                Stat.Ap, 
                1, 
                1,
                0);
            Config.ChangeStatConfig(Stat.Ap, exoMpConfig);
            
            var action = AI.ResolveAction(item) as CombineRune;
            TestContext.WriteLine(action);
            Assert.NotNull(action);
            Assert.AreNotEqual(Stat.Ap, action.Rune.Stat);
        }

    }
}
