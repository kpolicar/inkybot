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
            new Stat("Initiative", "ini", 1010, 0.1f, 0.05f, 200, 375),
            new Stat("Vitality", "vit", 505, 0.2f, 0.1f, 90, 286, 110, 310),
            new Stat("Pods", "pod", 404, 0.25f, 0.125f, 150, 350),

            ElementStatData("Strength", "stre"),
            ElementStatData("Intelligence", "int"),
            ElementStatData("Agility", "agi"),
            ElementStatData("Chance", "cha"),

            new Stat("Critical Resistance", "cri res", 50, 2f, 1f),
            new Stat("Pushback Resistance", "psh res", 50, 2f, 1f),

            new Stat("Power", "pow", 50, 2f, 2f, 15, 40),
            new Stat("Power (traps)", "tra per", 50, 2f, 2f, 15, 40),

            FlatElementResistanceStatData("Neutral"),
            FlatElementResistanceStatData("Earth"),
            FlatElementResistanceStatData("Fire"),
            FlatElementResistanceStatData("Air"),
            FlatElementResistanceStatData("Water"),

            new Stat("Wisdom", "wis", 33, 3f, 2f, 10, 25),
            new Stat("Prospecting", "prospe", 33, 3f, 2f, 10),

            EvadeStatData("Lock", "loc"),
            EvadeStatData("Dodge", "dod"),

            ElementDamageStatData("Neutral"),
            ElementDamageStatData("Earth"),
            ElementDamageStatData("Fire"),
            ElementDamageStatData("Air"),
            ElementDamageStatData("Water"),

            new Stat("Critical Damage", "cri dam", 20, 5f, 3f, 10),
            new Stat("Pushback Damage", "psh dam", 20, 5f, 3f, 10),
            new Stat("Trap Damage", "tra", 20, 5f, 5f, 10),
            new Stat("Hunting weapon", "hunting", 1, 5f, 5f),

            PerElementResistanceStatData("Neutral"),
            PerElementResistanceStatData("Earth"),
            PerElementResistanceStatData("Fire"),
            PerElementResistanceStatData("Air"),
            PerElementResistanceStatData("Water"),

            ReductionStatData("MP"),
            ReductionStatData("AP"),

            ParryStatData("MP"),
            ParryStatData("AP"),

            new Stat("Heals", "hea", 10, 10f, 5f),
            new Stat("% Critical", "cri", 10, 10f, 5f),
            new Stat("Reflect", "dam ref", 10, 10f, 10f),

            PerModifiersStatData("% Spell Damage", "spe dam"),
            PerModifiersStatData("% Weapon Damage", "we dam per"),
            PerModifiersStatData("% Melee Damage", "me dam per"),
            PerModifiersStatData("% Ranged Damage", "ra dam per"),
            PerModifiersStatData("% Ranged Resistance", "ra res per"),
            PerModifiersStatData("% Melee Resistance", "me res per"),

            new Stat("Damage", "dam", 5, 20f, 20f),
            new Stat("Summons", "summo", 3, 30f, 35f),
            new Stat("Range", "range",1, 51f, 25f),
            new Stat("MP", "mp ga",1, 90f, 45f),
            new Stat("AP", "ap ga",1, 100f, 50f)
        };

        public readonly int changeToPaRuneThreshold;
        public readonly int changeToRaRuneThreshold;
        public readonly int maxValueSmRuneCanHit;
        public readonly int maxValuePaRuneCanHit;

        public readonly string DisplayName;
        public readonly string RuneName;
        public readonly int maximum;
        public readonly float negSinkValue;
        public readonly float sinkValue;


        private Stat(string DisplayName,
            string RuneName,
            int maximum,
            float sinkValue,
            float negSinkValue,
            int changeToPaRuneThreshold = int.MaxValue,
            int changeToRaRuneThreshold = int.MaxValue,
            int maxValueSmRuneCanHit = -1,
            int maxValuePaRuneCanHit = -1) {
            this.DisplayName = DisplayName;
            this.RuneName = RuneName;
            this.maximum = maximum;
            this.sinkValue = sinkValue;
            this.negSinkValue = negSinkValue;
            this.changeToPaRuneThreshold = changeToPaRuneThreshold;
            this.changeToRaRuneThreshold = changeToRaRuneThreshold;
            this.maxValueSmRuneCanHit = maxValueSmRuneCanHit != -1 ? maxValueSmRuneCanHit : changeToPaRuneThreshold;
            this.maxValuePaRuneCanHit = maxValuePaRuneCanHit != -1 ? maxValuePaRuneCanHit : changeToRaRuneThreshold;
        }

        private static Stat ElementStatData(string DisplayName, string RuneName) {
            return new Stat(DisplayName, RuneName, 101, 1f, 1f, 20, 48, 28, 56);
        }

        private static Stat FlatElementResistanceStatData(string elementDisplayName) {
            return new Stat(elementDisplayName + " Resistance", elementDisplayName + " res", 50, 2f, 2f, 15);
        }

        private static Stat ElementDamageStatData(string elementDisplayName) {
            return new Stat(elementDisplayName + " Damage", elementDisplayName + " dam", 20, 5f, 2.5f, 10);
        }

        private static Stat PerElementResistanceStatData(string elementDisplayName) {
            return new Stat("% " + elementDisplayName + " Resistance", elementDisplayName + " res per", 16, 6f, 3f);
        }

        private static Stat ReductionStatData(string DisplayName) {
            return new Stat(DisplayName + " Reduction", DisplayName + " red",14, 7f, 4f, 10);
        }

        private static Stat ParryStatData(string DisplayName) {
            return new Stat(DisplayName + " Parry", DisplayName + " res",14, 7f, 4f, 10);
        }

        private static Stat EvadeStatData(string DisplayName, string RuneName) {
            return new Stat(DisplayName, RuneName, 25, 4f, 2f, 10);
        }

        private static Stat PerModifiersStatData(string DisplayName, string RuneName) {
            return new Stat(DisplayName, RuneName, 6, 15f, 8f);
        }
    }
}
