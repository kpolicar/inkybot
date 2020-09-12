using System.Collections.ObjectModel;

namespace WindowsFormsApp
{
    public static class Stat
    {
        public struct Data
        {
            public string DisplayName;
            public Data(string displayName, int maximum, float sinkValue, float negSinkValue) {
                this.DisplayName = displayName;
            }
        }
        
        public static readonly Data[] Stats = new [] {
            new Data("Initiative", 1010, 1f, 0.5f),
            new Data("Vitality", 1010, 1f, 0.5f),
            new Data("Pods", 1010, 1f, 0.5f),
            new Data("Strength", 1010, 1f, 0.5f),
            new Data("Intelligence", 1010, 1f, 0.5f),
            new Data("Agility", 1010, 1f, 0.5f),
            new Data("Chance", 1010, 1f, 0.5f),
            new Data("CriticalResistance", 1010, 1f, 0.5f),
            new Data("Pushback Resistance", 1010, 1f, 0.5f),
            new Data("Power", 1010, 1f, 0.5f),
            new Data("PerPowerTrap", 1010, 1f, 0.5f),
            new Data("FixedResistance", 1010, 1f, 0.5f),
            new Data("Wisdom", 1010, 1f, 0.5f),
            new Data("Prospecting", 1010, 1f, 0.5f),
            new Data("Lock", 1010, 1f, 0.5f),
            new Data("Dodge", 1010, 1f, 0.5f),
            new Data("DamageElemental", 1010, 1f, 0.5f),
            new Data("Critical Damage", 1010, 1f, 0.5f),
            new Data("PushbackDamage", 1010, 1f, 0.5f),
            new Data("TraRune", 1010, 1f, 0.5f),
            new Data("Hunting", 1010, 1f, 0.5f),
            new Data("PerResistance", 1010, 1f, 0.5f),
            new Data("Neutral Resistance", 1010, 1f, 0.5f),
            new Data("Water Resistance", 1010, 1f, 0.5f),
            new Data("MPReduction", 1010, 1f, 0.5f),
            new Data("APReduction", 1010, 1f, 0.5f),
            new Data("MP Parry", 1010, 1f, 0.5f),
            new Data("APResist", 1010, 1f, 0.5f),
            new Data("Heal", 1010, 1f, 0.5f),
            new Data("Critical", 1010, 1f, 0.5f),
            new Data("Reflect", 1010, 1f, 0.5f),
            new Data("PerSpellDamage", 1010, 1f, 0.5f),
            new Data("PerRangedResistance", 1010, 1f, 0.5f),
            new Data("PerWeaponDamage", 1010, 1f, 0.5f),
            new Data("PerMeleeDamage", 1010, 1f, 0.5f),
            new Data("MeleeResistance", 1010, 1f, 0.5f),
            new Data("Damage", 1010, 1f, 0.5f),
            new Data("Summon", 1010, 1f, 0.5f),
            new Data("Range", 1010, 1f, 0.5f),
            new Data("MP", 1010, 1f, 0.5f),
            new Data("AP", 1010, 1f, 0.5f),
        };
    }
}