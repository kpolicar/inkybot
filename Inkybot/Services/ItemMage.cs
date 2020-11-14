using System;
using Inkybot.Domain;

namespace Inkybot.Domain
{
    internal struct ItemMage
    {
        public readonly Stat Stat;
        public readonly Rune Rune;
        public readonly StatConfig MageConfig;
        public readonly int Value;
        public readonly bool Exo;
        public readonly int Max => MageConfig.Maximum;
            
        public int NumberOfRunesNeededForFullMage =>
            Math.Max(0, (int) Math.Ceiling((Max - Value) / (float) Rune.IncreaseInValue));
            
        public bool WillOvermage => Value + Rune.IncreaseInValue > Max;

            
        public ItemMage(Stat stat, Rune rune, StatConfig mageConfig, int value, bool exo=false) {
            Stat = stat;
            Rune = rune;
            MageConfig = mageConfig;
            Value = value;
            Exo = exo;
        }
    }
}
