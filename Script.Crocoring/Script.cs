using System;
using System.Diagnostics;
using Inkybot.Dofus;
using Inkybot.Dofus.Contracts;
using Inkybot.Dofus.Domain;

namespace Script.Crocoring
{
    public class CrocoringMagingAI : CustomDofusMagingAI
    {
        protected override bool ShouldPerfectStats => Sink > 13;
        protected override bool ShouldOvermageToUseRemainingSink => false;

        protected override IAction Resolve() {
            return ResolveDefault();
        }

        protected override ItemMage? BeforeExoRune(ItemMage proposedMage) =>
            (Item.IsOvermaged, Item.HasExo, Sink) switch {
                (_, _, _) when Sink % 10 >= 3 => ItemMage(new Rune(Stat.Vitality, Rune.RuneType.Pa)),
                (_, _, _) when Sink >= 10 => ItemMage(new Rune(Stat.Vitality, Rune.RuneType.Ra)),
                (false, false, 2) => ItemMage(new Rune(Stat.EarthResistance, Rune.RuneType.Sm)),
                (false, false, 1) => ItemMage(new Rune(Stat.Initiative, Rune.RuneType.Sm)),
                _ => null
            };
    }
}
