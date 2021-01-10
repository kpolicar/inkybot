using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Resources;
using Inkybot;
using Inkybot.Api;
using Inkybot.Contracts;
using Inkybot.Dofus;
using Inkybot.Services;
using NUnit.Framework;
using DofusMagingAI = Inkybot.Services.DofusMagingAI;
using DofusMagingJobContract = Inkybot.Contracts.DofusMagingJob;
using StatisticsManager = Inkybot.Services.StatisticsManager;
using StatisticsManagerContract = Inkybot.Contracts.StatisticsManager;
using DofusMagingAIContract = Inkybot.Contracts.DofusMagingAI;

namespace Tests.Design
{
    public abstract class ScreenReaderTest : Test
    {
        protected ScreenReaderDataProvider DataProvider =>
            (ScreenReaderDataProvider) ServiceContainer.GetService<DofusDataProvider>();

        protected override Dictionary<Type, object> Services() {
            Stat.Dictionary = new ResourceManager("Tests.Resources.StatDictionary", Assembly.GetExecutingAssembly())
            .GetResourceSet(new CultureInfo("en"), true, true);
            
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
