using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text.RegularExpressions;
using Inkybot.Helpers;
using Debug = System.Diagnostics.Debug;
using StatConfigResource = Inkybot.Resources.StatConfig;

namespace Inkybot.Domain
{
    public class Stat
    {
        public static Stat FirstOrNew(string identifier) {
            var stat = Stats.DefaultIfEmpty(null).FirstOrDefault(stat => stat.Identifier == identifier);
            return stat ?? new Stat(identifier);
        }
        
        public static void Init() {
            StatDictionary = new ResourceManager("Inkybot.Resources.StatDictionary", Assembly.GetExecutingAssembly())
                .GetResourceSet(CultureInfo.CurrentUICulture, true, true);
            RuneDictionary = new ResourceManager("Inkybot.Resources.RuneDictionary", Assembly.GetExecutingAssembly())
                .GetResourceSet(CultureInfo.CurrentUICulture, true, true);
            Stats = new[] {
                new Stat("initiative", 1010, 0.1f, 0.05f),
                new Stat("vitality", 505, 0.2f, 0.1f),
                new Stat("pods", 404, 0.25f, 0.125f),

                ElementStatData("strength"),
                ElementStatData("intelligence"),
                ElementStatData("agility"),
                ElementStatData("chance"),

                new Stat("critical_resistance", 50, 2f, 1f),
                new Stat("pushback_resistance", 50, 2f, 1f),

                new Stat("power", 50, 2f, 2f),
                new Stat("power_traps", 50, 2f, 2f),

                FlatElementResistanceStatData("neutral"),
                FlatElementResistanceStatData("earth"),
                FlatElementResistanceStatData("fire"),
                FlatElementResistanceStatData("air"),
                FlatElementResistanceStatData("water"),

                new Stat("wisdom", 33, 3f, 2f),
                new Stat("prospecting", 33, 3f, 2f),

                EvadeStatData("lock"),
                EvadeStatData("dodge"),

                ElementDamageStatData("neutral"),
                ElementDamageStatData("earth"),
                ElementDamageStatData("fire"),
                ElementDamageStatData("air"),
                ElementDamageStatData("water"),

                new Stat("critical_damage", 20, 5f, 3f),
                new Stat("pushback_damage", 20, 5f, 3f),
                new Stat("trap_damage", 20, 5f, 5f),
                new Stat("hunting_weapon", 1, 5f, 5f),

                PerElementResistanceStatData("neutral"),
                PerElementResistanceStatData("earth"),
                PerElementResistanceStatData("fire"),
                PerElementResistanceStatData("air"),
                PerElementResistanceStatData("water"),

                ReductionStatData("mp"),
                ReductionStatData("ap"),

                ParryStatData("mp"),
                ParryStatData("ap"),

                new Stat("heals", 10, 10f, 5f),
                new Stat("critical", 10, 10f, 5f),
                new Stat("reflect", 10, 10f, 10f),

                PerModifiersStatData("spell_damage"),
                PerModifiersStatData("weapon_damage"),
                PerModifiersStatData("melee_damage"),
                PerModifiersStatData("ranged_damage"),
                PerModifiersStatData("ranged_resistance"),
                PerModifiersStatData("melee_resistance"),

                new Stat("damage", 5, 20f, 20f),
                new Stat("summons", 3, 30f, 30f),
                new Stat("range", 1, 51f, 25f),
                new Stat("mp", 1, 90f, 45f),
                new Stat("ap", 1, 100f, 50f),
            };
        }

        public override string ToString() {
            return DisplayName;
        }

        public static bool operator == (Stat operand1, Stat operand2) {
            return operand1?.Identifier == operand2?.Identifier;
        }
            
        public static bool operator != (Stat operand1, Stat operand2) {
            return !(operand1 == operand2);
        }
        
        public static Stat[] Stats;
        private static ResourceSet StatDictionary;
        private static ResourceSet RuneDictionary;

        public StatConfigResource Config => (StatConfigResource) Properties.Settings.Default[Identifier];
        
        public int ChangeToPaRuneThreshold {
            get {
                var val = Numbers.Parse(Config.ChangeToPaRuneThreshold);
                return val ?? int.MaxValue;
            }
            set {
                Config.ChangeToPaRuneThreshold = value == int.MaxValue ? "-" : value.ToString();
                Properties.Settings.Default.Save();
            }
        }
        public int ChangeToRaRuneThreshold {
            get {
                var val = Numbers.Parse(Config.ChangeToRaRuneThreshold);
                return val ?? int.MaxValue;
            }
            set {
                Config.ChangeToRaRuneThreshold = value == int.MaxValue ? "-" : value.ToString();
                Properties.Settings.Default.Save();
            }
        }
        public int MaxValueAtWhichSmRuneCanLand {
            get {
                var val = Numbers.Parse(Config.MaxValueAtWhichSmRuneCanLand);
                return val ?? ChangeToPaRuneThreshold;
            }
            set {
                Config.MaxValueAtWhichSmRuneCanLand = value == int.MaxValue ? "-" : value.ToString();
                Properties.Settings.Default.Save();
            }
        }
        public int MaxValueAtWhichPaRuneCanLand {
            get {
                var val = Numbers.Parse(Config.MaxValueAtWhichPaRuneCanLand);
                return val ?? ChangeToRaRuneThreshold;
            }
            set {
                Config.MaxValueAtWhichPaRuneCanLand = value == int.MaxValue ? "-" : value.ToString();
                Properties.Settings.Default.Save();
            }
        }

        public readonly string DisplayName;
        public readonly string Identifier;
        public readonly string RuneName;
        public readonly int Maximum;
        public readonly float NegSinkValue;
        public readonly float SinkValue;
        public readonly bool Mageable;

        private Stat(string identifier,
            int maximum,
            float sinkValue,
            float negSinkValue) {
                
            DisplayName = StatDictionary.GetString(identifier);
            RuneName = RuneDictionary.GetString(identifier);

            Identifier = identifier;
            Maximum = maximum;
            SinkValue = sinkValue;
            NegSinkValue = negSinkValue;
            Mageable = true;
        }
        
        public Stat(string displayName) {
            Identifier = DisplayName = displayName;
            Mageable = false;
        }

        private static Stat ElementStatData(string identifier) {
            return new Stat(identifier, 101, 1f, 1f);
        }

        private static Stat FlatElementResistanceStatData(string elementIdentifier) {
            return new Stat(elementIdentifier + "_resistance", 50, 2f, 2f);
        }

        private static Stat ElementDamageStatData(string elementIdentifier) {
            return new Stat(elementIdentifier + "_damage", 20, 5f, 2.5f);
        }

        private static Stat PerElementResistanceStatData(string elementIdentifier) {
            return new Stat("per_" + elementIdentifier + "_resistance", 16, 6f, 3f);
        }

        private static Stat ReductionStatData(string reductionIdentifier) {
            return new Stat(reductionIdentifier + "_reduction", 14, 7f, 4f);
        }

        private static Stat ParryStatData(string parryIdentifier) {
            return new Stat(parryIdentifier + "_parry", 14, 7f, 4f);
        }

        private static Stat EvadeStatData(string identifier) {
            return new Stat(identifier, 25, 4f, 2f);
        }

        private static Stat PerModifiersStatData(string identifier) {
            return new Stat(identifier, 6, 15f, 8f);
        }
    }
}
