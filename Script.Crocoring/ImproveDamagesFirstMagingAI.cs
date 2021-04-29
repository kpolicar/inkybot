using System;
using System.Diagnostics;
using Inkybot.Dofus;
using Inkybot.Dofus.Contracts;
using Inkybot.Dofus.Domain;

namespace Script
{
    public class ImproveDamagesFirstMagingAI : CustomDofusMagingAI
    {
        // Should the bot use the sink using it's default behavior?
        protected override bool ShouldPerfectStats =>
            !ShouldImproveAirDamageWithSink &&
            !ShouldImproveWaterDamageWithSink;

        private bool ShouldImproveAirDamageWithSink => Sink >= 6 && IsAirDamageNotAtMaximum;
        private bool ShouldImproveWaterDamageWithSink => Sink >= 6 && IsWaterDamageNotAtMaximum;

        private bool IsAirDamageNotAtMaximum =>
            Item.Stats[Stat.AirDamage]?.Value < Item.Stats[Stat.AirDamage]?.Max;
        private bool IsWaterDamageNotAtMaximum =>
            Item.Stats[Stat.WaterDamage]?.Value < Item.Stats[Stat.WaterDamage]?.Max;

        protected override IAction Resolve() =>
            ResolveDefault();

        protected override ItemMage? BeforeExoRune(ItemMage proposedMage) {
            if (ShouldImproveAirDamageWithSink) {
                return ItemMage(new Rune(Stat.AirDamage, Rune.RuneType.Sm));
            }
            if (ShouldImproveWaterDamageWithSink) {
                return ItemMage(new Rune(Stat.WaterDamage, Rune.RuneType.Sm));
            }

            return null;
        }
    }
}
