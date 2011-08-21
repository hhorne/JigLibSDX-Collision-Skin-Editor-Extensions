using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace JigLibSDX_CSE
{
    static class Program
    {
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            SplashScreen splash = new SplashScreen();
            splash.ShowDialog();

            Application.Run(new Main());

            splash = null;
        }
    }
}
