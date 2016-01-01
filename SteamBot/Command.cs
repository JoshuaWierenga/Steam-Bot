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
                Console.Write("command console: ");
                string command = Console.ReadLine().ToLower() ;
                switch (command)
                {
                    #region log users
                    case "users":
                    case "chat users":
                        foreach (KeyValuePair<SteamID, EClanPermission> chatuser in SteamBot.chatusers)
                        {
                            Console.WriteLine(SteamBot.steamFriends.GetFriendPersonaName(chatuser.Key) + " : " + chatuser.Value);
                        }                    
                        break;
                    #endregion

                    #region kick/ban
                    case "ban":
                    case "kick":
                        Console.Write("What is the users name: ");

                        string user = Console.ReadLine();

                        ulong userid = 0;

                        foreach (KeyValuePair<SteamID, EClanPermission> chatuser in SteamBot.chatusers)
                        {
                            if (SteamBot.steamFriends.GetFriendPersonaName(chatuser.Key) == user)
                            {
                                userid = chatuser.Key;
                            }
                        }

                        if (command == "kick") { SteamBot.kickban(0, userid, ); }
                        else if (command == "ban") { SteamBot.kickban(1, userid, ); };
                        break;
                    #endregion

                    case "id":
                        Console.WriteLine(SteamBot.steamUser.SteamID);
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