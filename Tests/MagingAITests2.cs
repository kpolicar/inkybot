using Inkybot.Actions;
using Inkybot.Dofus;
using Inkybot.Dofus.Repositories;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class MagingAITest2 : Design.MagingAITest
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
            }));
            Config.ResetConfig(item);
        }

        [Test]
        public void TestSmallerExoBeforeLargerOnlyIfSink() {
            var exoPerFireConfig = MageConfig.ItemStatMageConfig.MakeExo(
                Stat.PerFireResistance, 
                1, 
                0,
                0);
            Config.ChangeStatConfig(Stat.PerFireResistance, exoPerFireConfig);
            var exoMpConfig = MageConfig.ItemStatMageConfig.MakeExo(
                Stat.Mp, 
                1, 
                1,
                0);
            Config.ChangeStatConfig(Stat.Mp, exoMpConfig);
            Config.ChangeStatConfigTargetMinimum(Stat.Vitality, 370);
            
            var action = AI.ResolveAction(item) as CombineRune;
            Assert.NotNull(action);
            Assert.AreEqual(Stat.Mp, action.Rune.Stat);

            Job.Sink = 7;
            action = AI.ResolveAction(item) as CombineRune;
            Assert.NotNull(action);
            Assert.AreEqual(Stat.PerFireResistance, action.Rune.Stat);

            Job.Sink = 5;
            action = AI.ResolveAction(item) as CombineRune;
            Assert.NotNull(action);
            Assert.AreEqual(Stat.Mp, action.Rune.Stat);
        }

        [Test]
        public void TestVitalityOvermage() {
            Config.ChangeStatConfig(Stat.Vitality, Config.Config![Stat.Vitality]!.Value.Clone(450, 400, 0));
            
            var action = AI.ResolveAction(item) as CombineRune;
            Assert.AreEqual(Stat.Vitality, action.Rune.Stat);
        }

        [Test]
        public void TestPerResistanceExo() {
            var overPerNeutralRes = new MageConfig.ItemStatMageConfig(
                Stat.PerNeutralResistance, 7, 10, 12, 10, 0);
            Config.ChangeStatConfig(Stat.PerNeutralResistance, overPerNeutralRes);
            
            Job.Sink = 5;
            var action = AI.ResolveAction(item) as CombineRune;
            Assert.IsNull(action);
            
            Job.Sink = 6;
            action = AI.ResolveAction(item) as CombineRune;
            Assert.AreEqual(Stat.PerNeutralResistance, action.Rune.Stat);
            
            overPerNeutralRes = new MageConfig.ItemStatMageConfig(
                Stat.PerNeutralResistance, 7, 10, 12, 11, 0);
            Config.ChangeStatConfig(Stat.PerNeutralResistance, overPerNeutralRes);
            
            Job.Sink = 5;
            action = AI.ResolveAction(item) as CombineRune;
            Assert.AreEqual(Stat.PerNeutralResistance, action.Rune.Stat);
        }

    }
}
