using SteamKit2;
using System;
using System.Collections.Generic;

namespace SteamBot
{
    class Command
    {
        public static void Commands()
        {
            while (true)
            {
                Console.Write(": ");
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
                }
            }
        }
    }
}