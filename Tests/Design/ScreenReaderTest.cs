using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using Inkybot;
using Inkybot.Api;
using Inkybot.Contracts;
using Inkybot.Design;
using Inkybot.Domain;
using Inkybot.Services;
using NUnit.Framework;

using DofusMagingAI = Inkybot.Services.DofusMagingAI;
using DofusMagingJobContract = Inkybot.Contracts.DofusMagingJob;
using ServiceContainer = Inkybot.Design.ServiceContainer;
using StatisticsManager = Inkybot.Services.StatisticsManager;
using StatisticsManagerContract = Inkybot.Contracts.StatisticsManager;
using DofusMagingAIContract = Inkybot.Contracts.DofusMagingAI;

namespace Tests.Design
{
    public abstract class ScreenReaderTest
    {
        protected static Dictionary<Type, object> _services = new Dictionary<Type, object> {
            { typeof(MageConfig), new SettingsMageConfig() },
            { typeof(DofusDataProvider), new ScreenReaderDataProvider() },
            { typeof(Input), new Win32Input() },
            { typeof(ActionFactory), new MouseActionFactory() },
            { typeof(ConfigManager), new ConfigManager() },
            { typeof(ActionHandler), new ActionHandler() },
            { typeof(IItemHistoryAnalyzer), new ItemHistoryAnalyzer() },
            { typeof(ApiClient), new ApiClient() },
            { typeof(DofusMagingJobContract), new ScreenReaderDofusMagingJob() },
            { typeof(DofusMagingAIContract), new DofusMagingAI() },
            { typeof(StatisticsManagerContract), new StatisticsManager() },
        };

        private static ServiceContainer Services = new ServiceContainer();

        protected ScreenReaderDataProvider DataProvider =>
            (ScreenReaderDataProvider) Services.GetService<DofusDataProvider>();

        protected ScreenReaderTest() {
            AddServices();
            Stat.Init();
            BindServices();
        }

        protected virtual void AddServices() {
        }
        
        private static void BindServices() {
            foreach (var serviceBinding in _services) {
                var @abstract = serviceBinding.Key;
                var concrete = serviceBinding.Value;
                Services.AddService(@abstract, concrete);
            }
            foreach (var serviceBinding in _services) {
                var concrete = serviceBinding.Value;
                if (concrete is InjectableService service) {
                    service.BindDependencies(Services);
                }
            }
        }
    }
}
