using System;
using System.Linq;
using SteamKit2;
using System.IO;
using System.Threading;

namespace SteamBot
{
    class SteamBot
    {

        static string user, pass;

        static SteamClient steamClient;
        static CallbackManager manager;
        static SteamUser steamUser;
        static SteamFriends steamFriends;

        static bool isRunning = false;

        //static string notGaming;

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
                user = Console.ReadLine();

                Console.Write("Password: ");
                pass = Console.ReadLine();

                Login.WriteLine(user);
                Login.WriteLine(pass);
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

            new Callback<SteamClient.ConnectedCallback>(OnConnected, manager);
            new Callback<SteamClient.DisconnectedCallback>(OnDisconnected, manager);

            new Callback<SteamUser.UpdateMachineAuthCallback>(OnMachineAuth, manager);

            new Callback<SteamUser.LoggedOnCallback>(OnLoggedOn, manager);

            new Callback<SteamUser.AccountInfoCallback>(OnAccountInfo, manager);
            new Callback<SteamFriends.FriendMsgCallback>(OnChatMessage, manager);

            new Callback<SteamFriends.FriendsListCallback>(OnFriendsList, manager);

            new Callback<SteamFriends.ChatInviteCallback>(OnChatInvite, manager);
            new Callback<SteamFriends.ChatEnterCallback>(OnChatEnter, manager);
            new Callback<SteamFriends.ChatMsgCallback>(OnGroupMessage, manager);
            new Callback<SteamFriends.ChatMemberInfoCallback>(OnGroupUserJoin, manager);

            isRunning = true;

            Console.WriteLine("\nConnecting to Steam...\n");

            steamClient.Connect();

            isRunning = true;
            while (isRunning)
            {
                manager.RunWaitCallbacks(TimeSpan.FromSeconds(1));
            }
            Console.ReadKey();
        }

