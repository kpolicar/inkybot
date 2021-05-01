using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Resources;
using Inkybot;
using Inkybot.Api;
using Inkybot.Contracts;
using Inkybot.Dofus;
using Inkybot.Dofus.Contracts;
using Inkybot.Services;
using Tests.Services;
using DofusMagingAI = Inkybot.Services.DofusMagingAI;
using DofusMagingJobContract = Inkybot.Contracts.DofusMagingJob;
using DofusMagingAIContract = Inkybot.Dofus.Contracts.DofusMagingAI;

namespace Tests.Design
{
    public abstract class MagingAITest : Test
    {
        protected ConfigManager Config =>
            (ConfigManager) ServiceContainer.GetService<MageConfigManager>();
        protected DofusMagingAI AI =>
            (DofusMagingAI) ServiceContainer.GetService<DofusMagingAIContract>();
        protected MagingJobMock Job =>
            (MagingJobMock) ServiceContainer.GetService<DofusMagingJobContract>();

        protected override Dictionary<Type, object> Services() {
            Stat.Dictionary = new ResourceManager("Tests.Resources.StatDictionary", Assembly.GetExecutingAssembly())
                .GetResourceSet(new CultureInfo("en"), true, true);
            Rune.Dictionary = new ResourceManager("Tests.Resources.RuneDictionary", Assembly.GetExecutingAssembly())
                .GetResourceSet(new CultureInfo("en"), true, true);

            var magingJob = new MagingJobMock();
            return new Dictionary<Type, object> {
                { typeof(ActionFactory), new MouseActionFactory() },
                { typeof(DofusDataProvider), new ScreenReaderDataProvider() },
                { typeof(ScreenCapture), new Win32ScreenCapture() },
                { typeof(MageConfigManager), new ConfigManager() },
                { typeof(DofusMagingJobContract), magingJob },
                { typeof(DofusSinkProvider), magingJob },
                { typeof(DofusMagingAIContract), new DofusMagingAI() },
            };
        }
    }
}
