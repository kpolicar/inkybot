using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows.Forms;
using ImageMagick;
using Inkybot.Adapters;
using Inkybot.Api;
using Inkybot.Contracts;
using Inkybot.Design;
using Inkybot.Dofus;
using Inkybot.Dofus.Contracts;
using Inkybot.Domain;
using Inkybot.Properties;
using Inkybot.Services;
using Microsoft.CSharp;
using Newtonsoft.Json;
using DofusMagingAI = Inkybot.Services.DofusMagingAI;
using DofusMagingJobContract = Inkybot.Contracts.DofusMagingJob;
using ServiceContainer = Inkybot.Design.ServiceContainer;
using DofusMagingAIContract = Inkybot.Dofus.Contracts.DofusMagingAI;
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
            public const string Url = "http://inkybot.test";
            public const string GrantId = "2";
            public const string GrantSecret = "***REMOVED***";
        #else
            public const string Url = "https://inkybot.me";
            public const string GrantId = "2";
            public const string GrantSecret = "***REMOVED***";
        #endif
        
        public const string VersionNumber = "20";
        public const string Version = "v2.1";
        public const string VersionEndpoint = "v2.1";

        public static string InstanceIdentifier {
            private set;
            get;
        } = null!;
        

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
            { typeof(MageConfigManager), new ConfigManager() },
            { typeof(ActionHandler), new ActionHandler() },
            { typeof(ApiClient), new ApiClient() },
            { typeof(MagingAIServiceManager), new MagingAIServiceManager() },
            { typeof(ApiNotifier), new ApiNotifier() },
            { typeof(MageQueueManager), new MageQueueManager() },
        };
        
        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main() {
            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            InstanceIdentifier = "instance-" + new string(Enumerable.Repeat(chars, 16)
                .Select(s => s[random.Next(s.Length)]).ToArray());
            
            ApplyAdditionalUserSettings();
            SetAppLocale();
            InitDependencies();
                
            BindServices();
            BindLogger();
            
            Measurements.BindDependencies(Services);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            // Application.Run(new ConfigForm(new StatsForm()));
            Application.Run(new MainForm());
        }

        private static void ApplyAdditionalUserSettings() {
            OpenCL.IsEnabled = !Settings.Default.DisableOpenCL;
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
            _services[typeof(DofusSinkProvider)] = _services[typeof(DofusMagingJobContract)];
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
