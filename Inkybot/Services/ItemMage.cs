using System;
using Inkybot.Dofus;
using Inkybot.Domain;
using ItemStatMageConfig = Inkybot.Dofus.MageConfig.ItemStatMageConfig;

namespace Inkybot.Services
{
    internal struct ItemMage
    {
        public readonly Stat Stat;
        public readonly Rune Rune;
        public readonly ItemStatMageConfig MageConfig;
        public readonly int Value;
        public readonly bool Exo;
        public readonly int Max => MageConfig.Maximum;
        public readonly int Target => MageConfig.Target;
            
        public int NumberOfRunesNeededForFullMage =>
            Math.Max(0, (int) Math.Ceiling((Max - Value) / (float) Rune.IncreaseInValue));
        
        public int NumberOfRunesNeededToReachTarget =>
            Math.Max(0, (int) Math.Ceiling((Target - Value) / (float) Rune.IncreaseInValue));

        public ItemMage? WithLowerRuneStrength =>
            Rune.Weaker != null
                ? new ItemMage(Stat, Rune.Weaker, MageConfig, Value, Exo)
                : (ItemMage?) null;
        
        public bool CanHit
        {
            get {
                switch (Rune.type) {
                    case Rune.Type.Sm when Value <= MageConfig.MaxValueAtWhichSmRuneCanHit:
                    case Rune.Type.Pa when Value <= MageConfig.MaxValueAtWhichPaRuneCanHit:
                    case Rune.Type.Ra:
                        return true;
                    default:
                        return false;
                }
            }
        }
            
        public bool WillOvermage => Value + Rune.IncreaseInValue > Max;
        public bool WillOvertarget => Value + Rune.IncreaseInValue > Target;

            
        public ItemMage(Stat stat, Rune rune, ItemStatMageConfig mageConfig, int value, bool exo=false) {
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
