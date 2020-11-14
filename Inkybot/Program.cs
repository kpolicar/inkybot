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
using Inkybot.Domain;
using Inkybot.Events;
using Inkybot.Services;
using static System.Configuration.ConfigurationManager;
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
        public const string VersionNumber = "7";
        public const string Version = "v0.7 Beta";
        public const string VersionEndpoint = "v0.7beta";
        

        public static ServiceContainer Services = new ServiceContainer();
        

        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main() {
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
            //Application.Run(new OcrDebugForm());
            Application.Run(new MainForm());
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
