using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using Inkybot.Dofus;

namespace Inkybot.Dofus
{
    public struct Stat
    {
        public readonly string Identifier;
        public readonly int Maximum;
        public readonly float SinkValue;
        public readonly float NegSinkValue;
        public readonly bool CanUsePaRunes;
        public readonly bool CanUseRaRunes;
        public readonly bool Mageable;
        public Rune.Type StrongestRuneType
            => CanUseRaRunes ? Rune.Type.Ra : CanUsePaRunes ? Rune.Type.Pa : Rune.Type.Sm;

        private Stat(
            string identifier,
            int maximum,
            float sinkValue,
            float negSinkValue,
            bool canUsePaRunes,
            bool canUseRaRunes) =>
            (Identifier, Maximum, SinkValue, NegSinkValue, CanUsePaRunes, CanUseRaRunes, Mageable) =
            (identifier, maximum, sinkValue, negSinkValue, canUsePaRunes, canUseRaRunes, true);

        public Stat(string identifier) =>
            (Identifier, Maximum, SinkValue, NegSinkValue, CanUsePaRunes, CanUseRaRunes, Mageable) =
            (identifier, 0, 0, 0, false, false, false);

        public override bool Equals(object obj) {
            if (obj is Stat stat)
                return Identifier == stat.Identifier;
            return base.Equals(obj);
        }
        public override int GetHashCode() => Identifier.GetHashCode();
        public static bool operator ==(Stat x, Stat y) => x.Equals(y);
        public static bool operator !=(Stat x, Stat y) => !x.Equals(y);

        public readonly static Stat Initiative = new Stat("initiative", 1010, 0.1f, 0.05f, true, true);
        public readonly static Stat Vitality = new Stat("vitality", 505, 0.2f, 0.1f, true, true);
        public readonly static Stat Pods = new Stat("pods", 404, 0.25f, 0.125f, true, true);
        public readonly static Stat Strength = new Stat("strength", 101, 1, 1, true, true);
        public readonly static Stat Intelligence = new Stat("intelligence", 101, 1, 1, true, true);
        public readonly static Stat Agility = new Stat("agility", 101, 1, 1, true, true);
        public readonly static Stat Chance = new Stat("chance", 101, 1, 1, true, true);
        public readonly static Stat CriticalResistance = new Stat("critical_resistance", 50, 2, 1, true, false);
        public readonly static Stat PushbackResistance = new Stat("pushback_resistance", 50, 2, 1, true, false);
        public readonly static Stat Power = new Stat("power", 50, 2, 2, true, true);
        public readonly static Stat PowerTraps = new Stat("power_traps", 50, 2, 2, true, true);
        public readonly static Stat NeutralResistance = new Stat("neutral_resistance", 50, 2, 2, true, false);
        public readonly static Stat EarthResistance = new Stat("earth_resistance", 50, 2, 2, true, false);
        public readonly static Stat FireResistance = new Stat("fire_resistance", 50, 2, 2, true, false);
        public readonly static Stat AirResistance = new Stat("air_resistance", 50, 2, 2, true, false);
        public readonly static Stat WaterResistance = new Stat("water_resistance", 50, 2, 2, true, false);
        public readonly static Stat Wisdom = new Stat("wisdom", 33, 3, 2, true, true);
        public readonly static Stat Prospecting = new Stat("prospecting", 33, 3, 2, true, false);
        public readonly static Stat Lock = new Stat("lock", 25, 4, 2, true, false);
        public readonly static Stat Dodge = new Stat("dodge", 25, 4, 2, true, false);
        public readonly static Stat NeutralDamage = new Stat("neutral_damage", 20, 5, 2.5f, true, false);
        public readonly static Stat EarthDamage = new Stat("earth_damage", 20, 5, 2.5f, true, false);
        public readonly static Stat FireDamage = new Stat("fire_damage", 20, 5, 2.5f, true, false);
        public readonly static Stat AirDamage = new Stat("air_damage", 20, 5, 2.5f, true, false);
        public readonly static Stat WaterDamage = new Stat("water_damage", 20, 5, 2.5f, true, false);
        public readonly static Stat CriticalDamage = new Stat("critical_damage", 20, 5, 3, true, false);
        public readonly static Stat PushbackDamage = new Stat("pushback_damage", 20, 5, 3, true, false);
        public readonly static Stat TrapDamage = new Stat("trap_damage", 20, 5, 5, true, false);
        public readonly static Stat HuntingWeapon = new Stat("hunting_weapon", 1, 5, 5, false, false);
        public readonly static Stat PerNeutralResistance = new Stat("per_neutral_resistance", 16, 6, 3, false, false);
        public readonly static Stat PerEarthResistance = new Stat("per_earth_resistance", 16, 6, 3, false, false);
        public readonly static Stat PerFireResistance = new Stat("per_fire_resistance", 16, 6, 3, false, false);
        public readonly static Stat PerAirResistance = new Stat("per_air_resistance", 16, 6, 3, false, false);
        public readonly static Stat PerWaterResistance = new Stat("per_water_resistance", 16, 6, 3, false, false);
        public readonly static Stat MpReduction = new Stat("mp_reduction", 14, 7, 4, true, false);
        public readonly static Stat ApReduction = new Stat("ap_reduction", 14, 7, 4, true, false);
        public readonly static Stat MpParry = new Stat("mp_parry", 14, 7, 4, true, false);
        public readonly static Stat ApParry = new Stat("ap_parry", 14, 7, 4, true, false);
        public readonly static Stat Heals = new Stat("heals", 10, 10, 5, true, false);
        public readonly static Stat Critical = new Stat("critical", 10, 10, 5, false, false);
        public readonly static Stat Reflect = new Stat("reflect", 10, 10, 10, false, false);
        public readonly static Stat SpellDamage = new Stat("spell_damage", 6, 15, 8, false, false);
        public readonly static Stat WeaponDamage = new Stat("weapon_damage", 6, 15, 8, false, false);
        public readonly static Stat MeleeDamage = new Stat("melee_damage", 6, 15, 8, false, false);
        public readonly static Stat RangedDamage = new Stat("ranged_damage", 6, 15, 8, false, false);
        public readonly static Stat RangedResistance = new Stat("ranged_resistance", 6, 15, 8, false, false);
        public readonly static Stat MeleeResistance = new Stat("melee_resistance", 6, 15, 8, false, false);
        public readonly static Stat Damage = new Stat("damage", 5, 20, 20, false, false);
        public readonly static Stat Summons = new Stat("summons", 3, 30, 30, false, false);
        public readonly static Stat Range = new Stat("range", 1, 51, 25, false, false);
        public readonly static Stat Mp = new Stat("mp", 1, 90, 45, false, false);
        public readonly static Stat Ap = new Stat("ap", 1, 100, 50, false, false);
        
