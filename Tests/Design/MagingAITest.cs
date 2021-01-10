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

        protected override Dictionary<Type, object> Services() {
            return new Dictionary<Type, object> {
                { typeof(DofusDataProvider), new ScreenReaderDataProvider() },
                { typeof(ScreenCapture), new Win32ScreenCapture() },
                { typeof(ConfigManager), new ConfigManager() },
                { typeof(IItemHistoryAnalyzer), new ItemHistoryAnalyzer() },
                { typeof(DofusMagingJobContract), new ScreenReaderDofusMagingJob() },
                { typeof(DofusMagingAIContract), new DofusMagingAI() },
            };
        }
    }
}
