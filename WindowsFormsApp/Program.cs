using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsFormsApp.Services;

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
      Services.AddService(typeof(DofusCommandIssuer), new MouseCommandIssuer());
      Services.AddService(typeof(DofusMagus), new DofusMagus());
      
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      Application.Run(new MainForm());
    }
  }
}
