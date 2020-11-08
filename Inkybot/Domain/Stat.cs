using System.Linq;

namespace Inkybot
{
    public class Stat
    {
        public static bool operator == (Stat operand1, Stat operand2) {
            return operand1?.DisplayName == operand2?.DisplayName;
        }
            
        public static bool operator != (Stat operand1, Stat operand2) {
            return !(operand1 == operand2);
        }
        
        public static readonly Stat[] Stats = {
            new Stat("Initiative", 1010, 0.1f, 0.05f, 200, 375),
            new Stat("Vitality", 505, 0.2f, 0.1f, 120, 286),
            new Stat("Pods", 404, 0.25f, 0.125f, 150, 350),

            ElementStatData("Strength"),
            ElementStatData("Intelligence"),
            ElementStatData("Agility"),
            ElementStatData("Chance"),

            new Stat("Critical Resistance", 50, 2f, 1f),
            new Stat("Pushback Resistance", 50, 2f, 1f),

            new Stat("Power", 50, 2f, 2f, 15, 40),
            new Stat("Power (traps)", 50, 2f, 2f, 15, 40),

            FlatElementResistanceStatData("Neutral"),
            FlatElementResistanceStatData("Earth"),
            FlatElementResistanceStatData("Fire"),
            FlatElementResistanceStatData("Air"),
            FlatElementResistanceStatData("Water"),

            new Stat("Wisdom", 33, 3f, 2f, 10, 25),
            new Stat("Prospecting", 33, 3f, 2f, 10),

            EvadeStatData("Lock"),
            EvadeStatData("Dodge"),

            ElementDamageStatData("Neutral"),
            ElementDamageStatData("Earth"),
            ElementDamageStatData("Fire"),
            ElementDamageStatData("Air"),
            ElementDamageStatData("Water"),

            new Stat("Critical Damage", 20, 5f, 3f, 10),
            new Stat("Pushback Damage", 20, 5f, 3f, 10),
            new Stat("Trap Damage", 20, 5f, 5f, 10),
            new Stat("Hunting weapon", 1, 5f, 5f),

            PerElementResistanceStatData("Neutral"),
            PerElementResistanceStatData("Earth"),
            PerElementResistanceStatData("Fire"),
            PerElementResistanceStatData("Air"),
            PerElementResistanceStatData("Water"),

            ReductionStatData("MP"),
            ReductionStatData("AP"),

            ParryStatData("MP"),
            ParryStatData("AP"),

            new Stat("Heals", 10, 10f, 5f),
            new Stat("% Critical", 10, 10f, 5f),
            new Stat("Reflect", 10, 10f, 10f),

            PerModifiersStatData("% Spell Damage"),
            PerModifiersStatData("% Ranged Resistance"),
            PerModifiersStatData("% Weapon Damage"),
            PerModifiersStatData("% Melee Damage"),
            PerModifiersStatData("% Melee Resistance"),

            new Stat("Damage", 5, 20f, 20f),
            new Stat("Summons", 3, 30f, 35f),
            new Stat("Range", 1, 51f, 25f),
            new Stat("MP", 1, 90f, 45f),
            new Stat("AP", 1, 100f, 50f)
        };

        public readonly int changeToPaRuneThreshold;
        public readonly int changeToRaRuneThreshold;

        public readonly string DisplayName;
        public readonly int maximum;
        public readonly float negSinkValue;
        public readonly float sinkValue;


        private Stat(string DisplayName,
            int maximum,
            float sinkValue,
            float negSinkValue,
            int changeToPaRuneThreshold = int.MinValue,
            int changeToRaRuneThreshold = int.MinValue) {
            this.DisplayName = DisplayName;
            this.maximum = maximum;
            this.sinkValue = sinkValue;
            this.negSinkValue = negSinkValue;
            this.changeToPaRuneThreshold = changeToPaRuneThreshold;
            this.changeToRaRuneThreshold = changeToRaRuneThreshold;
        }

        private static Stat ElementStatData(string DisplayName) {
            return new Stat(DisplayName, 101, 1f, 1f, 20, 48);
        }

        private static Stat FlatElementResistanceStatData(string elementDisplayName) {
            return new Stat(elementDisplayName + " Resistance", 50, 2f, 2f, 15);
        }

        private static Stat ElementDamageStatData(string elementDisplayName) {
            return new Stat(elementDisplayName + " Damage", 20, 5f, 2.5f, 10);
        }

        private static Stat PerElementResistanceStatData(string elementDisplayName) {
            return new Stat("% " + elementDisplayName + " Resistance", 16, 6f, 3f);
        }

        private static Stat ReductionStatData(string DisplayName) {
            return new Stat(DisplayName + " Reduction", 14, 7f, 4f, 10);
        }

        private static Stat ParryStatData(string DisplayName) {
            return new Stat(DisplayName + " Parry", 14, 7f, 4f, 10);
        }

        private static Stat EvadeStatData(string DisplayName) {
            return new Stat(DisplayName, 25, 4f, 2f, 10);
        }

        private static Stat PerModifiersStatData(string DisplayName) {
            return new Stat(DisplayName, 6, 15f, 8f);
        }
    }
}
