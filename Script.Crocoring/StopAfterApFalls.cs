using System;
using System.Diagnostics;
using Inkybot.Dofus;
using Inkybot.Dofus.Contracts;
using Inkybot.Dofus.Domain;

namespace Script.Crocoring
{
    public class StopAfterApFalls : CustomDofusMagingAI
    {
        private bool ApHasFallen =>
            Item.Stats[Stat.Ap]?.Value == 0;
        
        protected override IAction Resolve() {
            if (ApHasFallen) {
                return Finish();
            }
            return ResolveDefault();
        }
    }
}
