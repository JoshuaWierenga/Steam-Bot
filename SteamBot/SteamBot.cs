using System;
using System.Linq;
using SteamKit2;
using System.IO;
using System.Threading;
using System.Collections.Generic;

namespace SteamBot
{
    class SteamBot
    {
        static string newuser, newpass;

        static SteamClient steamClient;
        static CallbackManager manager;
        static SteamUser steamUser;
        public static SteamFriends steamFriends;

        static bool isRunning = false;

        static Dictionary<SteamID, EClanPermission> botranks = new Dictionary<SteamID, EClanPermission>();

        public static Dictionary<SteamID,Dictionary<SteamID, EClanPermission>> chatusers = new Dictionary<SteamID, Dictionary<SteamID, EClanPermission>>();

        public static Dictionary<SteamID, SteamID> chatclanid = new Dictionary<SteamID, SteamID>();

        static string authCode;

        static int LogTotal;

        public static void Main()
        {

            Console.Title = "BBBBBBOOOOOTTTTTT";
            Console.WriteLine("CTRL+C quits the program");

            if (!File.Exists("userPass.txt") || File.ReadAllLines("userPass.txt").Count() < 1)
            {
                StreamWriter Login = new StreamWriter("userPass.txt");
                Login.Close();
                Login = File.AppendText("userPass.txt");
                Console.Write("Username: ");
                newuser = Console.ReadLine();

                Console.Write("Password: ");
                newpass = Console.ReadLine();

                Login.WriteLine(newuser);
                Login.WriteLine(newpass);
                Login.Close();
            }
            SteamLogIn();
        }

        public static void SteamLogIn()
        {
            steamClient = new SteamClient();

            manager = new CallbackManager(steamClient);

            steamUser = steamClient.GetHandler<SteamUser>();

            steamFriends = steamClient.GetHandler<SteamFriends>();

            manager.Subscribe<SteamClient.ConnectedCallback>(OnConnected);
            manager.Subscribe<SteamClient.DisconnectedCallback>(OnDisconnected);
            manager.Subscribe<SteamUser.LoggedOnCallback>(OnLoggedOn);
            manager.Subscribe<SteamUser.UpdateMachineAuthCallback>(OnMachineAuth);

            manager.Subscribe<SteamUser.AccountInfoCallback>(OnAccountInfo);

            manager.Subscribe<SteamFriends.FriendMsgCallback>(OnChatMessage);
            manager.Subscribe<SteamFriends.ChatMsgCallback>(OnGroupMessage);

            manager.Subscribe<SteamFriends.FriendsListCallback>(OnFriendsList);

            manager.Subscribe<SteamFriends.ChatInviteCallback>(OnChatInvite);
            manager.Subscribe<SteamFriends.ChatEnterCallback>(OnChatEnter);

            manager.Subscribe<SteamFriends.ChatMemberInfoCallback>(OnGroupEvent);

            isRunning = true;

            Console.WriteLine("\nConnecting to Steam...\n");

            steamClient.Connect();

            isRunning = true;
            while (isRunning)
            {
                manager.RunWaitCallbacks(TimeSpan.FromSeconds(1));
            }
            Console.ReadLine();

        }

        static void OnConnected(SteamClient.ConnectedCallback callback)
        {
            if (callback.Result != EResult.OK)
            {
                Console.WriteLine("Unable to connect to Steam: {0}", callback.Result);
                isRunning = false;
                return;
            }

            string[] currentUserFile = File.ReadAllLines("userPass.txt");
            Console.WriteLine("Connected to Steam. \nLogging in {0}...\n", currentUserFile);

            byte[] sentryHash = null;

            


            if (File.Exists("sentry.bin"))
            {
                byte[] sentryFile = File.ReadAllBytes("sentry.bin");

                sentryHash = CryptoHelper.SHAHash(sentryFile);
            }

            string[] userPass = File.ReadAllLines("userPass.txt");
            steamUser.LogOn(new SteamUser.LogOnDetails
            {
                
                Username = userPass[0],
                Password = userPass[1],

                AuthCode = authCode,

                SentryFileHash = sentryHash
            });
        }

