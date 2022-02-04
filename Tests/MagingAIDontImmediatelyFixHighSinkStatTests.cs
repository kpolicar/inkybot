using System;
using Inkybot.Actions;
using Inkybot.Dofus;
using Inkybot.Dofus.Repositories;
using NUnit.Framework;
using Tests.Services;

namespace Tests
{
    [TestFixture]
    public class MagingAIDontImmediatelyFixHighSinkStatTests : Design.MagingAITest
    {
        public Item item;

        [SetUp]
        public void InitSetupItem() {
            item = new Item(new ItemStatRepository(new[] {
                new ItemStat("vitality", 330, 301, 400),
                new ItemStat("strength", 93, 81, 100),
                new ItemStat("wisdom", 38, 31, 40),
                new ItemStat("critical", 5, 4, 5),
                new ItemStat("range", 0, 1, 1),
                new ItemStat("neutral_damage", 18, 16, 20),
                new ItemStat("earth_damage", 18, 16, 20),
                new ItemStat("per_neutral_resistance", 10, 7, 10),
                new ItemStat("per_earth_resistance", 10, 7, 10),
            }));
            Config.ResetUserSettings(item);
        }

        [Test]
        public void TestRestoreHighSinkStatsImmediatelyBasic() {
            MageConfigProvider.RestoreHighSinkStatsImmediately = true;
            var action = AI.ResolveAction(item) as CombineRune;
            Assert.AreEqual(Stat.Range, action!.Rune.Stat);
            
            MageConfigProvider.RestoreHighSinkStatsImmediately = false;
            var action2 = AI.ResolveAction(item) as CombineRune;
            Assert.AreNotEqual(Stat.Range, action2!.Rune.Stat);
        }

        [Test]
        public void TestDontRestoreHighSinkStatsImmediatelySinkAvailableTargetsReachedGoForOvermage() {
            item = new Item(new ItemStatRepository(new[] {
                new ItemStat("vitality", 399, 301, 400),
                new ItemStat("strength", 93, 81, 100),
                new ItemStat("wisdom", 38, 31, 40),
                new ItemStat("critical", 5, 4, 5),
                new ItemStat("range", 0, 1, 1),
                new ItemStat("neutral_damage", 18, 16, 20),
                new ItemStat("earth_damage", 18, 16, 20),
                new ItemStat("per_neutral_resistance", 10, 7, 10),
                new ItemStat("per_earth_resistance", 10, 7, 10),
            }));
            Config.ResetUserSettings(item);
            Job.Sink = 10;
            MageConfigProvider.RestoreHighSinkStatsImmediately = false;
            
            var action = AI.ResolveAction(item) as CombineRune;
            Assert.AreEqual(new Rune(Stat.Vitality, Rune.RuneType.Ra), action!.Rune);
        }

        [Test]
        public void TestRestoreHighSinkStatsImmediatelyTargetsReachedNoMoreSink() {
            item = new Item(new ItemStatRepository(new[] {
                new ItemStat("vitality", 450, 301, 400),
                new ItemStat("strength", 93, 81, 100),
                new ItemStat("wisdom", 38, 31, 40),
                new ItemStat("critical", 5, 4, 5),
                new ItemStat("range", 0, 1, 1),
                new ItemStat("neutral_damage", 18, 16, 20),
                new ItemStat("earth_damage", 18, 16, 20),
                new ItemStat("per_neutral_resistance", 10, 7, 10),
                new ItemStat("per_earth_resistance", 10, 7, 10),
            }));
            Config.ResetUserSettings(item);
            Job.Sink = 1;
            MageConfigProvider.RestoreHighSinkStatsImmediately = false;
            
            var action = AI.ResolveAction(item) as CombineRune;
            Assert.AreEqual(Stat.Range, action!.Rune.Stat);
        }

        [Test]
        public void TestRestoreHighSinkStatsImmediatelyMakePerResistanceExo() {
            item = new Item(new ItemStatRepository(new[] {
                new ItemStat("vitality", 400, 301, 400),
                new ItemStat("strength", 93, 81, 100),
                new ItemStat("wisdom", 38, 31, 40),
                new ItemStat("critical", 5, 4, 5),
                new ItemStat("range", 0, 1, 1),
                new ItemStat("neutral_damage", 18, 16, 20),
                new ItemStat("earth_damage", 18, 16, 20),
                new ItemStat("per_neutral_resistance", 10, 7, 10),
                new ItemStat("per_earth_resistance", 10, 7, 10),
            }));
            Config.ResetUserSettings(item);
            Job.Sink = 12;
            MageConfigProvider.RestoreHighSinkStatsImmediately = false;
            var exoAirRes = MageConfig.ItemStatMageConfig.MakeExo(
                Stat.PerAirResistance, 
                4, 
                1,
                0);
            Config.ChangeStatConfig(Stat.PerAirResistance, exoAirRes);
            
            var action = AI.ResolveAction(item) as CombineRune;
            Assert.AreEqual(Stat.PerAirResistance, action!.Rune.Stat);
        }

    }
}
