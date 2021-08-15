using System;
using System.Diagnostics;
using Inkybot.Dofus;
using Inkybot.Dofus.Contracts;
using Inkybot.Dofus.Domain;

namespace Script.Crocoring
{
    public class ResBeforeApFalls : CustomDofusMagingAI
    {
        protected override bool ShouldPerfectStats => false;
        protected override bool ShouldOvermageToUseRemainingSink => false;

        protected int? ValueOfStatToExo =>
            Item.Stats[Stat.PerAirResistance]?.Value;


        protected override IAction Resolve() {
            if (HasLandedAirResExoWithAp) {
                return Finish();
            }
            return ResolveDefault();
        }

        protected bool HasLandedAirResExoWithAp =>
            Item.Stats[Stat.Ap]?.Value != 0 && ValueOfStatToExo > 0;

        protected override ItemMage? BeforeExoRune(ItemMage proposedMage) {
            if (Item.Stats[Stat.Ap]?.Value == 0) {
                if (Sink >= 6 || ValueOfStatToExo < 2) { // minimum of exo is 2
                    return ItemMage(Stat.PerAirResistance);
                }
                return ItemMage(Stat.Ap);
            }

            return null;
        }
    }
}
