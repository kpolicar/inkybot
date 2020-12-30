using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows.Forms;
using Inkybot.Api;
using Inkybot.Contracts;
using Inkybot.Design;
using Inkybot.Domain;
using Inkybot.Domain.Repositories;
using Inkybot.Properties;
using Inkybot.Services;
using Newtonsoft.Json;
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
        public const string Version = "1";
        public const string VersionEndpoint = "v1";
        

        public static ServiceContainer Services = new ServiceContainer();
        public static CultureInfo Lang;
        
        public static Dictionary<Type, object> _services = new Dictionary<Type, object> {
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
            Stat.Init();

            BindServices();
            BindNotifications();
            BindLogger();
            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new OcrDebugForm());
            Application.Run(new MainForm());
            //Print();
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

        #if DEBUG
        private static void Print() {
            var StatDictionary = new ResourceManager("Inkybot.Resources.StatDictionary", Assembly.GetExecutingAssembly())
                .GetResourceSet(CultureInfo.CurrentUICulture, true, true);
            var MagingDictionary = new ResourceManager("Inkybot.Resources.MagingDictionary", Assembly.GetExecutingAssembly())
                .GetResourceSet(CultureInfo.CurrentUICulture, true, true);

            var words = new List<string>();

            foreach (DictionaryEntry dictionaryEntry in StatDictionary) {
                foreach (var word in dictionaryEntry.Value.ToString().Split(' ')) {
                    if (words.Contains(word)) continue;
                    words.Add(word);
                    Debug.WriteLine(word);
                }
            }
            foreach (DictionaryEntry dictionaryEntry in MagingDictionary) {
                foreach (var word in dictionaryEntry.Value.ToString().Split(' ')) {
                    if (words.Contains(word)) continue;
                    words.Add(word);
                    Debug.WriteLine(word);
                }
            }
        }
        #endif
        
        private static void SetAppLocale() {
            if (Properties.Settings.Default.locale == Properties.Resources.FrenchLocaleCode) {
                Lang =
                    Thread.CurrentThread.CurrentUICulture =
                        CultureInfo.CurrentUICulture =
                            CultureInfo.DefaultThreadCurrentCulture =
                                new CultureInfo(Properties.Resources.FrenchLocaleCode);
            } else {
                Lang =
                    Thread.CurrentThread.CurrentUICulture =
                        CultureInfo.CurrentUICulture =
                            CultureInfo.DefaultThreadCurrentCulture =
                                new CultureInfo(Properties.Resources.EnglishLocaleCode);
            }
            Properties.Resources.Culture = Lang;
            Properties.Regex.Culture = Lang;
            Resources.MagingDictionary.Culture = Lang;
            Resources.RuneDictionary.Culture = Lang;
            Resources.StatDictionary.Culture = Lang;
        }

        private static void BindNotifications() {
            var actions = (ActionHandler) Services.GetService(typeof(ActionHandler));
            var magingJob = (DofusMagingJobContract) Services.GetService(typeof(DofusMagingJobContract));
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
