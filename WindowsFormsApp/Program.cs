using System;
using System.ComponentModel.Design;
using System.Windows.Forms;
using WindowsFormsApp.Contracts;
using WindowsFormsApp.Services;
using ApiDataProvider = WindowsFormsApp.Api.ApiDataProvider;

namespace WindowsFormsApp
{
    internal static class Program
    {
        public static bool debug = true;
        public static ServiceContainer Services = new ServiceContainer();

        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main() {
            Services.AddService(typeof(ScreenCapture), new Win32ScreenCapture());
            Services.AddService(typeof(Mouse), new Win32Mouse());
            Services.AddService(typeof(ActionFactory), new MouseActionFactory());
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
