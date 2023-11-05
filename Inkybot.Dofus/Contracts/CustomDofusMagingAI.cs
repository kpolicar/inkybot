using System.Diagnostics;
using System.Linq;
using System.Security.Policy;
using Inkybot.Dofus.Domain;
using Inkybot.Dofus.Repositories;
using NLog;

namespace Inkybot.Dofus.Contracts
{
    public abstract class CustomDofusMagingAI : DofusMagingAI
    {
        public string Path { get; private set; } = null!;
        public MageConfig MageConfig { get; private set; } = new MageConfig(new Item(new ItemStatRepository(new ItemStat[] {})));
        private DofusMagingAI Default = null!;
        protected virtual bool ShouldPerfectStats => true;
        protected virtual bool ShouldOvermageToReachMinimum => true;
        protected virtual bool ShouldOvermageToUseRemainingSink => true;
        public virtual bool FinishAfterExoLandedButIsNotVisibleOnItem => true;
        protected NLog.Logger Log = null!;


        protected IAction ResolveDefault() =>
            Default.ResolveAction(Item);

        protected IAction ResolveDefaultExcludingStats(Stat[] excludedStats) =>
            ResolveExcludingStats(excludedStats);
        
        protected IAction ResolveDefaultExcludingStat(Stat stat) =>
            ResolveExcludingStat(stat);

        protected override IAction ResolveExcludingStats(Stat[] excludedStats) =>
            Default.ResolveActionExcludingStats(Item, excludedStats);
        
        protected IAction ResolveExcludingStat(Stat stat) =>
            ResolveExcludingStats(new []{ stat });

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

        
        public void SetPath(string scriptPath) =>
            Path = scriptPath;
        
        public void SetMageConfig(MageConfig mageConfig) =>
            MageConfig = mageConfig;

        public void SetLogger(Logger customLogger) =>
            Log = customLogger;
    }
}
