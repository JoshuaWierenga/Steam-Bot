using System;
using System.Windows.Forms;
using System.Threading;


namespace FormConsole
{

    static class Program
    {
        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Commands();
        }

        public static void Commands()
        {

            Bot_Control_Panel tf = new Bot_Control_Panel();
            Thread Bot = new Thread(new ThreadStart(SteamBot.MainBot));
            Thread ct = new Thread(
                new ThreadStart(
                    delegate()
                    {
                        while (true)
                        {
                            string command = Console.ReadLine();
                            switch (command.ToLower())
                            {
                                case "startbot":
                                    Bot.Start();
                                    tf.Show();
                                    tf.Activate();
                                    Application.Run(tf);
                                    Application.Restart();
                                    break;
                                case "close":
                                case "quit":
                                    Environment.Exit(0);
                                    break;
                            }
                        }
                    }));
            ct.Start();

        }
    }
}