        public readonly static Dictionary<string, Stat> Stats = new Dictionary<string, Stat> {
            { "initiative", Initiative },
            { "vitality", Vitality },
            { "pods", Pods },
            { "strength", Strength },
            { "intelligence", Intelligence },
            { "agility", Agility },
            { "chance", Chance },
            { "critical_resistance", CriticalResistance },
            { "pushback_resistance", PushbackResistance },
            { "power", Power },
            { "power_traps", PowerTraps },
            { "neutral_resistance", NeutralResistance },
            { "earth_resistance", EarthResistance },
            { "fire_resistance", FireResistance },
            { "air_resistance", AirResistance },
            { "water_resistance", WaterResistance },
            { "wisdom", Wisdom },
            { "prospecting", Prospecting },
            { "lock", Lock },
            { "dodge", Dodge },
            { "neutral_damage", NeutralDamage },
            { "earth_damage", EarthDamage },
            { "fire_damage", FireDamage },
            { "air_damage", AirDamage },
            { "water_damage", WaterDamage },
            { "critical_damage", CriticalDamage },
            { "pushback_damage", PushbackDamage },
            { "trap_damage", TrapDamage },
            { "hunting_weapon", HuntingWeapon },
            { "per_neutral_resistance", PerNeutralResistance },
            { "per_earth_resistance", PerEarthResistance },
            { "per_fire_resistance", PerFireResistance },
            { "per_air_resistance", PerAirResistance },
            { "per_water_resistance", PerWaterResistance },
            { "mp_reduction", MpReduction },
            { "ap_reduction", ApReduction },
            { "mp_parry", MpParry },
            { "ap_parry", ApParry },
            { "heals", Heals },
            { "critical", Critical },
            { "reflect", Reflect },
            { "spell_damage", SpellDamage },
            { "weapon_damage", WeaponDamage },
            { "melee_damage", MeleeDamage },
            { "ranged_damage", RangedDamage },
            { "ranged_resistance", RangedResistance },
            { "melee_resistance", MeleeResistance },
            { "damage", Damage },
            { "summons", Summons },
            { "range", Range },
            { "mp", Mp },
            { "ap", Ap },
        };
        

