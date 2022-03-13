// Free Custom script made by Wardzz00#0001 and corrected by Association#5241. If you have any request of it doesn't work as intended, feel free to ping me on discord and I'll try to bring a fix to it
// This script is untested. Feel free to comment about if it worked and if not, where the issue was if there is any. 
// Please, keep in mind that if the item has too many lines and the exo line is hidden, the bot may consider the item as finished. 


// Before you use this custom AI, make sure that you turned off "Restore high sink stats immediately (AP,MP,Range,Summons) otherwise the bot may put the AP back before the custom AI activates. --- To be tested
// If you didn't turn it off and it still worked for you, please comment about it so that I know it works and I can change this line. 

// Thanks for using this custom AI ! Have fun, stay safe and make lots of exos ! :)

using System;
using System.Diagnostics;
using Inkybot.Dofus;
using Inkybot.Dofus.Contracts;
using Inkybot.Dofus.Domain;

namespace Script.Exo3Percent
{
    public class ThreePerResistanceThenAp : CustomDofusMagingAI
    {
        protected override bool ShouldPerfectStats => false;
        protected override bool ShouldOvermageToUseRemainingSink => false;

        protected Stat HighSinkStatToUseForExo = Stat.Ap;
        protected Stat StatToExo = Stat.PerAirResistance; // Change this to the res you want to exo

        
        protected override IAction Resolve() {
            if (Item.Stats[HighSinkStatToUseForExo]?.Value == 0) { // if the AP has fallen off
                return Sink >= StatToExo.SinkValue // is there enough sink for another % air res?
                    ? Combine(StatToExo)
                    : Combine(HighSinkStatToUseForExo);
            }

            if (HasLandedAirResExoWithHighSinkStat) { // the % air res exo has landed and the AP is back on the item
                return Finish();
            }

            return ResolveDefaultExcludingStat(HighSinkStatToUseForExo);
        }

        protected bool HasLandedAirResExoWithHighSinkStat =>
            Item.Stats[HighSinkStatToUseForExo]?.Value != 0
            && Item.Stats[StatToExo]?.Value > 0;
    }
}
    