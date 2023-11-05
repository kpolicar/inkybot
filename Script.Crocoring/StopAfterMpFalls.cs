using System;
using System.Diagnostics;
using Inkybot.Dofus;
using Inkybot.Dofus.Contracts;
using Inkybot.Dofus.Domain;

namespace Script.Crocoring
{
    public class StopAfterMpFalls : CustomDofusMagingAI
    {
        private bool MpHasFallen =>
            Item.Stats[Stat.Mp]?.Value == 0;
        
        protected override IAction Resolve() {
            if (MpHasFallen) {
                return Finish();
            }
            return ResolveDefault();
        }
    }
}