        public readonly struct StatConfig
        {
            public static StatConfig None = new StatConfig();
            public readonly int? MaxValueAtWhichSmRuneCanHit;
            public readonly int? ChangeToPaRuneThreshold;
            public readonly int? MaxValueAtWhichPaRuneCanHit;
            public readonly int? ChangeToRaRuneThreshold;
            public readonly bool HighSinkStat;
            
            public StatConfig(
                int? maxValueSmRuneCanHit=null,
                int? changeToPaRuneThreshold=null,
                int? maxValuePaRuneCanHit=null,
                int? changeToRaRuneThreshold=null,
                bool highSinkStat=false) =>
                (MaxValueAtWhichSmRuneCanHit, ChangeToPaRuneThreshold, MaxValueAtWhichPaRuneCanHit, ChangeToRaRuneThreshold, HighSinkStat) =
                (maxValueSmRuneCanHit, changeToPaRuneThreshold, maxValuePaRuneCanHit, changeToRaRuneThreshold, highSinkStat);

            public StatConfig((
                int? maxValueSmRuneCanHit,
                int? changeToPaRuneThreshold,
                int? maxValuePaRuneCanHit,
                int? changeToRaRuneThreshold,
                bool highSinkStat) a) :
                this(
                    a.maxValueSmRuneCanHit,
                    a.changeToPaRuneThreshold,
                    a.maxValuePaRuneCanHit,
                    a.changeToRaRuneThreshold,
                    a.highSinkStat) { }

            public (int? maxValueSmRuneCanHit,
                int? changeToPaRuneThreshold,
                int? maxValuePaRuneCanHit,
                int? changeToRaRuneThreshold,
                bool highSinkStat)
                Deconstruct() => (
                    MaxValueAtWhichSmRuneCanHit,
                    ChangeToPaRuneThreshold,
                    MaxValueAtWhichPaRuneCanHit,
                    ChangeToRaRuneThreshold,
                    HighSinkStat
                );
        }

        public static readonly Dictionary<Stat, StatConfig> DefaultConfig = new Dictionary<Stat, StatConfig> {
            {Initiative, new StatConfig(210, 170, 570, 470)},
            {Vitality, new StatConfig(115, 90, 310, 290)},
            {Pods, new StatConfig(110, 90, 570, 470)},
            {Strength, new StatConfig(21, 17, 62, 55)},
            {Intelligence, new StatConfig(21, 17, 62, 55)},
            {Agility, new StatConfig(21, 17, 62, 55)},
            {Chance, new StatConfig(21, 17, 62, 55)},
            {CriticalResistance, new StatConfig(19, 14)},
            {PushbackResistance, new StatConfig(19, 14)},
            {Power, new StatConfig(21, 17, 54, 50)},
            {PowerTraps, new StatConfig(21, 17, 54, 50)},
            {NeutralResistance, new StatConfig(19, 14)},
            {EarthResistance, new StatConfig(19, 14)},
            {FireResistance, new StatConfig(19, 14)},
            {AirResistance, new StatConfig(19, 14)},
            {WaterResistance, new StatConfig(19, 14)},
            {Wisdom, new StatConfig(19, 17, 45, 40)},
            {Prospecting, new StatConfig(19, 17)},
            {Lock, new StatConfig(19, 14)},
            {Dodge, new StatConfig(19, 14)},
            {NeutralDamage, new StatConfig(19, 14)},
            {EarthDamage, new StatConfig(19, 14)},
            {FireDamage, new StatConfig(19, 14)},
            {AirDamage, new StatConfig(19, 14)},
            {WaterDamage, new StatConfig(19, 14)},
            {CriticalDamage, new StatConfig(19, 14)},
            {PushbackDamage, new StatConfig(19, 14)},
            {TrapDamage, new StatConfig(19, 14)},
            {HuntingWeapon, StatConfig.None},
            {PerNeutralResistance, StatConfig.None},
            {PerEarthResistance, StatConfig.None},
            {PerFireResistance, StatConfig.None},
            {PerAirResistance, StatConfig.None},
            {PerWaterResistance, StatConfig.None},
            {MpReduction, new StatConfig(15, 12)},
            {ApReduction, new StatConfig(15, 12)},
            {MpParry, new StatConfig(15, 12)},
            {ApParry, new StatConfig(15, 12)},
            {Heals, new StatConfig(19, 15)},
            {Critical, StatConfig.None},
            {Reflect, StatConfig.None},
            {SpellDamage, StatConfig.None},
            {WeaponDamage, StatConfig.None},
            {MeleeDamage, StatConfig.None},
            {RangedDamage, StatConfig.None},
            {RangedResistance, StatConfig.None},
            {MeleeResistance, StatConfig.None},
            {Damage, StatConfig.None},
            {Summons, new StatConfig(highSinkStat: true)},
            {Range, new StatConfig(highSinkStat: true)},
            {Mp, new StatConfig(highSinkStat: true)},
            {Ap, new StatConfig(highSinkStat: true)},
        };

        
        public static Stat FirstOrNew(string identifier) {
            return Stats.ContainsKey(identifier)
                ? Stats[identifier]
                : new Stat(identifier);
        }
    }
}
