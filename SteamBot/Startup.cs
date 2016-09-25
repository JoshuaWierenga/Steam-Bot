using System;
using System.Windows.Forms;

namespace SteamBot
{
    public static class Startup
    {
        /// <summary>
        /// To start the steam bot
        /// </summary>
        public static void Main()
        {
            SteamBot.SteamConnect();
        }

        /// <summary>
        /// To start the login form
        /// </summary>
        [STAThread]
        public static void LoginGui()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new LoginGui());
        }
    }
}
