using System;
using System.Diagnostics;
using Inkybot.Dofus;
using Inkybot.Dofus.Contracts;
using Inkybot.Dofus.Domain;

namespace Script.Crocoring
{
    public class NomenclatuRingMagingAI : CustomDofusMagingAI
    {
        protected override IAction Resolve()
        {
            if (ShouldInsteadTryMpExo)
                return Combine(Stat.Mp);

            if (HasSuccessfullyMadeMpExo)
                return Finish();
            
            return ResolveDefault();
        }

        protected bool ShouldInsteadTryMpExo =>
            Item.Stats[Stat.Vitality]?.Value >= 320;

        protected bool HasSuccessfullyMadeMpExo =>
            Item.Stats[Stat.Mp]?.Value > 0;
    }
}