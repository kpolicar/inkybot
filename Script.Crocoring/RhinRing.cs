using System;
using System.Diagnostics;
using Inkybot.Dofus;
using Inkybot.Dofus.Contracts;
using Inkybot.Dofus.Domain;

namespace Script.Crocoring
{
    public class RhinRing : CustomDofusMagingAI
    {
        protected override IAction Resolve() {
            return ResolveDefault();
        }
        
        protected override ItemMage? BeforeExoRune(ItemMage proposedMage) =>
            Item.Stats[Stat.WaterDamage]?.Value == 17
                ? ItemMage(new Rune(Stat.WaterDamage, Rune.RuneType.Pa))
                : null;
    }
}
