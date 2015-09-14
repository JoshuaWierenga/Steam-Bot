using System.Windows.Forms;
using System.Threading;


namespace SteamBot
{

    static class Startup
    {
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Gui Gui = new Gui();
            Thread Bot = new Thread(new ThreadStart(SteamBot.Main));
            Thread Commands = new Thread(new ThreadStart(Command.Commands));
            Bot.Start();
            Commands.Start();
            Application.Run(Gui);
        }
    }
}