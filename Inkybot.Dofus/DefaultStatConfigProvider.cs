using System.Collections.Generic;
using Inkybot.Dofus.Contracts;

namespace Inkybot.Dofus
{
    /**
     * <summary>A stat config provider with subjectively-reasonable default values</summary>
     */
    public class DefaultStatConfigProvider : StatConfigProvider
    {

        private static DefaultStatConfigProvider? _instance;
        public static DefaultStatConfigProvider Instance => _instance ??= new DefaultStatConfigProvider();

        private DefaultStatConfigProvider() {
        }
        
        /**
         * <returns>The default configuration for the stat</returns>
         */
        public StatConfig Config(Stat stat) {
            return defaultConfig[stat];
        }

        /**
         * <returns>Default stat configurations</returns>
         */
        public Dictionary<Stat, StatConfig> Config() {
            return defaultConfig;
        }

        private Dictionary<Stat, StatConfig> defaultConfig = new Dictionary<Stat, StatConfig> {
            {Stat.Initiative, new StatConfig(210, 170, 570, 470)},
            {Stat.Vitality, new StatConfig(115, 90, 315, 310)},
            {Stat.Pods, new StatConfig(110, 90, 570, 470)},
            {Stat.Strength, new StatConfig(21, 17, 62, 55)},
            {Stat.Intelligence, new StatConfig(21, 17, 62, 55)},
            {Stat.Agility, new StatConfig(21, 17, 62, 55)},
            {Stat.Chance, new StatConfig(21, 17, 62, 55)},
            {Stat.CriticalResistance, new StatConfig(17, 14, useRaRunes: false)},
            {Stat.PushbackResistance, new StatConfig(17, 14, useRaRunes: false)},
            {Stat.Power, new StatConfig(21, 17, 54, 50)},
            {Stat.PowerTraps, new StatConfig(21, 17, 54, 50)},
            {Stat.NeutralResistance, new StatConfig(17, 14, useRaRunes: false)},
            {Stat.EarthResistance, new StatConfig(17, 14, useRaRunes: false)},
            {Stat.FireResistance, new StatConfig(17, 14, useRaRunes: false)},
            {Stat.AirResistance, new StatConfig(17, 14, useRaRunes: false)},
            {Stat.WaterResistance, new StatConfig(17, 14, useRaRunes: false)},
            {Stat.Wisdom, new StatConfig(19, 17, 45, 40)},
            {Stat.Prospecting, new StatConfig(19, 17, useRaRunes: false)},
            {Stat.Lock, new StatConfig(19, 14, useRaRunes: false)},
            {Stat.Dodge, new StatConfig(19, 14, useRaRunes: false)},
            {Stat.NeutralDamage, new StatConfig(17, 14, useRaRunes: false)},
            {Stat.EarthDamage, new StatConfig(17, 14, useRaRunes: false)},
            {Stat.FireDamage, new StatConfig(17, 14, useRaRunes: false)},
            {Stat.AirDamage, new StatConfig(17, 14, useRaRunes: false)},
            {Stat.WaterDamage, new StatConfig(17, 14, useRaRunes: false)},
            {Stat.CriticalDamage, new StatConfig(17, 14, useRaRunes: false)},
            {Stat.PushbackDamage, new StatConfig(17, 14, useRaRunes: false)},
            {Stat.TrapDamage, new StatConfig(17, 14, useRaRunes: false)},
            {Stat.HuntingWeapon, new StatConfig(usePaRunes: false, useRaRunes: false)},
            {Stat.PerNeutralResistance, new StatConfig(usePaRunes: false, useRaRunes: false)},
            {Stat.PerEarthResistance, new StatConfig(usePaRunes: false, useRaRunes: false)},
            {Stat.PerFireResistance, new StatConfig(usePaRunes: false, useRaRunes: false)},
            {Stat.PerAirResistance, new StatConfig(usePaRunes: false, useRaRunes: false)},
            {Stat.PerWaterResistance, new StatConfig(usePaRunes: false, useRaRunes: false)},
            {Stat.MpReduction, new StatConfig(15, 12, useRaRunes: false)},
            {Stat.ApReduction, new StatConfig(15, 12, useRaRunes: false)},
            {Stat.MpParry, new StatConfig(15, 12, useRaRunes: false)},
            {Stat.ApParry, new StatConfig(15, 12, useRaRunes: false)},
            {Stat.Heals, new StatConfig(19, 15, useRaRunes: false)},
            {Stat.Critical, new StatConfig(usePaRunes: false, useRaRunes: false)},
            {Stat.Reflect, new StatConfig(usePaRunes: false, useRaRunes: false)},
            {Stat.SpellDamage, new StatConfig(usePaRunes: false, useRaRunes: false)},
            {Stat.WeaponDamage, new StatConfig(usePaRunes: false, useRaRunes: false)},
            {Stat.MeleeDamage, new StatConfig(usePaRunes: false, useRaRunes: false)},
            {Stat.RangedDamage, new StatConfig(usePaRunes: false, useRaRunes: false)},
            {Stat.RangedResistance, new StatConfig(usePaRunes: false, useRaRunes: false)},
            {Stat.MeleeResistance, new StatConfig(usePaRunes: false, useRaRunes: false)},
            {Stat.Damage, new StatConfig(usePaRunes: false, useRaRunes: false)},
            {Stat.Summons, new StatConfig(highSinkStat: true, usePaRunes: false, useRaRunes: false)},
            {Stat.Range, new StatConfig(highSinkStat: true, usePaRunes: false, useRaRunes: false)},
            {Stat.Mp, new StatConfig(highSinkStat: true, usePaRunes: false, useRaRunes: false)},
            {Stat.Ap, new StatConfig(highSinkStat: true, usePaRunes: false, useRaRunes: false)},
        };
    }
}
