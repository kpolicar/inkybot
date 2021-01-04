using System;
using System.Collections.Generic;
using Inkybot;
using Inkybot.Api;
using Inkybot.Contracts;
using Inkybot.Services;
using DofusMagingAI = Inkybot.Services.DofusMagingAI;
using DofusMagingJobContract = Inkybot.Contracts.DofusMagingJob;
using StatisticsManager = Inkybot.Services.StatisticsManager;
using StatisticsManagerContract = Inkybot.Contracts.StatisticsManager;
using DofusMagingAIContract = Inkybot.Contracts.DofusMagingAI;

namespace Tests.Design
{
    public abstract class MagingAITest : Test
    {
        protected ConfigManager Config =>
            ServiceContainer.GetService<ConfigManager>();
        protected DofusMagingAI AI =>
            (DofusMagingAI) ServiceContainer.GetService<DofusMagingAIContract>();

        protected override void Init() {
            Stat.Init();
            base.Init();
        }

        protected override Dictionary<Type, object> Services() {
            return new Dictionary<Type, object> {
                {typeof(MageConfig), new SettingsMageConfig()},
                {typeof(DofusDataProvider), new ScreenReaderDataProvider()},
                {typeof(ScreenCapture), new Win32ScreenCapture()},
                {typeof(Input), new Win32Input()},
                {typeof(ActionFactory), new MouseActionFactory()},
                {typeof(ConfigManager), new ConfigManager()},
                {typeof(ActionHandler), new ActionHandler()},
                {typeof(IItemHistoryAnalyzer), new ItemHistoryAnalyzer()},
                {typeof(ApiClient), new ApiClient()},
                {typeof(AuthManager), new AuthManager()},
                {typeof(DofusMagingJobContract), new ScreenReaderDofusMagingJob()},
                {typeof(DofusMagingAIContract), new DofusMagingAI()},
                {typeof(StatisticsManagerContract), new StatisticsManager()},
                {typeof(MagingAIServiceManager), new MagingAIServiceManager()},
            };
        }
    }
}
