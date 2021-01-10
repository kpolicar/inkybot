using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using Inkybot.Dofus;
using Inkybot.Dofus.Contracts;

namespace Inkybot.Dofus
{
    public class Stat
    {
        public static StatConfigProvider? configManager;
        public static StatConfigProvider ConfigManager {
            get => configManager ??= DefaultStatConfigProvider.Instance;
            set => configManager = value;
        }
        public static ResourceSet Dictionary = null!;
        
        public readonly string Identifier;
        public readonly int Maximum;
        public readonly float SinkValue;
        public readonly float NegSinkValue;
        public readonly bool CanUsePaRunes;
        public readonly bool CanUseRaRunes;
        public readonly bool Mageable;
        public Rune.RuneType StrongestRuneType
            => CanUseRaRunes ? Rune.RuneType.Ra : CanUsePaRunes ? Rune.RuneType.Pa : Rune.RuneType.Sm;
        public string DisplayName => Dictionary.GetString(Identifier)!;
        public string RuneName => Rune.Dictionary.GetString(Identifier)!;
        public StatConfig Config => ConfigManager.Config(this);

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

        public override bool Equals(object? obj) =>
            obj is Stat other && Identifier.Equals(other.Identifier);
        public override int GetHashCode() => Identifier.GetHashCode();
        public static bool operator ==(Stat x, Stat y) => x.Equals(y);
        public static bool operator !=(Stat x, Stat y) => !x.Equals(y);
        public override string ToString() => DisplayName;

        public static readonly Stat Initiative = new Stat("initiative", 1010, 0.1f, 0.05f, true, true);
        public static readonly Stat Vitality = new Stat("vitality", 505, 0.2f, 0.1f, true, true);
        public static readonly Stat Pods = new Stat("pods", 404, 0.25f, 0.125f, true, true);
        public static readonly Stat Strength = new Stat("strength", 101, 1, 1, true, true);
        public static readonly Stat Intelligence = new Stat("intelligence", 101, 1, 1, true, true);
        public static readonly Stat Agility = new Stat("agility", 101, 1, 1, true, true);
        public static readonly Stat Chance = new Stat("chance", 101, 1, 1, true, true);
        public static readonly Stat CriticalResistance = new Stat("critical_resistance", 50, 2, 1, true, false);
        public static readonly Stat PushbackResistance = new Stat("pushback_resistance", 50, 2, 1, true, false);
        public static readonly Stat Power = new Stat("power", 50, 2, 2, true, true);
        public static readonly Stat PowerTraps = new Stat("power_traps", 50, 2, 2, true, true);
        public static readonly Stat NeutralResistance = new Stat("neutral_resistance", 50, 2, 2, true, false);
        public static readonly Stat EarthResistance = new Stat("earth_resistance", 50, 2, 2, true, false);
        public static readonly Stat FireResistance = new Stat("fire_resistance", 50, 2, 2, true, false);
        public static readonly Stat AirResistance = new Stat("air_resistance", 50, 2, 2, true, false);
        public static readonly Stat WaterResistance = new Stat("water_resistance", 50, 2, 2, true, false);
        public static readonly Stat Wisdom = new Stat("wisdom", 33, 3, 2, true, true);
        public static readonly Stat Prospecting = new Stat("prospecting", 33, 3, 2, true, false);
        public static readonly Stat Lock = new Stat("lock", 25, 4, 2, true, false);
        public static readonly Stat Dodge = new Stat("dodge", 25, 4, 2, true, false);
        public static readonly Stat NeutralDamage = new Stat("neutral_damage", 20, 5, 2.5f, true, false);
        public static readonly Stat EarthDamage = new Stat("earth_damage", 20, 5, 2.5f, true, false);
        public static readonly Stat FireDamage = new Stat("fire_damage", 20, 5, 2.5f, true, false);
        public static readonly Stat AirDamage = new Stat("air_damage", 20, 5, 2.5f, true, false);
        public static readonly Stat WaterDamage = new Stat("water_damage", 20, 5, 2.5f, true, false);
        public static readonly Stat CriticalDamage = new Stat("critical_damage", 20, 5, 3, true, false);
        public static readonly Stat PushbackDamage = new Stat("pushback_damage", 20, 5, 3, true, false);
        public static readonly Stat TrapDamage = new Stat("trap_damage", 20, 5, 5, true, false);
        public static readonly Stat HuntingWeapon = new Stat("hunting_weapon", 1, 5, 5, false, false);
        public static readonly Stat PerNeutralResistance = new Stat("per_neutral_resistance", 16, 6, 3, false, false);
        public static readonly Stat PerEarthResistance = new Stat("per_earth_resistance", 16, 6, 3, false, false);
        public static readonly Stat PerFireResistance = new Stat("per_fire_resistance", 16, 6, 3, false, false);
        public static readonly Stat PerAirResistance = new Stat("per_air_resistance", 16, 6, 3, false, false);
        public static readonly Stat PerWaterResistance = new Stat("per_water_resistance", 16, 6, 3, false, false);
        public static readonly Stat MpReduction = new Stat("mp_reduction", 14, 7, 4, true, false);
        public static readonly Stat ApReduction = new Stat("ap_reduction", 14, 7, 4, true, false);
        public static readonly Stat MpParry = new Stat("mp_parry", 14, 7, 4, true, false);
        public static readonly Stat ApParry = new Stat("ap_parry", 14, 7, 4, true, false);
        public static readonly Stat Heals = new Stat("heals", 10, 10, 5, true, false);
        public static readonly Stat Critical = new Stat("critical", 10, 10, 5, false, false);
        public static readonly Stat Reflect = new Stat("reflect", 10, 10, 10, false, false);
        public static readonly Stat SpellDamage = new Stat("spell_damage", 6, 15, 8, false, false);
        public static readonly Stat WeaponDamage = new Stat("weapon_damage", 6, 15, 8, false, false);
        public static readonly Stat MeleeDamage = new Stat("melee_damage", 6, 15, 8, false, false);
        public static readonly Stat RangedDamage = new Stat("ranged_damage", 6, 15, 8, false, false);
        public static readonly Stat RangedResistance = new Stat("ranged_resistance", 6, 15, 8, false, false);
        public static readonly Stat MeleeResistance = new Stat("melee_resistance", 6, 15, 8, false, false);
        public static readonly Stat Damage = new Stat("damage", 5, 20, 20, false, false);
        public static readonly Stat Summons = new Stat("summons", 3, 30, 30, false, false);
        public static readonly Stat Range = new Stat("range", 1, 51, 25, false, false);
        public static readonly Stat Mp = new Stat("mp", 1, 90, 45, false, false);
        public static readonly Stat Ap = new Stat("ap", 1, 100, 50, false, false);
        
        public static readonly Dictionary<string, Stat> Stats = new Dictionary<string, Stat> {
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
        

        public static Stat FirstOrNew(string identifier) {
            return Stats.ContainsKey(identifier)
                ? Stats[identifier]
                : new Stat(identifier);
        }
    }
}
