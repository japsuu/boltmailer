using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Boltmailer_client
{
    static class Program
    {
        public static string LastCreatedLockfilePath;

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(HandleUncaught);
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new GeneralOverview());

            bool configCompleted = bool.Parse(System.Configuration.ConfigurationManager.AppSettings.Get("setupCompleted"));

            if (configCompleted)
            {
                Application.Run(new GeneralOverview());
            }
            else
            {
                Application.Run(new Setup());
            }

        }

        static void HandleUncaught(object sender, UnhandledExceptionEventArgs args)
        {
            if (File.Exists(LastCreatedLockfilePath))
            {
                File.Delete(LastCreatedLockfilePath);
            }
        }
    }
}
