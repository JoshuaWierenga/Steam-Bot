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
                    case "groups":
                    case "group chats":
                    case "chats":
                        string[] file = File.ReadAllLines("groupList.txt");

                        foreach (var line in file)
                        {
                            string[] seperatedLine = SteamBot.seperate(1, '✏', line);
                            Console.WriteLine(SteamBot.steamFriends.GetClanName(Convert.ToUInt64(seperatedLine[1])) + "(" + seperatedLine[1] + ")");
                        }
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
                                if (seperatedLine[0] == chat || seperatedLine[1] == chat)
                                {

                                }
                            }


                        }
                        break;
                    case "admins":
                        string[] file3 = File.ReadAllLines("admin.txt");
                        foreach (var line in file3)
                        {
                            Console.WriteLine(SteamBot.steamFriends.GetFriendPersonaName(Convert.ToUInt64(line)) + "(" + line + ")");
                            
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