        static void OnConnected(SteamClient.ConnectedCallback callback)
        {
            if (callback.Result != EResult.OK)
            {
                Console.WriteLine("Unable to connect to Steam: {0}", callback.Result);
                isRunning = false;
                return;
            }

            Console.WriteLine("Connected to Steam. \nLogging in {0}...\n", user);

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

        /*static void OnLoggedIn(SteamUser.LoginKeyCallback callback)
        {
            FriendListPanel();
            GroupListPanel();
        }*/

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
                    steamFriends.AddFriend(friend.SteamID);
                    Thread.Sleep(500);
                    Console.WriteLine("Recived Friend Request from: " + steamFriends.GetFriendPersonaName(friend.SteamID));
                    if (newfriend == "[unknown]")
                    {
                        return;
                    }
                    else
                    {
                        steamFriends.SendChatMessage(76561198068676400, EChatEntryType.ChatMsg, "User : " + newfriend + " has added the bot");
                    }
                }
            }
        }

        static void OnAccountInfo(SteamUser.AccountInfoCallback callback)
        {
            steamFriends.SetPersonaState(EPersonaState.Online);
        }

        static void OnChatMessage(SteamFriends.FriendMsgCallback callback)
        {
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
                                args = seperate(2, ' ', callback.Message);
                                Console.WriteLine("!send " + args[1] + " " + args[2] + " command recieved. User: " + steamFriends.GetFriendPersonaName(callback.Sender));
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
                                        steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Message sent to " + friend);
                                        break;
                                    }
                                    else if (i == (steamFriends.GetFriendCount() - 1))
                                    {
                                        steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Error " + args[1] + " is not part of a nickname that is a friend of this bot");
                                    }
                                }
                                break;
                            #endregion
                            #region friends
                            case "!friends":
                            case "!listfriends":
                                Console.WriteLine("!Friends command recieved. User: " + steamFriends.GetFriendPersonaName(callback.Sender));
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
                                Console.WriteLine("!state " + args[1] + " command recived. User: " + steamFriends.GetFriendPersonaName(callback.Sender));
                                if (args[1] == "away")
                                {
                                    steamFriends.SetPersonaState(EPersonaState.Away);
                                    return;
                                }
                                else if (args[1] == "busy")
                                {
                                    steamFriends.SetPersonaState(EPersonaState.Busy);
                                    return;
                                }
                                else if (args[1] == "online")
                                {
                                    steamFriends.SetPersonaState(EPersonaState.Online);
                                    return;
                                }
                                else if (args[1] == "snooze")
                                {
                                    steamFriends.SetPersonaState(EPersonaState.Snooze);
                                    return;
                                }
                                break;
                            #endregion
                            #region Name
                            case "!name":
                            case "!setname":
                                args = seperate(1, ' ', callback.Message);
                                Console.WriteLine("!name" + args[1] + " command recived. User: " + steamFriends.GetFriendPersonaName(callback.Sender));
                                steamFriends.SetPersonaName(args[1]);
                                break;
                            #endregion
                            #region Quit
                            case "!quit":
                                {
                                    Console.WriteLine("!quit commmand recived From: " + steamFriends.GetFriendPersonaName(callback.Sender));
                                    steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Bot Disconnected");
                                    System.Threading.Thread.Sleep(2000);
                                    steamUser.LogOff();
                                    System.Threading.Thread.Sleep(18000);
                                    System.Environment.Exit(1);
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
                                nameandidsaving("friendList.txt", "friend");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg,"Bot has : " + steamFriends.GetFriendCount().ToString() + " Friends");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg,"Bot`s friend list has been reloaded, but must be restarted for changes to take effect");
                                break;
                            #endregion
                            #region Group Refesh
                            case "!grouprefresh":
                                nameandidsaving("groupList.txt", "group");
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
            Console.WriteLine(user + " Has been invited to " + callback.ChatRoomName + "`s group chat. (" + callback.ChatRoomID + ") by " + steamFriends.GetFriendPersonaName(callback.FriendChatID));

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
            //steamFriends.SendChatMessage(76561198068676400, EChatEntryType.ChatMsg, Chatters.ToString());

            Console.Write(callback.ChatMembers.Count);
            for(var i = 0; i < callback.ChatMembers.Count; i++)
            {
                Console.WriteLine(callback.ChatMembers[i].SteamID);
            }
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
                        user = steamFriends.GetFriendPersonaName(callback.ChatterID);

                        if (callback.ChatRoomID == 110338190878531432)
                        {
                            //steamFriends.SendChatMessage(Convert.ToUInt64(line), EChatEntryType.ChatMsg, user + " : " + callback.Message + " from " + "Requiem>Gamers group chat");
                        }
                        else
                        {
                            steamFriends.SendChatMessage(Convert.ToUInt64(line), EChatEntryType.ChatMsg, user + " : " + callback.Message + " from " + steamFriends.GetClanName(callback.ChatRoomID) + " (" + callback.ChatRoomID + ") group chat");
                        }
                    }
                }
            }

            string[] args;
            if (callback.ChatMsgType == EChatEntryType.ChatMsg)
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
                            #region State
                            /*case "!state":
                                args = seperate(1, ' ', callback.Message);
                                Console.WriteLine("!state " + args[1] + " command recived. User: " + steamFriends.GetFriendPersonaName(callback.ChatterID));
                                if (args[1] == "away" || args[1] == "Away")
                                {
                                    steamFriends.SetPersonaState(EPersonaState.Away);
                                    return;
                                }
                                else if (args[1] == "busy" || args[1] == "Busy")
                                {
                                    steamFriends.SetPersonaState(EPersonaState.Busy);
                                    return;
                                }
                                else if (args[1] == "play" || args[1] == "Play")
                                {
                                    steamFriends.SetPersonaState(EPersonaState.LookingToPlay);
                                    return;
                                }
                                else if (args[1] == "Trade" || args[1] == "Trade")
                                {
                                    steamFriends.SetPersonaState(EPersonaState.LookingToTrade);
                                    return;
                                }
                                else if (args[1] == "offline" || args[1] == "Offline")
                                {
                                    steamFriends.SetPersonaState(EPersonaState.Offline);
                                    return;
                                }
                                else if (args[1] == "online" || args[1] == "Online")
                                {
                                    steamFriends.SetPersonaState(EPersonaState.Online);
                                    return;
                                }
                                else if (args[1] == "snooze" || args[1] == "Snooze")
                                {
                                    steamFriends.SetPersonaState(EPersonaState.Snooze);
                                    return;
                                }
                                break;*/
                            #endregion
                            #region Name
                            case "!name":
                                args = seperate(1, ' ', callback.Message);
                                Console.WriteLine("!name" + args[1] + " command recived. User: " + steamFriends.GetFriendPersonaName(callback.ChatterID));
                                steamFriends.SetPersonaName(args[1]);
                                break;
                            case "!test":

                                args = seperate(1, ' ', callback.Message);
                                if (args[0] == "-1")
                                {
                                    steamFriends.SendChatRoomMessage(callback.ChatRoomID, EChatEntryType.ChatMsg, "no args found");
                                    steamFriends.SendChatMessage(callback.ChatterID, EChatEntryType.ChatMsg, "no args found");
                                    break;
                                }
                                else
                                {
                                    steamFriends.SendChatRoomMessage(callback.ChatRoomID, EChatEntryType.ChatMsg, args[1]);
                                }
                                break;
                                #endregion
                        }
                    }
                    else
                    {

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
                                user = steamFriends.GetFriendPersonaName(callback.ChatterID);
                                Console.WriteLine(user + " : " + callback.Message);
                                break;
                        }
                    }

                }

            }
        }

        static void OnGroupUserJoin(SteamFriends.ChatMemberInfoCallback callback)
        {
            Console.WriteLine(steamFriends.GetFriendPersonaName(callback.StateChangeInfo.ChatterActedBy) + " " + callback.StateChangeInfo.StateChange + " the Chat");
        }

        public static bool isBotAdmin(SteamID sid)
        {
            try
            {
                if (sid.ConvertToUInt64() == Convert.ToUInt64(File.ReadAllText("admin.txt")))
                {
                    return true;
                }

                steamFriends.SendChatMessage(sid, EChatEntryType.ChatMsg, "You are not a bot admin");
                Console.WriteLine(steamFriends.GetFriendPersonaName(sid) + " attempted to use an administrator command while not an administrator.");
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

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

        public static void StatePanel(string state)
        {
            if (state == "Online")
            {
                steamFriends.SetPersonaState(EPersonaState.Online);
            }
            else if (state == "Offline")
            {
                steamFriends.SetPersonaState(EPersonaState.Offline);
            }
            else if (state == "Away")
            {
                steamFriends.SetPersonaState(EPersonaState.Away);
            }
            else if (state == "Busy")
            {
                steamFriends.SetPersonaState(EPersonaState.Busy);
            }
            else if (state == "Snooze")
            {
                steamFriends.SetPersonaState(EPersonaState.Snooze);
            }

        }

        public static void NamePanel(string name)
        {
            steamFriends.SetPersonaName(name);
        }

        public static void GroupChatPanel(ulong GroupID)
        {
            steamFriends.JoinChat(GroupID); 
        }

        public static void GroupMessagePanel(ulong GroupID, string GroupMessage)
        {
            steamFriends.SendChatRoomMessage(GroupID, EChatEntryType.ChatMsg, GroupMessage);
        }

        public static void nameandidsaving(string filename, string list)
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

        public static string namestosteamId(string name, string file, int kindofid)
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

        public static void MessageFriendPanel(ulong SteamID, string Message)
        {
            steamFriends.SendChatMessage(Convert.ToUInt64(SteamID), EChatEntryType.ChatMsg, Message + ": Sent from bot control panel");
        }

        public static void InvitetoGroupChat(ulong SteamID, ulong GroupID)
        {
            steamFriends.InviteUserToChat(SteamID, GroupID);
        }
    }
}