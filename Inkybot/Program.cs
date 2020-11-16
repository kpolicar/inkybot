using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using Inkybot.Api;
using Inkybot.Contracts;
using Inkybot.Domain;
using Inkybot.Services;
using DofusMagingJob = Inkybot.Domain.DofusMagingJob;
using DofusMagingJobContract = Inkybot.Contracts.DofusMagingJob;

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
        public const string VersionNumber = "8";
        public const string Version = "v0.8 Beta";
        public const string VersionEndpoint = "v0.8beta";
        

        public static ServiceContainer Services = new ServiceContainer();
        public static CultureInfo Lang;
        

        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main() {
            SetAppLocale();
            Stat.Init();
            
            Services.AddService(typeof(MageConfig), new SettingsMageConfig());
            Services.AddService(typeof(DofusDataProvider), new ScreenReaderDataProvider());
            Services.AddService(typeof(ScreenCapture), new Win32ScreenCapture());
            Services.AddService(typeof(Input), new Win32Input());
            Services.AddService(typeof(ActionFactory), new MouseActionFactory());
            Services.AddService(typeof(ConfigManager), new ConfigManager());
            Services.AddService(typeof(ActionHandler), new ActionHandler());
            Services.AddService(typeof(IItemHistoryAnalyzer), new ItemHistoryAnalyzer());
            Services.AddService(typeof(ApiClient), new ApiClient());
            Services.AddService(typeof(DofusMagingJobContract), new DofusMagingJob());
            Services.AddService(typeof(DofusMagingAI), new BasicDofusMagingAI());
            
            BindNotifications();
            BindLogger();
            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new OcrDebugForm());
            //Application.Run(new MainForm());
            
        }
        
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
