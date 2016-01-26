using System.Windows.Forms;
using System.Threading;


namespace SteamBot
{

    static class Startup
    {
        public static Gui gui = new Gui();

        public static void Main()
        {
            Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            Thread Bot = new Thread(new ThreadStart(SteamBot.Main));
            Bot.Start();
            Application.Run(gui);
        }
        public static void startconsole()
        {
            Thread Commands = new Thread(new ThreadStart(Command.Commands));
            Commands.Start();
        }
    }
}