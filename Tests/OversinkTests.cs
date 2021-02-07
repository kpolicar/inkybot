using System.Globalization;
using System.Reflection;
using System.Resources;
using Inkybot.Dofus;
using Inkybot.Dofus.Repositories;
using NUnit.Framework;

namespace Tests
{
    public class OversinkTests
    {
        [SetUp]
        public void SetupDisplayNames() {
            Rune.Dictionary = new ResourceManager("Tests.Resources.RuneDictionary", Assembly.GetExecutingAssembly())
                .GetResourceSet(new CultureInfo("en"), true, true);
            Stat.Dictionary = new ResourceManager("Tests.Resources.StatDictionary", Assembly.GetExecutingAssembly())
                .GetResourceSet(new CultureInfo("en"), true, true);
        }

        [Test]
        public void TestItemStats() {
            var itemStat1 = new ItemStat("vitality", 405, 301, 400);
            Assert.AreEqual(1, itemStat1.Oversink);
            
            var itemStat2 = new ItemStat("strength", 10, 5, 5);
            Assert.AreEqual(5, itemStat2.Oversink);
        }

        [Test]
        public void TestItem() {
            var item = new Item(new ItemStatRepository(new[] {
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
            
            Assert.AreEqual(100, item.Oversink);
        }
    }
}
