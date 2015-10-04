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
                string command = Console.ReadLine().ToLower() ;
                switch (command)
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
                        bool id = ulong.TryParse(SteamBot.NamestosteamId(chat, "groupList.txt", 1), out clanid);

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

                    #region kick/ban
                    case "ban":
                    case "kick":
                        Console.Write("What group is the user in: ");

                        string group = Console.ReadLine();

                        Console.WriteLine("You can use steamidfinder.com/ to get id");
                        Console.Write("What is the users id: ");

                        string user2 = Console.ReadLine();

                        ulong userid;
                        ulong chatid2;

                        bool groupid = ulong.TryParse(SteamBot.NamestosteamId(group, "groupList.txt", 1), out chatid2);
                        bool userid2 = ulong.TryParse(user2, out userid);

                        if (groupid && userid2)
                        {
                            if (command == "kick") { SteamBot.steamFriends.KickChatMember(chatid2, userid); }
                            else if (command == "ban") { SteamBot.steamFriends.BanChatMember(chatid2, userid); };
                            
                            
                        }
                        break;
                    #endregion
                    
                    case "exit":
                    case "quit":
                        Environment.Exit(1);
                        break;
                }
            }
        }
    }
}