using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows.Forms;
using Inkybot.Api;
using Inkybot.Contracts;
using Inkybot.Design;
using Inkybot.Dofus;
using Inkybot.Dofus.Contracts;
using Inkybot.Domain;
using Inkybot.Properties;
using Inkybot.Services;
using Microsoft.CSharp;
using DofusMagingAI = Inkybot.Services.DofusMagingAI;
using DofusMagingJobContract = Inkybot.Contracts.DofusMagingJob;
using ServiceContainer = Inkybot.Design.ServiceContainer;
using DofusMagingAIContract = Inkybot.Contracts.DofusMagingAI;
using MageConfigProvider = Inkybot.Services.MageConfigProvider;
using StatConfigProvider = Inkybot.Services.StatConfigProvider;
using StatConfigProviderContract = Inkybot.Dofus.Contracts.StatConfigProvider;
using MageConfigProviderContract = Inkybot.Dofus.Contracts.MageConfigProvider;
using UserSettings = Inkybot.Properties.Settings;

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
        public const string VersionNumber = "15";
        public const string Version = "v1.2";
        public const string VersionEndpoint = "v1.2";
        

        public static ServiceContainer Services = new ServiceContainer();
        public static CultureInfo Lang = null!;
        private static string DefaultLocale => Properties.Resources.EnglishLocaleCode;
        
        public static readonly Dictionary<Type, object> _services = new Dictionary<Type, object> {
            { typeof(DofusDataProvider), new ScreenReaderDataProvider() },
            { typeof(ScreenCapture), new Win32ScreenCapture() },
            { typeof(Input), new Win32Input() },
            { typeof(ActionFactory), new MouseActionFactory() },
            { typeof(AuthManager), new ApiAuthManager() },
            { typeof(DofusMagingJobContract), new ScreenReaderDofusMagingJob() },
            { typeof(DofusMagingAIContract), new DofusMagingAI() },
            { typeof(StatConfigProviderContract), new StatConfigProvider() },
            { typeof(MageConfigProviderContract), new MageConfigProvider() },
            { typeof(UserSettingsConfigManager), new FileSystemUserSettingsConfigManager() },
            { typeof(AnalyticsReporter), new ApiAnalyticsReporter() },
            { typeof(ConfigManager), new ConfigManager() },
            { typeof(ActionHandler), new ActionHandler() },
            { typeof(ApiClient), new ApiClient() },
            { typeof(MagingAIServiceManager), new MagingAIServiceManager() },
            { typeof(ApiNotifier), new ApiNotifier() },
        };
        
        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main() {
            UpgradeApp();
            SetAppLocale();
            InitDependencies();
                
            BindServices();
            BindLogger();
            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ConfigForm());
        }

        public static void UpgradeApp() {
            if (!Settings.Default.UpgradeRequired) return;
            
            // Settings.Default.Upgrade();
            // Settings.Default.Reload();
            Settings.Default.UpgradeRequired = false;
            Settings.Default.Save();
        }

        private static void InitDependencies() {
            MageConfig.ConfigManager = (MageConfigProviderContract) _services[typeof(MageConfigProviderContract)];
            Stat.ConfigManager = (StatConfigProviderContract) _services[typeof(StatConfigProviderContract)];
            Stat.Dictionary = new ResourceManager("Inkybot.Resources.StatDictionary", Assembly.GetExecutingAssembly())
                .GetResourceSet(Lang, true, true);
            Rune.Dictionary = new ResourceManager("Inkybot.Resources.RuneDictionary", Assembly.GetExecutingAssembly())
                .GetResourceSet(Lang, true, true);
        }

        private static void BindServices() {
            foreach (var serviceBinding in _services) {
                var @abstract = serviceBinding.Key;
                var concrete = serviceBinding.Value;
                Services.AddService(@abstract, concrete);
            }
            foreach (var serviceBinding in _services) {
                var concrete = serviceBinding.Value;
                if (concrete is HasDependencies service) {
                    service.BindDependencies(Services);
                }
            }
        }

        private static void SetAppLocale() {
            var gameSettings = DetectUserGame.ReadSettings();
            var locale =
                gameSettings != null
                    && DetectUserGame.HasValidAndSupportedLanguage(gameSettings)
                    && UserSettings.Default.locale == ""
                ? gameSettings.language.value
                : UserSettings.Default.locale ?? DefaultLocale;

            var culture = locale == Properties.Resources.FrenchLocaleCode
                ? new CultureInfo(Properties.Resources.FrenchLocaleCode)
                : new CultureInfo(Properties.Resources.EnglishLocaleCode);
            Lang =
                Thread.CurrentThread.CurrentCulture =
                Thread.CurrentThread.CurrentUICulture =
                CultureInfo.CurrentCulture = 
                CultureInfo.CurrentUICulture =
                CultureInfo.DefaultThreadCurrentCulture =
                CultureInfo.DefaultThreadCurrentUICulture =
                    culture;
            Properties.Resources.Culture = Lang;
            Properties.Regex.Culture = Lang;
            Resources.MagingDictionary.Culture = Lang;
            Resources.RuneDictionary.Culture = Lang;
            Resources.StatDictionary.Culture = Lang;
        }

        private static void BindLogger() {
            new FileEventLogger().BindToServices();
        }
    }
}
