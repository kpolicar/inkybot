using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsFormsApp.Contracts;
using WindowsFormsApp.Services;
using Mouse = WindowsFormsApp.Contracts.Mouse;

namespace WindowsFormsApp
{
  static class Program
  {
    public static bool debug = false;
    public static ServiceContainer Services = new ServiceContainer();

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main() {
      Services.AddService(typeof(ScreenCapture), new Win32ScreenCapture());
      Services.AddService(typeof(Mouse), new Win32Mouse());
      Services.AddService(typeof(ActionFactory), new MouseActionFactory());
      Services.AddService(typeof(ActionHandler), new ActionHandler());
      Services.AddService(typeof(DofusMagingAI), new BasicDofusMagingAI());
      Services.AddService(typeof(IItemHistoryAnalyzer), new ItemHistoryAnalyzer());
      Services.AddService(typeof(DofusMagingJob), new DofusMagingJob());
      Services.AddService(typeof(Auth), new Auth());

      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      Application.Run(new MainForm());
    }
  }
}
