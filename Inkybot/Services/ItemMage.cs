using System;
using Inkybot.Domain;

namespace Inkybot.Services
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
        

        public static ItemMage WithRuneTypeOffset(ItemMage itemMage, int runeTypeOffset) {
            var runeType = itemMage.Rune.type;
            runeType = runeType != Rune.Type.Sm ? runeType - runeTypeOffset : runeType;
                    
            var rune = new Rune(itemMage.Stat, runeType);
            
            return new ItemMage(itemMage.Stat, rune, itemMage.MageConfig, itemMage.Value, itemMage.Exo);
        }
    }
}
