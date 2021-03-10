using System;
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
            Assert.Null(action);
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
        public void TestExoSmallerBeforeLargerIfSink() {
            var exoIniConfig = MageConfig.ItemStatMageConfig.MakeExo(
                Stat.Initiative, 
                10, 
                0);
            Config.ChangeStatConfig(Stat.Initiative, exoIniConfig);
            var exoApConfig = MageConfig.ItemStatMageConfig.MakeExo(
                Stat.Ap, 
                1, 
                1);
            Config.ChangeStatConfig(Stat.Ap, exoApConfig);
            
            var action1 = AI.ResolveAction(item) as CombineRune;
            Assert.NotNull(action1);
            Assert.AreEqual(new Rune(Stat.Ap, Rune.RuneType.Sm), action1.Rune);
            
            Job.Sink = 1;
            var action2 = AI.ResolveAction(item) as CombineRune;
            Assert.NotNull(action2);
            Assert.AreEqual(new Rune(Stat.Initiative, Rune.RuneType.Sm), action2.Rune);
            
            
            item = new Item(new ItemStatRepository(new[] {
                new ItemStat("vitality", 288, 251, 300),
                new ItemStat("power", 48, 41, 50),
                new ItemStat("critical", 4, 3, 4),
                new ItemStat("range", 2, 2, 2),
                new ItemStat("neutral_damage", 10, 7, 10),
                new ItemStat("earth_damage", 10, 7, 10),
                new ItemStat("water_damage", 10, 7, 10),
                new ItemStat("per_neutral_resistance", 10, 7, 10),
            }));
            Config.ResetConfig(item);
            Config.ChangeStatConfig(Stat.Initiative, exoIniConfig);
            Config.ChangeStatConfig(Stat.Ap, exoApConfig);
            Job.Sink = 1;
            var action3 = AI.ResolveAction(item) as CombineRune;
            Assert.NotNull(action3);
            Assert.AreEqual(new Rune(Stat.Initiative, Rune.RuneType.Sm), action3.Rune);
        }

        [Test]
        public void WouldOvermageIfRemainingSink() {
            item = new Item(new ItemStatRepository(new[] {
                new ItemStat("vitality", 300, 251, 300),
                new ItemStat("wisdom", 24, 16, 25),
                new ItemStat("power", 39, 21, 40),
                new ItemStat("critical", 5, 3, 5),
                new ItemStat("ap", 1, 1, 1),
                new ItemStat("critical_resistance", 20, 11, 20),
                new ItemStat("pushback_resistance", 20, 11, 20),
            }));
            Config.ResetConfig(item);
            
            var exoMpConfig = MageConfig.ItemStatMageConfig.MakeExo(
                Stat.Mp, 
                1, 
                1);
            Config.ChangeStatConfig(Stat.Mp, exoMpConfig);
            Job.Sink = 10;
            var action = AI.ResolveAction(item) as CombineRune;
            Assert.NotNull(action);
            Assert.AreEqual(new Rune(Stat.Vitality, Rune.RuneType.Pa), action.Rune);
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

        [Test]
        public void TestOversinkOver101Cap() {
            item = new Item(new ItemStatRepository(new[] {
                new ItemStat("vitality", 390, 301, 400),
                new ItemStat("strength", 93, 81, 100),
                new ItemStat("wisdom", 38, 31, 50),
                new ItemStat("critical", 5, 4, 5),
            }));
            Config.ResetConfig(item);
            
            var action = AI.ResolveAction(item) as CombineRune;
            Assert.NotNull(action);
            Assert.AreEqual(Stat.Wisdom, action.Rune.Stat);
        }

        [Test]
        public void TestOversinkOver101CapRange() {
            item = new Item(new ItemStatRepository(new[] {
                new ItemStat("vitality", 294, 251, 300),
                new ItemStat("wisdom", 49, 41, 50),
                new ItemStat("critical", 4, 3, 4),
                new ItemStat("range", 1, 2, 2),
                new ItemStat("neutral_damage", 10, 7, 10),
                new ItemStat("earth_damage", 10, 7, 10),
                new ItemStat("air_damage", 10, 7, 10),
                new ItemStat("per_neutral_resistance", 10, 7, 10),
            }));
            Config.ResetConfig(item);
            var exoApConfig = MageConfig.ItemStatMageConfig.MakeExo(
                Stat.Ap, 
                1, 
                1);
            Config.ChangeStatConfig(Stat.Ap, exoApConfig);
            
            var action = AI.ResolveAction(item) as CombineRune;
            Assert.NotNull(action);
            Assert.AreEqual(Stat.Range, action.Rune.Stat);
        }
    }
}