        static void OnLoggedOn(SteamUser.LoggedOnCallback callback)
        {
            if (callback.Result == EResult.AccountLogonDenied)
            {
                Console.WriteLine("This account is SteamGuard protected.");

                Console.Write("Please enter the auth code sent to your email at {0}: ", callback.EmailDomain);

                authCode = Console.ReadLine();

                return;
            }
            if (callback.Result != EResult.OK)
            {
                Console.WriteLine("Unable to log in to Steam: {0}\n", callback.Result);
                isRunning = false;
                return;
            }
            string[] user = File.ReadAllLines("userPass.txt");
            Console.WriteLine("{0} successfully Logged in!", user[0]);
            
        }

        static void OnMachineAuth(SteamUser.UpdateMachineAuthCallback callback)
        {
            Console.WriteLine("Updating Sentry file...");
            byte[] sentryHash = CryptoHelper.SHAHash(callback.Data);
            File.WriteAllBytes("sentry.bin", callback.Data);
            steamUser.SendMachineAuthResponse(new SteamUser.MachineAuthDetails
            {
                JobID = callback.JobID,
                FileName = callback.FileName,
                BytesWritten = callback.BytesToWrite,
                FileSize = callback.Data.Length,
                Offset = callback.Offset,
                Result = EResult.OK,
                LastError = 0,
                OneTimePassword = callback.OneTimePassword,
                SentryFileHash = sentryHash,
            });
            Console.WriteLine("Done.");
        }

        static void OnDisconnected(SteamClient.DisconnectedCallback callback)
        {
            string[] user = File.ReadAllLines("userPass.txt");
            Console.WriteLine("\n{0} disconnected from Steam, reconnecting in 5...\n", user[0]);

            Thread.Sleep(TimeSpan.FromSeconds(5));

            steamClient.Connect();
        }

        static void OnFriendsList(SteamFriends.FriendsListCallback callback)
        {
            Thread.Sleep(2500);
            foreach (var friend in callback.FriendList)
            {
                if (friend.Relationship == EFriendRelationship.RequestRecipient)
                {
                    var newfriend = steamFriends.GetFriendPersonaName(friend.SteamID);
                    if (newfriend == "[unknown]")
                    {
                        return;
                    }
                    steamFriends.AddFriend(friend.SteamID);
                    Thread.Sleep(500);
                    Console.WriteLine("Recived Friend Request from: " + newfriend);                   
                    steamFriends.SendChatMessage(76561198068676400, EChatEntryType.ChatMsg, "User : " + newfriend + " has added the bot");
                }
            }
        }

        static void OnAccountInfo(SteamUser.AccountInfoCallback callback)
        {
            steamFriends.SetPersonaState(EPersonaState.Online);
        }

        static void OnChatMessage(SteamFriends.FriendMsgCallback callback)
        {
            if (callback.Message.Length < 1)
            {
                return;
            }

            switch (callback.Message.ToLower())
            {
                #region Greetings
                case "hi":
                    Console.WriteLine(steamFriends.GetFriendPersonaName(callback.Sender) + " said hi");
                    steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "hi " + steamFriends.GetFriendPersonaName(callback.Sender));
                    break;
                case "hello":
                    Console.WriteLine(steamFriends.GetFriendPersonaName(callback.Sender) + " said hello");
                    steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "hello " + steamFriends.GetFriendPersonaName(callback.Sender));
                    break;
                #endregion
                default:
                    {
                        Console.WriteLine(callback.Message + " From: " + steamFriends.GetFriendPersonaName(callback.Sender));
                        break;
                    }
            }

