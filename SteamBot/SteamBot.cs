using System;
using System.Linq;
using SteamKit2;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.Text;

namespace SteamBot
{
    class SteamBot
    {
        //Defining strings
        static string authCode;

        //Defining steam stuff
        static SteamClient steamClient;
        static CallbackManager manager;
        public static SteamUser steamUser;
        public static SteamFriends steamFriends;

        //Defining bools
        static bool isRunning = false;

        //Defining ints
        static int LogTotal;
        static int votingtokick;
        static int votingtoban;
        static int kicknumbers;
        static int bannumbers;

        //Defining dictionarys
        static Dictionary<SteamID, EClanPermission> botranks = new Dictionary<SteamID, EClanPermission>();

        public static Dictionary<SteamID,Dictionary<SteamID, EClanPermission>> chatusers = new Dictionary<SteamID, Dictionary<SteamID, EClanPermission>>();

        public static Dictionary<SteamID, SteamID> chatclanid = new Dictionary<SteamID, SteamID>();

        public static Dictionary<SteamID, Dictionary<SteamID, List<SteamID>>> kickList = new Dictionary<SteamID, Dictionary<SteamID, List<SteamID>>>();

        public static Dictionary<SteamID, Dictionary<SteamID, List<SteamID>>> banList = new Dictionary<SteamID, Dictionary<SteamID, List<SteamID>>>();

        //Defining other stuff
        static Random random = new Random();
        static ConfigItems config;

        public static void Main()
        {
            Console.Title = "BBBBBBOOOOOTTTTTT";
            Console.WriteLine("CTRL+C quits the program");

            reloadConfig();
            if (config.User.Length == 0)
            {
                Console.Write($"Username: ");
                config.User = Console.ReadLine();
                Console.Write($"Password: ");
                config.Pass = Console.ReadLine();
                saveNewConfig(config);
                Main();
            }
            else { SteamLogIn(); }
        }

        static void SteamLogIn()
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

            Console.WriteLine("\nConnecting to Steam...\n");

            steamClient.Connect();

            isRunning = true;
            while (isRunning)
            {
                manager.RunWaitCallbacks(TimeSpan.FromSeconds(1));
            }
            Console.ReadLine();
        }

