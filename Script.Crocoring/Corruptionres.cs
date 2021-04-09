using System;
using System.Diagnostics;
using Inkybot.Dofus;
using Inkybot.Dofus.Contracts;
using Inkybot.Dofus.Domain;

namespace Script.Crocoring
{
    public class CorruptionRingMagingAI : CustomDofusMagingAI
    {
        protected override bool ShouldPerfectStats => Sink > 21;
        protected override bool ShouldOvermageToUseRemainingSink => false;

        protected override IAction Resolve()
        {
            if (ShouldInsteadTryMpExo) {
                return Combine(Stat.Mp);
            }

            if (HasSuccessfullyMadeMpExo) {
                return Finish();
            }
            return ResolveDefault();
        }

        protected bool ShouldInsteadTryMpExo =>
            Item.Stats[Stat.PerWaterResistance]?.Value >= 1;

        protected bool HasSuccessfullyMadeMpExo =>
            Item.Stats[Stat.Mp]?.Value > 0;

        protected override ItemMage? BeforeExoRune(ItemMage proposedMage) =>
            (Item.IsOvermaged, Item.HasExo, Sink) switch {
                (_, _, _) when Sink >= 3 => ItemMage(new Rune(Stat.PerWaterResistance, Rune.RuneType.Sm)),
                _ => null
            };
    }
}