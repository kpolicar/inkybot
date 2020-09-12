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

        private static Data ElementStatData(string displayName) {
            return new Data(displayName, 101, 1f, 1f);
        }

        private static Data FlatElementResistanceStatData(string elementDisplayName) {
            return new Data(elementDisplayName+" Resistance", 50, 2f, 2f);
        }

        private static Data ElementDamageStatData(string elementDisplayName) {
            return new Data(elementDisplayName+" Damage", 20, 5f, 2.5f);
        }

        private static Data PerElementResistanceStatData(string elementDisplayName) {
            return new Data("% "+elementDisplayName+" Resistance", 16, 6f, 3f);
        }

        private static Data ReductionStatData(string displayName) {
            return new Data(displayName+" Reduction", 14, 7f, 4f);
        }

        private static Data ParryStatData(string displayName) {
            return new Data(displayName+" Parry", 14, 7f, 4f);
        }

        private static Data EvadeStatData(string displayName) {
            return new Data(displayName, 25, 4f, 2f);
        }

        private static Data PerModifiersStatData(string displayName) {
            return new Data(displayName, 6, 15f, 8f);
        }
        
        public static readonly Data[] Stats = {
            new Data("Initiative", 1010, 1f, 0.5f),
            new Data("Vitality", 1010, 1f, 0.5f),
            new Data("Pods", 1010, 1f, 0.5f),
            
            ElementStatData("Strength"),
            ElementStatData("Intelligence"),
            ElementStatData("Agility"),
            ElementStatData("Chance"),
            
            new Data("CriticalResistance", 50, 2f, 1f),
            new Data("Pushback Resistance", 50, 2f, 1f),
            
            new Data("Power", 50, 2f, 2f),
            new Data("PerPowerTrap", 50, 2f, 2f),
            
            FlatElementResistanceStatData("Neutral"),
            FlatElementResistanceStatData("Earth"),
            FlatElementResistanceStatData("Fire"),
            FlatElementResistanceStatData("Air"),
            FlatElementResistanceStatData("Water"),
            
            new Data("Wisdom", 33, 3f, 2f),
            new Data("Prospecting", 33, 3f, 2f),
            
            EvadeStatData("Lock"),
            EvadeStatData("Dodge"),
            
            ElementDamageStatData("Neutral"),
            ElementDamageStatData("Earth"),
            ElementDamageStatData("Fire"),
            ElementDamageStatData("Air"),
            ElementDamageStatData("Water"),
            
            new Data("Critical Damage", 20, 5f, 3f),
            new Data("Pushback Damage", 20, 5f, 3f),
            new Data("Trap Damage", 20, 5f, 5f),
            new Data("Hunting", 1, 5f, 5f),
            
            PerElementResistanceStatData("Neutral"),
            PerElementResistanceStatData("Earth"),
            PerElementResistanceStatData("Fire"),
            PerElementResistanceStatData("Air"),
            PerElementResistanceStatData("Water"),
            
            ReductionStatData("MP"),
            ReductionStatData("AP"),
            
            ParryStatData("MP"),
            ParryStatData("AP"),
            
            new Data("Heal", 10, 10f, 5f),
            new Data("Critical", 10, 10f, 5f),
            new Data("Reflect", 10, 10f, 10f),
            
            PerModifiersStatData("Spell Damage"),
            PerModifiersStatData("Ranged Resistance"),
            PerModifiersStatData("Weapon Damage"),
            PerModifiersStatData("Melee Damage"),
            PerModifiersStatData("Melee Resistance"),
            
            new Data("Damage", 5, 20f, 20f),
            new Data("Summon", 3, 30f, 35f),
            new Data("Range", 1, 51f, 25f),
            new Data("MP", 1, 90f, 45f),
            new Data("AP", 1, 100f, 50f),
        };
    }
}