using System.Diagnostics;
using System.Security.Policy;
using Inkybot.Dofus.Domain;

namespace Inkybot.Dofus.Contracts
{
    public abstract class CustomDofusMagingAI : DofusMagingAI
    {
        private DofusMagingAI Default = null!;
        private Item resolving = null!;
        
        protected virtual bool ShouldPerfectStats => true;
        protected virtual bool ShouldOvermageToReachMinimum => true;
        protected virtual bool ShouldOvermageToUseRemainingSink => true;

        protected IAction ResolveDefault() =>
            Default.ResolveAction(resolving);

        public void SetDefaultMagingAI(DofusMagingAI defaultAI) =>
            Default = defaultAI;


        protected override IAction Resolve(Item item) {
            var proposed = ResolveDefault();

            if (Sink > 30) {
                
            }

            return proposed;
        }
        
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
        
        public virtual ItemMage? OverrideExoMage(ItemMage proposedMage) =>
            proposedMage;
    }
}
