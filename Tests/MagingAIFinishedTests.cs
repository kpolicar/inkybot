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
                null,
                0);
            Config.ChangeStatConfig(Stat.Initiative, exoIniConfig);
            action = AI.ResolveAction(item) as CombineRune;
            Assert.Null(action);

            Job.Sink = 7;
        }

        [Test]
        public void TestTimeForApRune() {
            item = new Item(new ItemStatRepository(new[] {
                new ItemStat("vitality", 367, 301, 400),
                new ItemStat("intelligence", 69, 51, 70),
                new ItemStat("wisdom", 31, 31, 40),
                new ItemStat("critical", 2, 2, 2),
                new ItemStat("fire_damage", 11, 9, 12),
                new ItemStat("heals", 10, 9, 12),
                new ItemStat("prospecting", 11, 11, 15),
                new ItemStat("per_earth_resistance", 7, 5, 7),
                new ItemStat("per_water_resistance", 7, 5, 7),
                new ItemStat("ap_parry", 7, 7, 10),
                new ItemStat("critical_resistance", -30, -30, -30),
            }));
            Config.ResetConfig(item);
            Config.ChangeStatConfigTarget(Stat.Vitality, 399);
            Config.ChangeStatConfigTargetMinimum(Stat.Vitality, 370);
            Config.ChangeStatConfigTarget(Stat.Intelligence, 69);
            Config.ChangeStatConfigTarget(Stat.Wisdom, 31);
            Config.ChangeStatConfigTarget(Stat.FireDamage, 11);
            Config.ChangeStatConfigTarget(Stat.Heals, 10);
            Config.ChangeStatConfigTarget(Stat.Prospecting, 11);
            Config.ChangeStatConfigTarget(Stat.ApParry, 7);
            var exoApConfig = MageConfig.ItemStatMageConfig.MakeExo(
                Stat.Ap, 
                1, 
                1,
                0);
            Config.ChangeStatConfig(Stat.Ap, exoApConfig);
            
            Job.Sink = 7;
            
            var action = AI.ResolveAction(item) as CombineRune;
            Assert.NotNull(action);
        }

    }
}
