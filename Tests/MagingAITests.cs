using Inkybot.Actions;
using Inkybot.Dofus;
using Inkybot.Dofus.Repositories;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class MagingAITest : Design.MagingAITest
    {
        public Item item;

        [SetUp]
        public void InitSetupItem() {
            item = new Item(new ItemStatRepository(new[] {
                new ItemStat("vitality", 351, 301, 400),
                new ItemStat("strength", 93, 81, 100),
                new ItemStat("wisdom", 38, 31, 40),
                new ItemStat("critical", 5, 4, 5),
                new ItemStat("range", 1, 1, 1),
                new ItemStat("neutral_damage", 18, 16, 20),
                new ItemStat("earth_damage", 18, 16, 20),
                new ItemStat("per_neutral_resistance", 10, 7, 10),
                new ItemStat("per_earth_resistance", 10, 7, 10),
            }));
            Config.ResetConfig(item);
        }

        [Test]
        public void TestOvermageToReachMinimumVitality() {
            Config.ChangeStatConfigTargetMinimum(Stat.Vitality, 370);
            var action = AI.ResolveAction(item) as CombineRune;
            Assert.NotNull(action);
            Assert.AreEqual(action.Rune.Stat, Stat.Vitality);
        }

        // Todo he should not be trying to do this - it will fail (strength cannot go over 101)
        [Test]
        public void TestOvermageToReachMinimumStrength() {
            Config.ChangeStatConfigTargetMinimum(Stat.Strength, 94);
            var action = AI.ResolveAction(item) as CombineRune;
            Assert.NotNull(action);
            Assert.AreEqual(action.Rune.Stat, Stat.Strength);
        }

        [Test]
        public void TestExoSmallerBeforeLarger() {
            var exoIniConfig = MageConfig.ItemStatMageConfig.MakeExo(
                Stat.Initiative, 
                10, 
                10);
            Config.ChangeStatConfig(Stat.Initiative, exoIniConfig);
            var exoApConfig = MageConfig.ItemStatMageConfig.MakeExo(
                Stat.Ap, 
                1, 
                1);
            Config.ChangeStatConfig(Stat.Ap, exoApConfig);
            
            var action = AI.ResolveAction(item) as CombineRune;
            Assert.NotNull(action);
            Assert.AreEqual(new Rune(Stat.Initiative, Rune.RuneType.Sm), action.Rune);
        }

        [Test]
        public void TestExoFocusStatWithTargetMinimum() {
            var exoIniConfig = MageConfig.ItemStatMageConfig.MakeExo(
                Stat.Intelligence, 
                10, 
                null);
            Config.ChangeStatConfig(Stat.Intelligence, exoIniConfig);
            var exoApConfig = MageConfig.ItemStatMageConfig.MakeExo(
                Stat.Ap, 
                1, 
                1);
            Config.ChangeStatConfig(Stat.Ap, exoApConfig);
            
            var action = AI.ResolveAction(item) as CombineRune;
            Assert.NotNull(action);
            Assert.AreEqual(new Rune(Stat.Ap, Rune.RuneType.Sm), action.Rune);
        }
    }
}
