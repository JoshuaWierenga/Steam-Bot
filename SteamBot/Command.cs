using SteamKit2;
using System;
using System.Collections.Generic;
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
                    #region log groups                      
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
                    #endregion

                    #region log users
                    case "users":
                    case "chat users":
                        Console.Write("log users for what group chat:");

                        string chat = Console.ReadLine();
                        ulong clanid;
                        ulong chatid = 0;
                        bool id = UInt64.TryParse(SteamBot.NamestosteamId(chat, "groupList.txt", 1), out clanid);

                        if (id)
                        {
                            try
                            {
                                foreach (KeyValuePair<SteamID, SteamID> ids in SteamBot.chatclanid)
                                {
                                    if (ids.Value == clanid)
                                    {
                                        chatid = ids.Key;
                                    }
                                }

                                foreach (KeyValuePair<SteamID, EClanPermission> user in SteamBot.chatusers[chatid])
                                {
                                    Console.WriteLine(SteamBot.steamFriends.GetFriendPersonaName(user.Key) + " : " + user.Value);
                                }
                            }
                            catch(Exception)
                            {

                            }
                            
                        }                      
                        break;
                    #endregion

                    #region log bot admins
                    case "admins":
                        string[] file3 = File.ReadAllLines("admin.txt");
                        foreach (var line in file3)
                        {
                            Console.WriteLine(SteamBot.steamFriends.GetFriendPersonaName(Convert.ToUInt64(line)) + "(" + line + ")");
                            
                        }
                        break;

                    #endregion

                    #region
                    case "exit":
                    case "quit":
                        Environment.Exit(1);
                        break;
                }
            }
        }
    }
}