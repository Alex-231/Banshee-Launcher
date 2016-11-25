using System;
using System.Windows.Forms;

namespace MinecraftModLauncher
{
    static class Program
    {
        //I bet the version goes here...

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
