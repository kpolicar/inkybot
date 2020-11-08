using System;

namespace Inkybot
{
    internal struct ItemMage
    {
        public readonly Stat Stat;
        public readonly Rune Rune;
        public readonly StatConfig MageConfig;
        public readonly int Value;
        public readonly int Max => MageConfig.maximum;
            
        public int NumberOfRunesNeededForFullMage =>
            (int) Math.Ceiling((Max - Value) / (float) Rune.IncreaseInValue);
            
        public bool WillOvermage => Value + Rune.IncreaseInValue > Max;

            
        public ItemMage(Stat stat, Rune rune, StatConfig mageConfig, int value) {
            Stat = stat;
            Rune = rune;
            MageConfig = mageConfig;
            Value = value;
        }
    }
}
