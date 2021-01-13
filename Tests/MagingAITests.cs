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
            // item = new Item(new ItemStatRepository(new[] {
            //     new ItemStat("vitality", 243, 201, 250),
            //     new ItemStat("strength", 52, 41, 60),
            //     new ItemStat("wisdom", 13, 16, 20),
            //     new ItemStat("critical", 7, 4, 7),
            //     new ItemStat("neutral_damage", 11, 9, 12),
            //     new ItemStat("earth_damage", 11, 9, 12),
            //     new ItemStat("per_water_resistance", 8, 7, 10),
            //     new ItemStat("ap_parry", -5, -5, -5),
            //     new ItemStat("critical_damage", 8, 7, 10),
            // }));
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
        public void Test() {
            Config.ChangeStatConfigTargetMinimum(Stat.Vitality, 370);
            var action = AI.ResolveAction(item) as CombineRune;
            
            TestContext.WriteLine(action?.Rune.ToString() ?? "null");
        }
    }
}