            string[] args;
            if (callback.EntryType == EChatEntryType.ChatMsg)
            {

                if (callback.Message.Length > 1)
                {
                    if (callback.Message.Remove(1) == "!")
                    {
                        string command = callback.Message;
                        if (callback.Message.Contains(" "))
                        {
                            command = callback.Message.Remove(callback.Message.IndexOf(' '));
                        }
                        switch (command.ToLower())
                        {
                            #region send
                            case "!send":
                            case "!message":
                            case "!sendmessage":
                            case "!pm":
                                args = seperate(2, ' ', callback.Message);
                                Console.WriteLine("Recived !pm command from: " + steamFriends.GetFriendPersonaName(callback.Sender) + ", sent to: " + args[1] + ", the message was: ");
                                Console.WriteLine(args[2]);
                                if (args[0] == "-1")
                                {
                                    steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Command syntax: !send [Friend] [Message]");
                                    return;
                                }
                                for (int i = 0; i < steamFriends.GetFriendCount(); i++)
                                {
                                    SteamID friend = steamFriends.GetFriendByIndex(i);
                                    if (steamFriends.GetFriendPersonaName(friend).ToLower().Contains(args[1].ToLower()))
                                    {
                                        steamFriends.SendChatMessage(friend, EChatEntryType.ChatMsg, args[2] + " : from " + steamFriends.GetFriendPersonaName(callback.Sender));
                                        steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Message sent.");
                                        break;
                                    }
                                    else if (i == (steamFriends.GetFriendCount() - 1))
                                    {
                                        steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Error: " + args[1] + " is not a friend of the bot.");
                                    }
                                }
                                break;
                            #endregion
                            #region friends
                            case "!friends":
                            case "!listfriends":
                                Console.WriteLine("Recived !listfriends command from: " + steamFriends.GetFriendPersonaName(callback.Sender));
                                for (int i = 0; i < steamFriends.GetFriendCount(); i++)
                                {
                                    SteamID friend = steamFriends.GetFriendByIndex(i);
                                    steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Friend: " + steamFriends.GetFriendPersonaName(friend) + "  State:  " + steamFriends.GetFriendPersonaState(friend));
                                }
                                break;
                            #endregion
                            #region State
                            case "!state":
                            case "!setstate":
                                args = seperate(1, ' ', callback.Message);
                                Console.WriteLine("Recived !setstate command from: " + steamFriends.GetFriendPersonaName(callback.Sender) + ", they set state to: " + args[1]);
                                if (args[0] == "-1")
                                {
                                    steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Command syntax: !setstate [State]");
                                    return;
                                }
                                SetState(args[1]);
                                break;
                            #endregion
                            #region Name
                            case "!name":
                            case "!setname":
                                args = seperate(1, ' ', callback.Message);
                                Console.WriteLine("Recived !setname command from: " + steamFriends.GetFriendPersonaName(callback.Sender) + " they set name to: " + args[1]);
                                if (args[0] == "-1")
                                {
                                    steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Command syntax: !setname [name]");
                                    return;
                                }
                                SetName(args[1]);
                                break;
                            #endregion
                            #region Quit
                            case "!quit":
                                {
                                    Console.WriteLine("!quit commmand recived From: " + steamFriends.GetFriendPersonaName(callback.Sender));
                                    if (isUserAdmin(callback.Sender))
                                    {
                                        foreach (var user in File.ReadAllLines("admin.txt"))
                                        {
                                            steamFriends.SendChatMessage(Convert.ToUInt64(user), EChatEntryType.ChatMsg, "Bot Disconnected");
                                            Console.WriteLine("Messaged: " + Convert.ToUInt64(user));
                                        }
                                        Thread.Sleep(3000);
                                        steamUser.LogOff();
                                        Environment.Exit(1);
                                    }
                                    break;
                                }
                            #endregion
                            #region Commands
                            case "!commands":
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Commands:");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "!help  : Displays help info");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "!friends or !listfriends :  Displays list of the bots friends");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "!state or !setstate :  Sets state of bot, can be set to: online, away, busy or snooze");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "!name or !setname :  Names the bot");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "!send or !message or !sendmessage :  Sends a message to someone, must be friend of bot");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "!chatlog :  Sends the chat to you though private message, run !group first to join a chat");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "!friendrefresh :  Reloads bots internal friend list");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "!chatrefresh :  Reloads bots internal group list");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "!quit :  Turn off the bot");
                                break;
                            #endregion
                            #region chatlog
                            case "!chatlog":
                                args = seperate(2, ' ', callback.Message);
                                if (args[0] == "-1")
                                {
                                    args = seperate(1, ' ', callback.Message);
                                    if (args[1] == "reset")
                                    {
                                        StreamWriter log;
                                        log = new StreamWriter("chatRequester.txt");
                                        log.WriteLine();
                                        log.Close();
                                    }
                                    else
                                    {
                                        steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Command syntax: !chatlog [BotChatID] [True/False]");
                                    }
                                }
                                else if (args[1] == "r" || args[1] == "requiem")
                                {
                                    if (args[2] == "true")
                                    {
                                        string ChatRequester = callback.Sender.ConvertToUInt64().ToString();
                                        StreamWriter log;
                                        if (!File.Exists("ChatRequester.txt"))
                                        {
                                            log = new StreamWriter("ChatRequester.txt");
                                        }
                                        else if (File.Exists("ChatRequester.txt") && File.ReadAllLines("ChatRequester.txt").Count() == 0)
                                        {
                                            log = File.AppendText("ChatRequester.txt");
                                        }
                                        else
                                        {
                                            steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Unexpected Error : File not found");
                                            log = new StreamWriter(string.Empty);

                                        }
                                        log.WriteLine(ChatRequester);
                                        log.Close();
                                        LogTotal++;
                                    }
                                    else if (args[2] == "false")
                                    {
                                        var oldLines = System.IO.File.ReadAllLines("ChatRequester.txt");
                                        var newLines = oldLines.Where(line => !line.Contains(callback.Sender.ConvertToUInt64().ToString()));
                                        System.IO.File.WriteAllLines("ChatRequester.txt", newLines);
                                    }
                                }
                                break;
                            #endregion
                            #region Friend Refesh
                            case "!friendrefresh":
                                Nameandidsaving("friendList.txt", "friend");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg,"Bot has : " + steamFriends.GetFriendCount().ToString() + " Friends");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg,"Bot`s friend list has been reloaded, but must be restarted for changes to take effect");
                                break;
                            #endregion
                            #region Group Refesh
                            case "!grouprefresh":
                                Nameandidsaving("groupList.txt", "group");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Bot is part of : " + steamFriends.GetClanCount().ToString() + " steam groups");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Bot`s group list has been reloaded, but must be restarted for changes to take effect");
                                break;
                            #endregion
                            #region help
                            case "!help":
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Bot help:");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "This bot was made by mrjosheyhalo");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "send !commands to get a list of commands");
                                Thread.Sleep(2500);
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Version 1.2.0");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Please message mrjosheyhalo to request new features");
                                break;
                            #endregion
                            default:
                                {
                                    Console.WriteLine(callback.Message + " From: " + steamFriends.GetFriendPersonaName(callback.Sender));
                                    break;
                                }
                        }
                    }
                }
            }
        }

        static void OnChatInvite(SteamFriends.ChatInviteCallback callback)
        {
            Console.WriteLine("Bot Has been invited to " + callback.ChatRoomName + "`s group chat. (" + callback.ChatRoomID + ") by " + steamFriends.GetFriendPersonaName(callback.FriendChatID));

            switch (callback.ChatRoomID.ToString())
            {
                case "103582791433493708":
                    break;
                default:
                    steamFriends.JoinChat(callback.ChatRoomID);
                    break;
            }


        }

        static void OnChatEnter(SteamFriends.ChatEnterCallback callback)
        {
            Console.WriteLine("Bot has joined " + steamFriends.GetClanName(callback.ClanID) + "`s group chat");
            Console.WriteLine("Number of people in this chat: " + callback.NumChatMembers);
            Dictionary<SteamID, EClanPermission> chatUsers = new Dictionary<SteamID, EClanPermission>();
            for (var i = 0; i < callback.ChatMembers.Count; i++)
            {
                chatUsers.Add(callback.ChatMembers[i].SteamID, callback.ChatMembers[i].Details);
                Console.WriteLine(steamFriends.GetFriendPersonaName(callback.ChatMembers[i].SteamID) + " : " + callback.ChatMembers[i].Details);
                if (callback.ChatMembers[i].SteamID == steamClient.SteamID)
                {
                    botranks.Add(callback.ChatID, callback.ChatMembers[i].Details);
                    Console.WriteLine("Bot is " + callback.ChatMembers[i].Details + " of " + callback.ChatRoomName);
                }
            }
            chatclanid.Add(callback.ChatID, callback.ClanID);
            chatusers.Add(callback.ChatID, chatUsers);
            /*Console.WriteLine("users: ");
            foreach (KeyValuePair<SteamID, EClanPermission> user in chatusers[callback.ClanID])
            {
                Console.WriteLine(steamFriends.GetFriendPersonaName(user.Key) + " : " + user.Value);
            }*/
        }

        static void OnGroupMessage(SteamFriends.ChatMsgCallback callback)
        {
            if (File.Exists("ChatRequester.txt"))
            {
                if (File.ReadAllLines("ChatRequester.txt").Length >= 1)
                {
                    var lines = File.ReadLines("ChatRequester.txt");
                    foreach (var line in lines)
                    {
                        string currentchatuser = steamFriends.GetFriendPersonaName(callback.ChatterID);

                        if (callback.ChatRoomID == 110338190878531432)
                        {
                            //steamFriends.SendChatMessage(Convert.ToUInt64(line), EChatEntryType.ChatMsg, user + " : " + callback.Message + " from " + "Requiem>Gamers group chat");
                        }
                        else
                        {
                            steamFriends.SendChatMessage(Convert.ToUInt64(line), EChatEntryType.ChatMsg, currentchatuser + " : " + callback.Message + " from " + steamFriends.GetClanName(callback.ChatRoomID) + " (" + callback.ChatRoomID + ") group chat");
                        }
                    }
                }
            }

            switch (callback.Message.ToLower())
            {
                #region Greatings
                case "hi":
                    Console.WriteLine("hi" + " command recieved. User: " + steamFriends.GetFriendPersonaName(callback.ChatterID));
                    steamFriends.SendChatRoomMessage(callback.ChatRoomID, EChatEntryType.ChatMsg, "Hi " + steamFriends.GetFriendPersonaName(callback.ChatterID));
                    break;
                case "hello":
                    Console.WriteLine("hello" + " command recieved. User: " + steamFriends.GetFriendPersonaName(callback.ChatterID));
                    steamFriends.SendChatRoomMessage(callback.ChatRoomID, EChatEntryType.ChatMsg, "Hello " + steamFriends.GetFriendPersonaName(callback.ChatterID));
                    break;
                    #endregion
                 default:
                     string currentchatuser = steamFriends.GetFriendPersonaName(callback.ChatterID);
                     Console.WriteLine(currentchatuser + " : " + callback.Message);
                     break;
             }
        }

        static void OnGroupEvent(SteamFriends.ChatMemberInfoCallback callback)
        {
            if (callback.StateChangeInfo.StateChange == EChatMemberStateChange.Left || callback.StateChangeInfo.StateChange == EChatMemberStateChange.Entered)
            {
                Console.WriteLine(steamFriends.GetFriendPersonaName(callback.StateChangeInfo.ChatterActedBy) + " " + callback.StateChangeInfo.StateChange.ToString().ToLower() + " the Chat");
            }
            else if (callback.StateChangeInfo.StateChange == EChatMemberStateChange.Kicked || callback.StateChangeInfo.StateChange == EChatMemberStateChange.Banned)
            {
                Console.WriteLine(steamFriends.GetFriendPersonaName(callback.StateChangeInfo.ChatterActedOn) + " was " + callback.StateChangeInfo.StateChange.ToString().ToLower() + " from the Chat");
            }
            else if (callback.StateChangeInfo.StateChange == EChatMemberStateChange.Disconnected)
            {
                Console.WriteLine(steamFriends.GetFriendPersonaName(callback.StateChangeInfo.ChatterActedBy) + " " + callback.StateChangeInfo.StateChange.ToString().ToLower() + " from the Chat");
            }

            if (callback.StateChangeInfo.StateChange == EChatMemberStateChange.Left || callback.StateChangeInfo.StateChange == EChatMemberStateChange.Disconnected || callback.StateChangeInfo.StateChange == EChatMemberStateChange.Kicked || callback.StateChangeInfo.StateChange == EChatMemberStateChange.Banned)
            {

                var users = chatusers[callback.ChatRoomID];
                users.Remove(callback.StateChangeInfo.ChatterActedOn);
                chatusers.Remove(callback.ChatRoomID);
                chatusers.Add(callback.ChatRoomID, users);

                if (callback.StateChangeInfo.ChatterActedOn == steamUser.SteamID)
                {
                    botranks.Remove(callback.ChatRoomID);
                    chatusers.Remove(callback.ChatRoomID);
                    chatclanid.Remove(callback.ChatRoomID);
                }
            }
            else if (callback.StateChangeInfo.StateChange == EChatMemberStateChange.Entered)
            {
                var users = chatusers[callback.ChatRoomID];
                users.Add(callback.StateChangeInfo.ChatterActedBy, callback.StateChangeInfo.MemberInfo.Details);
                chatusers.Remove(callback.ChatRoomID);
                chatusers.Add(callback.ChatRoomID, users);
            }
            
        }

        /// <summary>
        /// Checks if user is admin of bot
        /// put steam64 ids in admin.txt to add as admin
        /// </summary>
        /// <param name="sid">The steam64 id to check</param>
        /// <returns></returns>
        public static bool isUserAdmin(SteamID sid)
        {
            foreach (var user in File.ReadAllLines("admin.txt"))
            {
                Console.WriteLine(Convert.ToUInt64(user));
                Console.WriteLine(sid.ConvertToUInt64());
                try
                {
                    if (sid.ConvertToUInt64() == Convert.ToUInt64(user))
                    {
                        return true;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    return false;
                }
            }
            steamFriends.SendChatMessage(sid, EChatEntryType.ChatMsg, "You are not a bot admin");
            Console.WriteLine(steamFriends.GetFriendPersonaName(sid) + " attempted to use an administrator command while not an administrator.");
            return false;
        }

        /// <summary>
        /// Seperates string into parts
        /// Can be using to spilt chat commands in to command and arguments
        /// </summary>
        /// <param name="number">How many parts to split string into</param>
        /// <param name="seperator">What chat to split on</param>
        /// <param name="thestring">What string to use</param>
        /// <returns></returns>
        public static string[] seperate(int number, char seperator, string thestring)
        {
            string[] returned = new string[4];

            int i = 0;

            int error = 0;

            int length = thestring.Length;

            foreach (char c in thestring)
            {
                if (i != number)
                {
                    if (error > length || number > 5)
                    {
                        returned[0] = "-1";
                        return returned;
                    }
                    else if (c == seperator)
                    {
                        returned[i] = thestring.Remove(thestring.IndexOf(c));
                        thestring = thestring.Remove(0, thestring.IndexOf(c) + 1);
                        i++;
                    }
                    error++;

                    if (error == length && i != number)
                    {
                        returned[0] = "-1";
                        return returned;
                    }
                }
                else
                {
                    returned[i] = thestring;
                }
            }
            return returned;

        }

        /// <summary>
        /// Sets the bots state.
        /// </summary>
        /// <param name="state">The state you wish to set the bot to.</param>
        public static void SetState(string state)
        {
            state = state.ToLower();
            if (state == "online")
            {
                steamFriends.SetPersonaState(EPersonaState.Online);
            }
            else if (state == "offline")
            {
                steamFriends.SetPersonaState(EPersonaState.Offline);
            }
            else if (state == "away")
            {
                steamFriends.SetPersonaState(EPersonaState.Away);
            }
            else if (state == "busy")
            {
                steamFriends.SetPersonaState(EPersonaState.Busy);
            }
            else if (state == "snooze")
            {
                steamFriends.SetPersonaState(EPersonaState.Snooze);
            }

        }

        /// <summary>
        /// Sets the bots name.
        /// </summary>
        /// <param name="name">The name you wish to set the bot to.</param>
        public static void SetName(string name)
        {
            steamFriends.SetPersonaName(name);
        }
        
        /// <summary>
        /// To join a group chat.
        /// </summary>
        /// <param name="GroupID">The id of the group.</param>
        public static void JoinGroupChat(ulong GroupID)
        {
            steamFriends.JoinChat(GroupID); 
        }

        /// <summary>
        /// Send a message in a group chat, Must have already joined the chat.
        /// </summary>
        /// <param name="GroupID">The id of the group.</param>
        /// <param name="GroupMessage">The message.</param>
        public static void GroupMessage(ulong GroupID, string GroupMessage)
        {
            steamFriends.SendChatRoomMessage(GroupID, EChatEntryType.ChatMsg, GroupMessage);
        }

        /// <summary>
        /// Saves friend and group names and ids to a file.
        /// </summary>
        /// <param name="filename">The file you wish to save the info to.</param>
        /// <param name="list">If you wish to save friend or group info</param>
        public static void Nameandidsaving(string filename, string list)
        {
            if (list == "group")
            {
                for (int i = 0; i < steamFriends.GetClanCount(); i++)
                {
                    string id = steamFriends.GetClanByIndex(i).ConvertToUInt64().ToString();
                    string name = steamFriends.GetClanName(steamFriends.GetClanByIndex(i));

                    StreamWriter file;

                    if (!File.Exists(filename))
                    {
                        file = new StreamWriter(filename);
                    }
                    else if (File.Exists(filename) && File.ReadAllLines(filename).Count() >= steamFriends.GetClanCount())
                    {
                        file = new StreamWriter(filename);
                    }
                    else if (File.Exists(filename) && File.ReadAllLines(filename).Count() == 0)
                    {
                        file = File.AppendText(filename);
                    }
                    else
                    {
                        file = File.AppendText(filename);
                    }

                    file.WriteLine(name + "✏" + id);
                    file.Close();
                }
            }
            else if (list == "friend")
            {
                for (int i = 0; i < steamFriends.GetFriendCount(); i++)
                {
                    string id = steamFriends.GetFriendByIndex(i).ConvertToUInt64().ToString();
                    string name = steamFriends.GetFriendPersonaName(steamFriends.GetFriendByIndex(i));

                    StreamWriter file;

                    if (!File.Exists(filename))
                    {
                        file = new StreamWriter(filename);
                    }
                    else if (File.Exists(filename) && File.ReadAllLines(filename).Count() >= steamFriends.GetFriendCount())
                    {
                        file = new StreamWriter(filename);
                    }
                    else if (File.Exists(filename) && File.ReadAllLines(filename).Count() == 0)
                    {
                        file = File.AppendText(filename);
                    }
                    else
                    {
                        file = File.AppendText(filename);
                    }

                    file.WriteLine(name + "✏" + id);
                    file.Close();
                }
            }
        }

        /// <summary>
        /// Converts a friend or group name to a steamid.
        /// </summary>
        /// <param name="name">The name to convert.</param>
        /// <param name="file">The file which contains the names and ids, Use Nameandidsaving to create one.</param>
        /// <param name="kindofid">If the name is a friend or group name, 0 for friend and 1 for group</param>
        /// <returns></returns>
        public static string NamestosteamId(string name, string file, int kindofid)
        {
            if (File.Exists(file))
            {
                string[] Lines = File.ReadAllLines(file);

                foreach (var line in Lines)
                {
                    string[] seperatedLine = seperate(1, '✏', line);

                    SteamID steamid = Convert.ToUInt64(seperatedLine[1]);
                    var steamname = " ";
                    if (kindofid == 0)
                    {
                        steamname = steamFriends.GetFriendPersonaName(steamid);
                    }
                    else if(kindofid == 1)
                    {
                        steamname = steamFriends.GetClanName(steamid);
                    }
                    else
                    {
                        return "Can only convert friends and groups to id";
                    }

                    if (steamname == name)
                    {
                        return seperatedLine[1];
                    }
                }
                return "Friend/Group can not be found: " + name;
            }
            return "Can't find file";
        }

        /// <summary>
        /// Private messages a user, User must be friend of the bot.
        /// </summary>
        /// <param name="SteamID">The id of the friend.</param>
        /// <param name="Message">The message.</param>
        public static void PrivateMessage(ulong SteamID, string Message)
        {
            steamFriends.SendChatMessage(Convert.ToUInt64(SteamID), EChatEntryType.ChatMsg, Message + ": Sent from bot control panel");
        }

        /// <summary>
        /// Invite a user to a group chat, Bot must have already joined the chat.
        /// </summary>
        /// <param name="SteamID">The id of the user to invite.</param>
        /// <param name="GroupID">The id of the group.</param>
        public static void GroupChatInviter(ulong SteamID, ulong GroupID)
        {
            steamFriends.InviteUserToChat(SteamID, GroupID);
        }

    }
}