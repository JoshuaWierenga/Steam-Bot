using System;
using System.IO;

namespace SteamBot
{
    class Command
    {
        public static void Commands()
        {
            while (true)
            {
                string command = Console.ReadLine();
                switch (command.ToLower())
                {
                    case "log":
                        Console.WriteLine("Options are: group chats and group chat users");
                        Console.Write("log what:");
                        bool Option = false;
                        while (Option == false)
                        {
                            string log = Console.ReadLine();
                            switch (log.ToLower())
                            {
                                case "groups":
                                case "group chats":
                                case "chats":
                                    string[] file = File.ReadAllLines("groupList.txt");

                                    foreach (var line in file)
                                    {
                                        string[] seperatedLine = SteamBot.seperate(1, '✏', line);
                                        Console.WriteLine(seperatedLine[0]);
                                    }
                                    Option = true;
                                    break;

                                case "users":
                                case "chat users":
                                    Console.Write("log users for what group:");
                                    bool group = false;
                                    while (group == false)
                                    {
                                        string chat = Console.ReadLine();

                                        string[] file2 = File.ReadAllLines("groupList.txt");
                                        foreach (var line in file2)
                                        {
                                            string[] seperatedLine = SteamBot.seperate(1, '✏', line);
                                            if (seperatedLine[0] == chat)
                                            {

                                            }
                                        }


                                    }
                                    break;
                            }
                        }
                        break;
                    case "exit":
                    case "quit":
                        Environment.Exit(1);
                        break;
                }
            }
        }
    }
}