using System;
using Inkybot.Dofus;
using ItemStatMageConfig = Inkybot.Dofus.MageConfig.ItemStatMageConfig;

namespace Inkybot.Dofus.Domain
{
    public struct ItemMage
    {
        public readonly Stat Stat;
        public readonly Rune Rune;
        public readonly ItemStatMageConfig MageConfig;
        public readonly int Value;
        public readonly int Max => MageConfig.Maximum;
        public readonly int Min => MageConfig.Minimum;
        public readonly int? Target => MageConfig.Target;
        
        public readonly bool HasReachedTargetMinimum =>
            MageConfig.TargetMinimum == null || Value >= MageConfig.TargetMinimum;
        
        public readonly bool HasReachedTarget =>
            MageConfig.Target == null || Value >= MageConfig.Target;
            
        public int NumberOfRunesNeededForFullMage =>
            Math.Max(0, (int) Math.Ceiling((Max - Value) / (double) Rune.IncreaseInValue));

        public int NumberOfRunesNeededToReachTarget =>
            Target != null
                ? Math.Max(0, (int) Math.Ceiling((Target.Value - Value) / (double) Rune.IncreaseInValue))
                : 0;

        public ItemMage? WithLowerRuneStrength =>
            Rune.Weaker != null
                ? new ItemMage(Stat, Rune.Weaker, MageConfig, Value)
                : (ItemMage?) null;
        
        public bool CanHitAccordingToConfiguration
        {
            get {
                if ((Value + Rune.IncreaseInValue) * Stat.SinkValue > 101 && WillOvermage)
                    return false;
                switch (Rune.Type) {
                    case Rune.RuneType.Sm when Value <= MageConfig.MaxValueAtWhichSmRuneCanHit || MageConfig.MaxValueAtWhichSmRuneCanHit == null:
                    case Rune.RuneType.Pa when Value <= MageConfig.MaxValueAtWhichPaRuneCanHit || MageConfig.MaxValueAtWhichPaRuneCanHit == null:
                    case Rune.RuneType.Ra:
                        return true;
                    default:
                        return false;
                }
            }
        }
        
        public bool WillOvermage => Value + Rune.IncreaseInValue > Max;
        public bool WillOvertarget => Value + Rune.IncreaseInValue > Target;


        public ItemMage(Item item, Rune rune, ItemStatMageConfig mageConfig) : this(
            rune.Stat,
            rune,
            mageConfig,
            item.Stats[rune.Stat]?.Value ?? 0
        ) {
        }
            
        public ItemMage(Stat stat, Rune rune, ItemStatMageConfig mageConfig, int value) {
            Stat = stat;
            Rune = rune;
            MageConfig = mageConfig;
            Value = value;
        }
        
        public ItemMage Clone
            (Stat? stat=null, Rune? rune=null, ItemStatMageConfig? mageConfig=null, int? value=null)
            => new ItemMage(
                stat ?? Stat,
                rune ?? Rune,
                mageConfig ?? MageConfig,
                value ?? Value);
        

        public static ItemMage WithRuneTypeOffset(ItemMage itemMage, int runeTypeOffset) {
            var runeType = itemMage.Rune.Type;
            runeType = runeType != Rune.RuneType.Sm ? runeType - runeTypeOffset : runeType;
                    
            var rune = new Rune(itemMage.Stat, runeType);
            
            return new ItemMage(itemMage.Stat, rune, itemMage.MageConfig, itemMage.Value);
        }
    }
}
