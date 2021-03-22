using System.Diagnostics;
using System.Security.Policy;
using Inkybot.Dofus.Domain;

namespace Inkybot.Dofus.Contracts
{
    public abstract class CustomDofusMagingAI : DofusMagingAI
    {
        private DofusMagingAI Default = null!;
        protected virtual bool ShouldPerfectStats => true;
        protected virtual bool ShouldOvermageToReachMinimum => true;
        protected virtual bool ShouldOvermageToUseRemainingSink => true;

        protected IAction ResolveDefault() =>
            Default.ResolveAction(Item);

        public void SetDefaultMagingAI(DofusMagingAI defaultAI) =>
            Default = defaultAI;

        public ItemMage? OverrideMageToPerfectStatsWithSink(ItemMage proposedMage) =>
            ShouldPerfectStats
                ? proposedMage
                : (ItemMage?) null;
        
        public ItemMage? OverrideOvermageToReachMinimum(ItemMage proposedMage) =>
            ShouldOvermageToReachMinimum
                ? proposedMage
                : (ItemMage?) null;
        
        public ItemMage? OverrideOvermageWithRemainingSink(ItemMage proposedMage) =>
            ShouldOvermageToUseRemainingSink
                ? proposedMage
                : (ItemMage?) null;

        public ItemMage? OverrideExoMage(ItemMage proposedMage) {
            var overridenCombine = BeforeExoRune(proposedMage);
            return overridenCombine ?? proposedMage;
        }
        
        protected virtual ItemMage? BeforeExoRune(ItemMage proposedMage) => null;
    }
}