        //Callbacks
        static void OnConnected(SteamClient.ConnectedCallback callback)
        {
            if (callback.Result != EResult.OK)
            {
                Console.WriteLine("Unable to connect to Steam: {0}", callback.Result);
                isRunning = false;
                return;
            }

            Console.WriteLine("Connected to Steam. \nLogging in {0}...\n", config.User);

            byte[] sentryHash = null;
            if (File.Exists("sentry.bin"))
            {
                byte[] sentryFile = File.ReadAllBytes("sentry.bin");

                sentryHash = CryptoHelper.SHAHash(sentryFile);
            }

            steamUser.LogOn(new SteamUser.LogOnDetails
            {                
                Username = config.User,
                Password = config.Pass,

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

            Console.WriteLine("{0} successfully Logged in!", config.User);
            
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
            botranks.Clear();
            chatusers.Clear();
            chatclanid.Clear();

            Console.WriteLine("\n{0} disconnected from Steam, reconnecting in 5...\n", config.User);

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
                                        foreach (ulong user in config.Admins)
                                        {
                                            steamFriends.SendChatMessage(Convert.ToUInt64(user), EChatEntryType.ChatMsg, "Bot Quiting");
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
                                        var oldLines = File.ReadAllLines("ChatRequester.txt");
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
                    Console.WriteLine("Bot is " + callback.ChatMembers[i].Details.ToString().ToLower() + " of " + callback.ChatRoomName);
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

                        steamFriends.SendChatMessage(Convert.ToUInt64(line), EChatEntryType.ChatMsg, currentchatuser + " : " + callback.Message + " from " + steamFriends.GetClanName(callback.ChatRoomID) + " (" + callback.ChatRoomID + ") group chat");
                    }
                }
            }

            switch (callback.Message.ToLower())
            {
                #region Greatings
                case "hi":
                    Console.WriteLine(steamFriends.GetFriendPersonaName(callback.ChatterID) + " : " + callback.Message);
                    steamFriends.SendChatRoomMessage(callback.ChatRoomID, EChatEntryType.ChatMsg, "Hi " + steamFriends.GetFriendPersonaName(callback.ChatterID));
                    break;
                case "hello":
                    Console.WriteLine(steamFriends.GetFriendPersonaName(callback.ChatterID) + " : " + callback.Message);
                    steamFriends.SendChatRoomMessage(callback.ChatRoomID, EChatEntryType.ChatMsg, "Hello " + steamFriends.GetFriendPersonaName(callback.ChatterID));
                    break;
                #endregion

                #region some random responses
                case "lol":
                    steamFriends.SendChatRoomMessage(callback.ChatRoomID, EChatEntryType.ChatMsg, "lol");
                    break;

                case "420":
                    steamFriends.SendChatRoomMessage(callback.ChatRoomID, EChatEntryType.ChatMsg, "blaze it");
                    break;

                case "konami":
                case "konami code":
                    steamFriends.SendChatRoomMessage(callback.ChatRoomID, EChatEntryType.ChatMsg, "up up down down left right left right b a");
                    break;

                case "up up down down left right left right b a":
                    steamFriends.SendChatRoomMessage(callback.ChatRoomID, EChatEntryType.ChatMsg, $"{steamFriends.GetFriendPersonaName(callback.ChatterID)} now has 30 extra lives");
                    break;
                #endregion
                default:
                     string currentchatuser = steamFriends.GetFriendPersonaName(callback.ChatterID);
                     Console.WriteLine(currentchatuser + " : " + callback.Message);
                     break;
             }

            string[] args;
            if (callback.ChatMsgType == EChatEntryType.ChatMsg)
            {
                string command = callback.Message;
                if (callback.Message.Contains(" "))
                {
                    command = callback.Message.Remove(callback.Message.IndexOf(' ')).ToLower();
                }
                switch (command)
                {

                    #region kick
                    case "kick":
                        args = seperate(1, ' ', callback.Message);
                        EClanPermission rank = groupUserRank(callback.ChatterID, callback.ChatRoomID);

                        #region if owner or officer
                        if (rank == EClanPermission.Owner || rank == EClanPermission.Officer)
                        {
                            ulong userid = 0;

                            foreach (KeyValuePair<SteamID, EClanPermission> userids in chatusers[callback.ChatRoomID])
                            {
                                if (steamFriends.GetFriendPersonaName(userids.Key) == args[1])
                                {
                                    userid = userids.Key;
                                }
                            }

                            kickban(0, userid, callback.ChatRoomID);

                        }
                        #endregion

                        #region votingtokick == 0
                        else if (votingtokick == 0)
                        {
                            if (rank == EClanPermission.Moderator)
                            {
                                ulong userid = 0;

                                foreach (KeyValuePair<SteamID, EClanPermission> userids in chatusers[callback.ChatRoomID])
                                {
                                    if (steamFriends.GetFriendPersonaName(userids.Key) == args[1])
                                    {
                                        userid = userids.Key;
                                    }
                                }

                                if (userid != 0)
                                {
                                    steamFriends.SendChatRoomMessage(callback.ChatRoomID, EChatEntryType.ChatMsg, "Voting to kick " + steamFriends.GetFriendPersonaName(userid));

                                    kickList.Add(callback.ChatRoomID, new Dictionary<SteamID, List<SteamID>> { { callback.ChatRoomID, new List<SteamID> { callback.ChatterID } } });

                                    steamFriends.SendChatRoomMessage(callback.ChatRoomID, EChatEntryType.ChatMsg, "group: " + steamFriends.GetClanName(chatclanid[callback.ChatRoomID]));

                                    foreach (var userids in banList[callback.ChatRoomID])
                                    {
                                        steamFriends.SendChatRoomMessage(callback.ChatRoomID, EChatEntryType.ChatMsg, "person being kicked :  " + steamFriends.GetFriendPersonaName(userids.Key));

                                        foreach (var userid2 in userids.Value)
                                        {
                                            steamFriends.SendChatRoomMessage(callback.ChatRoomID, EChatEntryType.ChatMsg, "person voting for them to be kicked :  " + steamFriends.GetFriendPersonaName(userid));
                                        }
                                    }

                                    votingtokick = 1;
                                    kicknumbers++;
                                }
                            }

                            else
                            {
                                int number = random.Next(0, 3);
                                if (number == 0) { steamFriends.SendChatRoomMessage(callback.ChatRoomID, EChatEntryType.ChatMsg, "What you think you can ban them when your not even admin?"); }
                                else if (number == 1) { steamFriends.SendChatRoomMessage(callback.ChatRoomID, EChatEntryType.ChatMsg, "Why would i ever want to ban any one?"); }
                                else if (number == 2) { steamFriends.SendChatRoomMessage(callback.ChatRoomID, EChatEntryType.ChatMsg, "But they`re my friend."); }
                            }
                        }

                        #endregion

                        #region votingtokick == 1
                        else if (votingtokick == 1)
                        {
                            if (chatusers.ContainsKey(callback.ChatRoomID) && kickList.ContainsKey(callback.ChatRoomID))
                            {
                                ulong kickuserid = 0;

                                foreach (KeyValuePair<SteamID, EClanPermission> userids in chatusers[callback.ChatRoomID])
                                {
                                    if (steamFriends.GetFriendPersonaName(userids.Key) == args[1])
                                    {
                                        kickuserid = userids.Key;
                                    }
                                }

                                if (kickuserid != 0)
                                {
                                    bool voted = false;
                                    foreach (var userid in kickList[callback.ChatRoomID][kickuserid])
                                    {
                                        steamFriends.SendChatRoomMessage(callback.ChatRoomID, EChatEntryType.ChatMsg, $"{userid} : {callback.ChatterID}");
                                        if (userid == callback.ChatterID)
                                        {
                                            steamFriends.SendChatRoomMessage(callback.ChatRoomID, EChatEntryType.ChatMsg, "you have already voted");
                                            voted = true;
                                        }
                                    }

                                    if (voted == true) { break; }

                                    kicknumbers++;
                                    if (kicknumbers == 5)
                                    {
                                        ulong userid2 = 0;

                                        foreach (KeyValuePair<SteamID, EClanPermission> userids2 in chatusers[callback.ChatRoomID])
                                        {
                                            if (steamFriends.GetFriendPersonaName(userids2.Key) == args[1])
                                            {
                                                userid2 = userids2.Key;
                                            }
                                        }

                                        kickban(0, userid2, callback.ChatRoomID);
                                        votingtokick = 0;
                                        kicknumbers = 0;
                                        kickList.Clear();
                                    }
                                    else
                                    {
                                        steamFriends.SendChatRoomMessage(callback.ChatRoomID, EChatEntryType.ChatMsg, bannumbers + "/5 required for kicking");
                                        banList[callback.ChatRoomID][kickuserid].Add(callback.ChatterID);
                                    }
                                }
                                else { steamFriends.SendChatMessage(callback.ChatterID, EChatEntryType.ChatMsg, "Members can`t start kick requests, ask a mod or admin to kick them"); }
                            }
                        }
                        #endregion
                        break;
                    #endregion

                    #region ban
                    case "ban":
                        args = seperate(1, ' ', callback.Message);
                        EClanPermission rank2 = groupUserRank(callback.ChatterID, callback.ChatRoomID);

                        #region if owner
                        if (rank2 == EClanPermission.Owner)
                        {
                            ulong userid = 0;

                            foreach (KeyValuePair<SteamID, EClanPermission> userids in chatusers[callback.ChatRoomID])
                            {
                                if (steamFriends.GetFriendPersonaName(userids.Key) == args[1])
                                {
                                    userid = userids.Key;
                                }
                            }

                            kickban(1, userid, callback.ChatRoomID);

                        }
                        #endregion

                        #region votingtoban == 0
                        else if (votingtoban == 0)
                        {
                            if (rank2 == EClanPermission.Officer || rank2 == EClanPermission.Moderator)
                            {
                                ulong userid = 0;

                                foreach (KeyValuePair<SteamID, EClanPermission> userids in chatusers[callback.ChatRoomID])
                                {
                                    if (steamFriends.GetFriendPersonaName(userids.Key) == args[1])
                                    {
                                        userid = userids.Key;
                                    }
                                }

                                if (userid != 0)
                                {
                                    steamFriends.SendChatRoomMessage(callback.ChatRoomID, EChatEntryType.ChatMsg, "Voting to ban " + steamFriends.GetFriendPersonaName(userid));

                                    banList.Add(callback.ChatRoomID, new Dictionary<SteamID, List<SteamID>>{{callback.ChatRoomID, new List<SteamID>{callback.ChatterID}}});              

                                    steamFriends.SendChatRoomMessage(callback.ChatRoomID, EChatEntryType.ChatMsg, "group: " + steamFriends.GetClanName(chatclanid[callback.ChatRoomID]));

                                    foreach (var userids in banList[callback.ChatRoomID])
                                    {
                                        steamFriends.SendChatRoomMessage(callback.ChatRoomID, EChatEntryType.ChatMsg, "person being kicked :  " + steamFriends.GetFriendPersonaName(userids.Key));

                                        foreach (var userid2 in userids.Value)
                                        {
                                            steamFriends.SendChatRoomMessage(callback.ChatRoomID, EChatEntryType.ChatMsg, "person voting for them to be kicked :  " + steamFriends.GetFriendPersonaName(userid));
                                        }
                                    }
                                
                                    votingtoban = 1;
                                    bannumbers++;
                                }
                            }

                            else
                            {
                                int number = random.Next(0, 3);
                                if (number == 0) { steamFriends.SendChatRoomMessage(callback.ChatRoomID, EChatEntryType.ChatMsg, "What you think you can ban them when your not even admin?"); }
                                else if (number == 1) { steamFriends.SendChatRoomMessage(callback.ChatRoomID, EChatEntryType.ChatMsg, "Why would i ever want to ban any one?"); }
                                else if (number == 2) { steamFriends.SendChatRoomMessage(callback.ChatRoomID, EChatEntryType.ChatMsg, "But they`re my friend."); }
                            }
                        }

                        #endregion

                        #region votingtoban == 1
                        else if (votingtoban == 1)
                        {
                            if (chatusers.ContainsKey(callback.ChatRoomID) && banList.ContainsKey(callback.ChatRoomID))
                            {
                                ulong banuserid = 0;

                                foreach (KeyValuePair<SteamID, EClanPermission> userids in chatusers[callback.ChatRoomID])
                                {
                                    if (steamFriends.GetFriendPersonaName(userids.Key) == args[1])
                                    {
                                        banuserid = userids.Key;
                                    }
                                }

                                if (banuserid != 0)
                                {
                                    bool voted = false;
                                    foreach (var userid in banList[callback.ChatRoomID][banuserid])
                                    {
                                        steamFriends.SendChatRoomMessage(callback.ChatRoomID, EChatEntryType.ChatMsg, $"{userid} : {callback.ChatterID}");
                                        if (userid == callback.ChatterID)
                                        {
                                            steamFriends.SendChatRoomMessage(callback.ChatRoomID, EChatEntryType.ChatMsg, "you have already voted");
                                            voted = true;
                                        }
                                    }

                                    if (voted == true) { break; }

                                    bannumbers++;
                                    if (bannumbers == 5)
                                    {
                                        ulong userid2 = 0;

                                        foreach (KeyValuePair<SteamID, EClanPermission> userids2 in chatusers[callback.ChatRoomID])
                                        {
                                            if (steamFriends.GetFriendPersonaName(userids2.Key) == args[1])
                                            {
                                                userid2 = userids2.Key;
                                            }
                                        }

                                        kickban(1, userid2, callback.ChatRoomID);
                                        votingtoban = 0;
                                        bannumbers = 0;
                                        banList.Clear();
                                    }
                                    else
                                    {
                                        steamFriends.SendChatRoomMessage(callback.ChatRoomID, EChatEntryType.ChatMsg, bannumbers + "/5 required for banning");
                                        banList[callback.ChatRoomID][banuserid].Add(callback.ChatterID);
                                    }                                          
                                }
                                else { steamFriends.SendChatMessage(callback.ChatterID, EChatEntryType.ChatMsg, "Members can`t start ban requests, ask a mod or admin to ban them"); }
                            }
                        }
                        #endregion
                        break;
                    #endregion
                }
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
        /// <param name="userid">The steam64 id to check</param>
        /// <returns>true or false depending on if user is an admin of the bot</returns>
        public static bool isUserAdmin(SteamID userid)
        {
            foreach (ulong user in config.Admins)
            {
                if (userid == user)
                {
                    return true;
                }
            }
            steamFriends.SendChatMessage(userid, EChatEntryType.ChatMsg, "You are not a bot admin");
            Console.WriteLine(steamFriends.GetFriendPersonaName(userid) + " attempted to use an administrator command while not an administrator.");
            return false;
        }

        /// <summary>
        /// Checks what rank the user is in the group chat
        /// </summary>
        /// <param name="sid">The steam64 id to check</param>
        /// <param name="rank">What rank to check for</param>
        /// <returns></returns>
        public static EClanPermission groupUserRank(SteamID userid, SteamID groupid)
        {
            foreach (KeyValuePair<SteamID, EClanPermission> userrank in chatusers[groupid])
            {
                {
                    if (userid == userrank.Key)
                    {
                        return userrank.Value;
                    }
                }
            }
            return EClanPermission.NonMember;
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
            File.Delete(filename);
            StreamWriter file = File.AppendText(filename);
         
            if (list == "group")
            {
                for (int i = 0; i < steamFriends.GetClanCount(); i++)
                {
                    ulong id = steamFriends.GetClanByIndex(i).ConvertToUInt64();
                    string name = steamFriends.GetClanName(steamFriends.GetClanByIndex(i));

                    file.WriteLine(name + "✏" + id);
                }
            }
            else if (list == "friend")
            {
                for (int i = 0; i < steamFriends.GetFriendCount(); i++)
                {
                    ulong id = steamFriends.GetFriendByIndex(i).ConvertToUInt64();
                    string name = steamFriends.GetFriendPersonaName(id);

                    file.WriteLine(name + "✏" + id);
                }
            }
            file.Close();
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
            //steamFriends.SendChatMessage(Convert.ToUInt64(SteamID), EChatEntryType.ChatMsg, Message + ": Sent from bot control panel");
            steamFriends.SendChatMessage(Convert.ToUInt64(SteamID), EChatEntryType.ChatMsg, Message);
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

        /// <summary>
        /// Kicks or Bans a user from a group chat
        /// </summary>
        /// <param name="function">0 for kick, 1 for ban</param>
        /// <param name="User">User to kick or ban</param>
        /// <param name="ChatRoomID">The id of the chat room</param>
        public static void kickban(int Function, ulong Userid, ulong ChatRoomID)
        {
            if (chatusers.ContainsKey(ChatRoomID))
            {
                if (Function == 0) { steamFriends.SendChatRoomMessage(ChatRoomID, EChatEntryType.ChatMsg, "kicking " + steamFriends.GetFriendPersonaName(Userid)); }
                else if (Function == 1) { steamFriends.SendChatRoomMessage(ChatRoomID, EChatEntryType.ChatMsg, "banning " + steamFriends.GetFriendPersonaName(Userid)); }

                ulong clanid = chatclanid[ChatRoomID];

                if (Function == 0) { steamFriends.KickChatMember(clanid, Userid); }
                else if (Function == 1) { steamFriends.BanChatMember(clanid, Userid); }
            }      
        }

        public static void reloadConfig()
        {
            if(!File.Exists("config.cfg"))
            {
                StringBuilder sb = new StringBuilder();

                sb.Append("{\r\n");
                sb.Append("\"User\":\"\",\r\n");
                sb.Append("\"Pass\":\"\",\r\n");
                sb.Append("\"Admins\":[]\r\n");
                sb.Append("}\r\n");

                File.WriteAllText("config.cfg", sb.ToString());
            }
            try
            {
                config = new JavaScriptSerializer().Deserialize<ConfigItems>(File.ReadAllText("config.cfg"));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
                

        }

        public static void saveNewConfig(ConfigItems config)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("{\r\n");
            sb.Append("\"User\":\"" + config.User + "\",\r\n");
            sb.Append("\"Pass\":\"" + config.Pass + "\",\r\n");
            sb.Append("\"Admins\":[]\r\n");
            sb.Append("}\r\n");

            File.WriteAllText("config.cfg", sb.ToString());
        }
    }
}