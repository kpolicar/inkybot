using Inkybot;
using Inkybot.Domain;
using Inkybot.Domain.Repositories;
using NUnit.Framework;
using Tests.Design;

namespace Tests
{
    [TestFixture]
    public class MagingAITest : Design.MagingAITest
    {
        public Item item;

        [SetUp]
        public void InitSetupItem() {
            item = new Item(new ItemStatRepository(new[] {
                new ItemStat("vitality", 243, 201, 250),
                new ItemStat("strength", 52, 41, 60),
                new ItemStat("wisdom", 13, 16, 20),
                new ItemStat("critical", 7, 4, 7),
                new ItemStat("neutral_damage", 11, 9, 12),
                new ItemStat("earth_damage", 11, 9, 12),
                new ItemStat("per_water_resistance", 8, 7, 10),
                new ItemStat("ap_parry", -5, -5, -5),
                new ItemStat("critical_damage", 8, 7, 10),
            }));
        }

        [Test]
        public void Test() {
            var config = new Config(item);
            TestContext.WriteLine(config);
        }
    }
}
