using Inkybot.Actions;
using Inkybot.Dofus;
using Inkybot.Dofus.Repositories;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class MagingAIOverTargetTest : Design.MagingAITest
    {
        public Item item;

        [SetUp]
        public void InitSetupItem() {
            item = new Item(new ItemStatRepository(new[] {
                new ItemStat("vitality", 330, 301, 400),
                new ItemStat("strength", 93, 81, 100),
                new ItemStat("wisdom", 38, 31, 40),
                new ItemStat("critical", 5, 4, 5),
                new ItemStat("range", 1, 1, 1),
                new ItemStat("neutral_damage", 18, 16, 20),
                new ItemStat("earth_damage", 18, 16, 20),
                new ItemStat("per_neutral_resistance", 10, 7, 10),
                new ItemStat("per_earth_resistance", 10, 7, 10),
            }));
            Config.ResetUserSettings(item);
        }

        [Test]
        public void TestOverTarget() {
            
            Config.ChangeStatConfigTarget(Stat.Vitality, 350);
            var action = AI.ResolveAction(item) as CombineRune;
            Assert.AreEqual(new Rune(Stat.Vitality, Rune.RuneType.Ra), action!.Rune);
            
            Config.ChangeStatConfigTarget(Stat.Vitality, 344);
            action = AI.ResolveAction(item) as CombineRune;
            Assert.IsNull(action);
        }

    }
}
