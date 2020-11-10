using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Security.Principal;
using System.Threading;
using System.Windows.Forms;
using Inkybot.Api;
using Inkybot.Contracts;
using Inkybot.Events;
using Inkybot.Services;
using static System.Configuration.ConfigurationManager;

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
        public const string VersionNumber = "5";
        public const string Version = "v0.5 Beta";
        public const string VersionEndpoint = "v0.5beta";
        

        public static ServiceContainer Services = new ServiceContainer();
        

        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main() {
            Services.AddService(typeof(DofusDataProvider), new ScreenReaderDataProvider());
            Services.AddService(typeof(ScreenCapture), new Win32ScreenCapture());
            Services.AddService(typeof(Input), new Win32Input());
            Services.AddService(typeof(ActionFactory), new MouseActionFactory());
            Services.AddService(typeof(ConfigManager), new ConfigManager());
            Services.AddService(typeof(ActionHandler), new ActionHandler());
            Services.AddService(typeof(IItemHistoryAnalyzer), new ItemHistoryAnalyzer());
            Services.AddService(typeof(ApiClient), new ApiClient());
            Services.AddService(typeof(DofusMagingJob), new DofusMagingJob());
            Services.AddService(typeof(DofusMagingAI), new BasicDofusMagingAI());

            BindNotifications();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new OcrDebugForm());
            //Application.Run(new MainForm());

            var rm = new ResourceManager("Inkybot.Resources.StatDictionary", Assembly.GetExecutingAssembly());
            
            var resourceSet =
                rm.GetResourceSet(CultureInfo.CurrentUICulture, true, true);

            foreach (DictionaryEntry entry in resourceSet)
            {
                var stat = entry.Key.ToString();
                    

                var sa = (Inkybot.Config.StatConfig) Properties.Settings.Default["_" + stat.Replace("%", "per_")];
                Debug.WriteLine(stat + " " + sa.MaxValueSmRuneCanHit);
            }
        }

        private static void BindNotifications() {
            var actions = (ActionHandler) Services.GetService(typeof(ActionHandler));
            var magingJob = (DofusMagingJob) Services.GetService(typeof(DofusMagingJob));
            var notified = new[] { new ApiNotifier() };

            foreach (var notifier in notified) {
                actions!.ActionExecuted += notifier.Notify;
                magingJob!.Error += notifier.Notify;
            }
        }
    }
}
