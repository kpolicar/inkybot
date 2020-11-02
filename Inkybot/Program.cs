#if false
#define DEBUG
#endif

using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;
using System.Web.UI.MobileControls;
using System.Windows.Forms;
using Inkybot.Api;
using Inkybot.Contracts;
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
        public const string VersionNumber = "1";
        public const string Version = "v0.1 Beta";
        public const string VersionEndpoint = "v0.1beta";
        

        public static ServiceContainer Services = new ServiceContainer();
        

        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main() {
            Services.AddService(typeof(ScreenCapture), new Win32ScreenCapture());
            Services.AddService(typeof(Mouse), new Win32Mouse());
            Services.AddService(typeof(ActionFactory), new MouseActionFactory());
            Services.AddService(typeof(ConfigManager), new ConfigManager());
            Services.AddService(typeof(ActionHandler), new ActionHandler());
            Services.AddService(typeof(DofusMagingAI), new BasicDofusMagingAI());
            Services.AddService(typeof(IItemHistoryAnalyzer), new ItemHistoryAnalyzer());
            Services.AddService(typeof(ApiDataProvider), new ApiDataProvider());
            Services.AddService(typeof(DofusMagingJob), new DofusMagingJob());

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
