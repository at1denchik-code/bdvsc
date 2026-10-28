using System;
using System.Windows.Forms;

namespace EnterpriseLinQ
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            DatabaseInitializer.Initialize();
            Application.Run(new FormMain());
        }
    }
}
