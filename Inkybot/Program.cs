using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using Inkybot.Api;
using Inkybot.Contracts;
using Inkybot.Design;
using Inkybot.Domain;
using Inkybot.Properties;
using Inkybot.Services;
using Microsoft.CSharp;
using DofusMagingAI = Inkybot.Services.DofusMagingAI;
using DofusMagingJobContract = Inkybot.Contracts.DofusMagingJob;
using ServiceContainer = Inkybot.Design.ServiceContainer;
using StatisticsManager = Inkybot.Services.StatisticsManager;
using StatisticsManagerContract = Inkybot.Contracts.StatisticsManager;
using DofusMagingAIContract = Inkybot.Contracts.DofusMagingAI;

namespace Inkybot
{
    internal static class Program
    {
        
        #if DEBUG
            public const string Url = "http://inkybot-server.test";
            public const string GrantId = "2";
            public const string GrantSecret = "***REMOVED***";
        #else
            public const string Url = "https://inkybot.me";
            public const string GrantId = "2";
            public const string GrantSecret = "***REMOVED***";
        #endif
        public const string VersionNumber = "13";
        public const string Version = "v1.0";
        public const string VersionEndpoint = "v1";
        

        public static ServiceContainer Services = new ServiceContainer();
        public static CultureInfo Lang = null!;
        
        public static readonly Dictionary<Type, object> _services = new Dictionary<Type, object> {
            { typeof(MageConfig), new SettingsMageConfig() },
            { typeof(DofusDataProvider), new ScreenReaderDataProvider() },
            { typeof(ScreenCapture), new Win32ScreenCapture() },
            { typeof(Input), new Win32Input() },
            { typeof(ActionFactory), new MouseActionFactory() },
            { typeof(ConfigManager), new ConfigManager() },
            { typeof(ActionHandler), new ActionHandler() },
            { typeof(IItemHistoryAnalyzer), new ItemHistoryAnalyzer() },
            { typeof(ApiClient), new ApiClient() },
            { typeof(AuthManager), new AuthManager() },
            { typeof(DofusMagingJobContract), new ScreenReaderDofusMagingJob() },
            { typeof(DofusMagingAIContract), new DofusMagingAI() },
            { typeof(DefaultConfigProvider), new UserSettingsDefaultConfigProvider() },
            { typeof(StatisticsManagerContract), new StatisticsManager() },
            { typeof(MagingAIServiceManager), new MagingAIServiceManager() },
        };

        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main() {
            if (Settings.Default.UpgradeRequired)
            {
                Settings.Default.Upgrade();
                Settings.Default.Reload();
                Settings.Default.UpgradeRequired = false;
                Settings.Default.Save();
            }
            SetAppLocale();

            BindServices();
            BindNotifications();
            BindLogger();
            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
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

        private static void SetAppLocale() {
            if (Properties.Settings.Default.locale == Properties.Resources.FrenchLocaleCode) {
                Lang =
                    Thread.CurrentThread.CurrentUICulture =
                        CultureInfo.CurrentUICulture =
                            CultureInfo.DefaultThreadCurrentCulture =
                                CultureInfo.DefaultThreadCurrentUICulture =
                                    new CultureInfo(Properties.Resources.FrenchLocaleCode);
            } else {
                Lang =
                    Thread.CurrentThread.CurrentUICulture =
                        CultureInfo.CurrentUICulture =
                            CultureInfo.DefaultThreadCurrentCulture =
                                CultureInfo.DefaultThreadCurrentUICulture =
                                new CultureInfo(Properties.Resources.EnglishLocaleCode);
            }
            Properties.Resources.Culture = Lang;
            Properties.Regex.Culture = Lang;
            Resources.MagingDictionary.Culture = Lang;
            Resources.RuneDictionary.Culture = Lang;
            Resources.StatDictionary.Culture = Lang;
        }

        private static void BindNotifications() {
            var actions = Services.GetService<ActionHandler>();
            var magingJob = Services.GetService<DofusMagingJobContract>();
            var notified = new[] { new ApiNotifier() };

            foreach (var notifier in notified) {
                actions!.ActionExecuted += notifier.Notify;
                magingJob!.Error += notifier.Notify;
            }
        }

        private static void BindLogger() {
            new FileEventLogger().BindToServices();
        }
    }
}
