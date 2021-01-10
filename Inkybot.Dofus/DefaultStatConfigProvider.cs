using System.Collections.Generic;
using Inkybot.Dofus.Contracts;

namespace Inkybot.Dofus
{
    public class DefaultStatConfigProvider : StatConfigProvider
    {

        private static DefaultStatConfigProvider? _instance;
        public static DefaultStatConfigProvider Instance => _instance ??= new DefaultStatConfigProvider();

        private DefaultStatConfigProvider() {
        }
        
        public StatConfig Config(Stat stat) {
            return defaultConfig[stat];
        }

        public Dictionary<Stat, StatConfig> Config() {
            return defaultConfig;
        }

        private Dictionary<Stat, StatConfig> defaultConfig = new Dictionary<Stat, StatConfig> {
            {Stat.Initiative, new StatConfig(210, 170, 570, 470)},
            {Stat.Vitality, new StatConfig(115, 90, 330, 310)},
            {Stat.Pods, new StatConfig(110, 90, 570, 470)},
            {Stat.Strength, new StatConfig(21, 17, 62, 55)},
            {Stat.Intelligence, new StatConfig(21, 17, 62, 55)},
            {Stat.Agility, new StatConfig(21, 17, 62, 55)},
            {Stat.Chance, new StatConfig(21, 17, 62, 55)},
            {Stat.CriticalResistance, new StatConfig(19, 14)},
            {Stat.PushbackResistance, new StatConfig(19, 14)},
            {Stat.Power, new StatConfig(21, 17, 54, 50)},
            {Stat.PowerTraps, new StatConfig(21, 17, 54, 50)},
            {Stat.NeutralResistance, new StatConfig(19, 14)},
            {Stat.EarthResistance, new StatConfig(19, 14)},
            {Stat.FireResistance, new StatConfig(19, 14)},
            {Stat.AirResistance, new StatConfig(19, 14)},
            {Stat.WaterResistance, new StatConfig(19, 14)},
            {Stat.Wisdom, new StatConfig(19, 17, 45, 40)},
            {Stat.Prospecting, new StatConfig(19, 17)},
            {Stat.Lock, new StatConfig(19, 14)},
            {Stat.Dodge, new StatConfig(19, 14)},
            {Stat.NeutralDamage, new StatConfig(19, 14)},
            {Stat.EarthDamage, new StatConfig(19, 14)},
            {Stat.FireDamage, new StatConfig(19, 14)},
            {Stat.AirDamage, new StatConfig(19, 14)},
            {Stat.WaterDamage, new StatConfig(19, 14)},
            {Stat.CriticalDamage, new StatConfig(19, 14)},
            {Stat.PushbackDamage, new StatConfig(19, 14)},
            {Stat.TrapDamage, new StatConfig(19, 14)},
            {Stat.HuntingWeapon, StatConfig.None},
            {Stat.PerNeutralResistance, StatConfig.None},
            {Stat.PerEarthResistance, StatConfig.None},
            {Stat.PerFireResistance, StatConfig.None},
            {Stat.PerAirResistance, StatConfig.None},
            {Stat.PerWaterResistance, StatConfig.None},
            {Stat.MpReduction, new StatConfig(15, 12)},
            {Stat.ApReduction, new StatConfig(15, 12)},
            {Stat.MpParry, new StatConfig(15, 12)},
            {Stat.ApParry, new StatConfig(15, 12)},
            {Stat.Heals, new StatConfig(19, 15)},
            {Stat.Critical, StatConfig.None},
            {Stat.Reflect, StatConfig.None},
            {Stat.SpellDamage, StatConfig.None},
            {Stat.WeaponDamage, StatConfig.None},
            {Stat.MeleeDamage, StatConfig.None},
            {Stat.RangedDamage, StatConfig.None},
            {Stat.RangedResistance, StatConfig.None},
            {Stat.MeleeResistance, StatConfig.None},
            {Stat.Damage, StatConfig.None},
            {Stat.Summons, new StatConfig(highSinkStat: true)},
            {Stat.Range, new StatConfig(highSinkStat: true)},
            {Stat.Mp, new StatConfig(highSinkStat: true)},
            {Stat.Ap, new StatConfig(highSinkStat: true)},
        };
    }
}
