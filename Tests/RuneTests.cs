using System.Globalization;
using System.Reflection;
using System.Resources;
using Inkybot.Dofus;
using NUnit.Framework;

namespace Tests
{
    public class RuneTests
    {

        [SetUp]
        public void SetupDisplayNames() {
            Rune.Dictionary = new ResourceManager("Tests.Resources.RuneDictionary", Assembly.GetExecutingAssembly())
                .GetResourceSet(new CultureInfo("en"), true, true);
        }

        [Test]
        public void TestEquality() {
            var a = new Rune(Stat.Agility, Rune.RuneType.Sm);
            var b = new Rune(Stat.Agility, Rune.RuneType.Sm);
            Assert.AreEqual(a, b);
            Assert.True(a == b);
            
            // Different Strength
            var c = new Rune(Stat.Agility, Rune.RuneType.Pa);
            Assert.AreNotEqual(a, c);
            Assert.True(a != c);
            
            // Different Stat
            var d = new Rune(Stat.Ap, Rune.RuneType.Sm);
            Assert.AreNotEqual(a, d);
            Assert.True(a != d);
            
            // Nullable
            Assert.AreNotEqual(a, null);
            Assert.AreNotEqual(null, a);
            Assert.False(a == null);
            Assert.True(a != null);
            
            // Equality and ==
            Assert.AreEqual(a == b, a.Equals(b));
            Assert.AreEqual(a != b, !a.Equals(b));
        }
    }
}
