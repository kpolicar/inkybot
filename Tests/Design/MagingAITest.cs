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
using DofusMagingAI = Inkybot.Services.DofusMagingAI;
using DofusMagingJobContract = Inkybot.Contracts.DofusMagingJob;
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
            Stat.Dictionary = new ResourceManager("Tests.Resources.StatDictionary", Assembly.GetExecutingAssembly())
                .GetResourceSet(new CultureInfo("en"), true, true);
            Rune.Dictionary = new ResourceManager("Tests.Resources.RuneDictionary", Assembly.GetExecutingAssembly())
                .GetResourceSet(new CultureInfo("en"), true, true);
            
            return new Dictionary<Type, object> {
                { typeof(ActionFactory), new MouseActionFactory() },
                { typeof(DofusDataProvider), new ScreenReaderDataProvider() },
                { typeof(ScreenCapture), new Win32ScreenCapture() },
                { typeof(ConfigManager), new ConfigManager() },
                { typeof(DofusMagingJobContract), new ScreenReaderDofusMagingJob() },
                { typeof(DofusMagingAIContract), new DofusMagingAI() },
            };
        }
    }
}
