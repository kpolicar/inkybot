using System.Globalization;
using System.Reflection;
using System.Resources;
using Inkybot.Dofus;
using NUnit.Framework;
using ItemStatMageConfig=Inkybot.Dofus.MageConfig.ItemStatMageConfig;

namespace Tests
{
    public class ItemStatMageConfigTests
    {
        [Test]
        public void TestEquality() {
            var a = new ItemStatMageConfig(Stat.Agility, 5, 10, 9, 8, 0);
            var b = new ItemStatMageConfig(Stat.Agility, 5, 10, 9, 8, 0);
            
            Assert.AreEqual(a, b);
            Assert.True(a == b);
            
            // Different Minimum
            var c = new ItemStatMageConfig(Stat.Agility, 4, 10, 9, 8, 0);
            Assert.AreNotEqual(a, c);
            // Different Maximum
            var d = new ItemStatMageConfig(Stat.Agility, 5, 11, 9, 8, 0);
            Assert.AreNotEqual(a, d);
            // Different Target
            var e = new ItemStatMageConfig(Stat.Agility, 5, 10, 8, 8, 0);
            Assert.AreNotEqual(a, e);
            // Different Target Minimum
            var f = new ItemStatMageConfig(Stat.Agility, 5, 10, 9, 7, 0);
            Assert.AreNotEqual(a, f);
            // Different Stat
            var g = new ItemStatMageConfig(Stat.Chance, 5, 10, 9, 8, 0);
            Assert.AreNotEqual(a, g);
            
            // Nullable
            Assert.AreNotEqual(a, null);
            Assert.AreNotEqual(null, a);
            Assert.False(a == null);
            Assert.True(a != null);
            
            // Equality and ==
            Assert.AreEqual(a == b, a.Equals(b));
            Assert.AreEqual(a != b, !a.Equals(b));
            
            // Different Priority
            var h = new ItemStatMageConfig(Stat.Chance, 5, 10, 9, 8, 0);
            var i = new ItemStatMageConfig(Stat.Chance, 5, 10, 9, 8, 1);
            Assert.AreNotEqual(h, i);
            Assert.False(h == i);
            var j = i.Clone(i.Target, i.TargetMinimum, i.Priority);
            Assert.AreEqual(i, j);
            Assert.True(i == j);
            var k = i.Clone(i.Target, i.TargetMinimum, 2);
            Assert.AreNotEqual(k, i);
        }
    }
}
