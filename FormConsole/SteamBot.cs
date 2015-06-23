using System;
using System.Linq;
using SteamKit2;
using System.IO;
using System.Threading;

namespace FormConsole
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

        public static void MainBot()
        { 
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
            Console.Title = "BBBBBBOOOOOTTTTTT";
            Console.WriteLine("CTRL+C quits the program");

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

        static void OnLoggedIn(SteamUser.LoginKeyCallback callback)
        {
            FriendListPanel();
            GroupListPanel();
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
                    Console.WriteLine("hi" + " recieved. User: " + steamFriends.GetFriendPersonaName(callback.Sender));
                    steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Hi " + steamFriends.GetFriendPersonaName(callback.Sender));
                    break;
                case "hello":
                    Console.WriteLine("hello" + " recieved. User: " + steamFriends.GetFriendPersonaName(callback.Sender));
                    steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Hello " + steamFriends.GetFriendPersonaName(callback.Sender));
                    break;
                #endregion
                #region MeLined
                /*case "MeLined":
                    Console.WriteLine("me" + " command recieved. User: " + steamFriends.GetFriendPersonaName(callback.Sender));
                    if (steamFriends.GetFriendGamePlayedName(callback.Sender) == "")
                    {
                        notGaming = "Not Currently Playing Anything, What a shame";
                    }
                    else
                    {
                        notGaming = steamFriends.GetFriendGamePlayedName(callback.Sender);
                    }
                    steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Your name is:  " + steamFriends.GetFriendPersonaName(callback.Sender) + ", Your Status is:  " + steamFriends.GetFriendPersonaState(callback.Sender) + ", You are playing:  " + notGaming);
                    break;*/
                #endregion
                #region StatusLined
                /*case "StatusLined":
                    Console.WriteLine("States" + " command recieved. User: " + steamFriends.GetFriendPersonaName(callback.Sender));
                    steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "My name is:  " + steamFriends.GetPersonaName() + ", My Status is: " + steamFriends.GetPersonaState() + ", I am playing:  " + " I can`t play games, i`m a Bot :-)");
                    break;*/
                #endregion
                #region Me
                /*case "Me":
                    Console.WriteLine("me" + " command recieved. User: " + steamFriends.GetFriendPersonaName(callback.Sender));
                    steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Your name is:  " + steamFriends.GetFriendPersonaName(callback.Sender));
                    steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Your Status is:  " + steamFriends.GetFriendPersonaState(callback.Sender));
                    if (steamFriends.GetFriendGamePlayedName(callback.Sender) == "")
                    {
                        notGaming = "Not Currently Playing Anything, What a shame";
                    }
                    else
                    {
                        notGaming = steamFriends.GetFriendGamePlayedName(callback.Sender);
                    }
                    steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "You are playing:  " + notGaming);
                    break;*/
                #endregion
                #region Status
                /*case "Status":
                    Console.WriteLine("States" + " command recieved. User: " + steamFriends.GetFriendPersonaName(callback.Sender));
                    steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "My name is:  " + steamFriends.GetPersonaName());
                    steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "My Status is: " + steamFriends.GetPersonaState());
                    steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "I am playing:  " + " I can`t play games, i`m a Bot :-)");
                    break;*/
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
                                args = seperate(1, ' ', callback.Message);
                                Console.WriteLine("!name" + args[1] + " command recived. User: " + steamFriends.GetFriendPersonaName(callback.Sender));
                                steamFriends.SetPersonaName(args[1]);
                                break;
                            #endregion
                            #region Group
                            case "!group":
                                args = seperate(1, ' ', callback.Message);
                                Console.WriteLine("!group " + args[1] + " command recived. User: " + steamFriends.GetFriendPersonaName(callback.Sender));
                                if (args[1] == "requiem")
                                {
                                    steamFriends.JoinChat(103582791437475688);
                                }
                                break;
                            #endregion
                            #region GroupMessage
                            case "!gc":
                                args = seperate(2, ' ', callback.Message);
                                Console.WriteLine("!gc " + args[1] + " " + args[2] + " command recived. User: " + steamFriends.GetFriendPersonaName(callback.Sender));

                                if (args[1] == "requiem" || args[1] == "r" || args[1] == "" || args[1] == "")
                                {
                                    steamFriends.SendChatRoomMessage(103582791437475688, EChatEntryType.ChatMsg, args[2]);
                                }

                                break;
                            #endregion
                            #region Quit
                            case "!quit":
                                {
                                    Console.WriteLine("!quit commmand recived From: " + steamFriends.GetFriendPersonaName(callback.Sender));
                                    steamFriends.SendChatMessage(76561198068676400, EChatEntryType.ChatMsg, "Bot Disconnected at " + DateTime.Now + " UTC +10");
                                    steamFriends.SendChatMessage(76561198068676400, EChatEntryType.ChatMsg, "!quit command recived, sent by " + steamFriends.GetFriendPersonaName(callback.Sender) + " (" + callback.Sender + ") ");
                                    steamFriends.SendChatMessage(76561198031687808, EChatEntryType.ChatMsg, "Bot Disconnected at " + DateTime.Now + " UTC +10");
                                    steamFriends.SendChatMessage(76561198031687808, EChatEntryType.ChatMsg, "!quit command recived, sent by " + steamFriends.GetFriendPersonaName(callback.Sender) + " (" + callback.Sender + ") ");
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
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "!friends :  Displays list of the bots friends");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "!state   :  Sets state of bot, can be set to: online, away, busy or snooze");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "!name  :  Names the bot");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "!send   :  Sends a message to someone, must be friend of bot");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "!group  :  Joins a group chat, the only option currently is requiem");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "!gc       :  Sends a message to a group chat, run !group first to join a chat");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "!chatlog :  Sends the chat to you though private message, run !group first to join a chat");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "!friendrefresh :  Reloads bots internal friend list");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "!chatrefresh :  Reloads bots internal group list");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "!version : Displays version log");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "!quit     :  Turn off the bot");
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
                            #region Version
                            case "!version":
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Showing 5 most recent updates:");
                                Thread.Sleep(2000);
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Version 1.1.1");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Bot connected and disconnected messages now show name in console");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "-----------------------------------------------------------------");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Version 1.1.0");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, " Bot now auto downloads new friend and group info.");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "-----------------------------------------------------------------");                              
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Version 1.0.9");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Bot can now send group chat messages to the group it has joined though the gui");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "-----------------------------------------------------------------");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Version 1.0.8");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Bot can now join group chats again");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "-----------------------------------------------------------------");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Version 1.0.7");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Bot now logs list of groups it is part of for the group joining and group messaging features");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Bot now uses friend info to send messages to users from the gui.");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Bot now saves login info to file.");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "-----------------------------------------------------------------");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Version 1.0.6");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Bot now logs list of friends so future features can be added to send messages to more then one user.");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Added !friendrefresh command to update bots friend list, bot must be restarted currently to use new list.");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Bot no longer spams bot admins about unknown friend requests on startup.");
                                Thread.Sleep(2000);
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "-----------------------------------------------------------------");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Send !versionall to recive full log");
                                break;
                            #endregion
                            #region Versionall
                            case "!versionall":
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Showing all updates:");
                                Thread.Sleep(2000);
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Version 1.1.0");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, " Bot now auto downloads new friend and group info.");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "-----------------------------------------------------------------");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Version 1.0.9");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Bot can now send group chat messages to the group it has joined though the gui");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "-----------------------------------------------------------------");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Version 1.0.8");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Bot can now join group chats again");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "-----------------------------------------------------------------");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Version 1.0.7");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Bot now logs list of groups it is part of for the group joining and group messaging features");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Bot now uses friend info to send messages to users from the gui.");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Bot now saves login info to file.");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "-----------------------------------------------------------------");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Version 1.0.6");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Bot now logs list of friends so future features can be added to send messages to more then one user.");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Added !friendrefresh command to update bots friend list, bot must be restarted currently to use new list");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Bot no longer spams bot admins about unknown friend requests on startup");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "-----------------------------------------------------------------");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Version 1.0.5");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "hi response in groups can be any case and bot will still find it");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Removed status commands in group chat");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "-----------------------------------------------------------------");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Version 1.0.4");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Send command now responds with who the message was sent to");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "hi response now can be any case and bot will still find it");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Fixed send command again");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Added !help command");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "!version now only shows 5 most recent updates now");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "-----------------------------------------------------------------");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Version 1.0.3");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Fixed send command again");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "-----------------------------------------------------------------");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Version 1.0.2");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Fixed send command");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "-----------------------------------------------------------------");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Version 1.0.1");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Removed status commands");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Added more hi statments");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Added response if send command can't find user");
                                break;
                            #endregion
                            #region Friend Refesh
                            case "!friendrefresh":
                                FriendListPanel();
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg,"Bot has : " + steamFriends.GetFriendCount().ToString() + " Friends");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg,"Bot`s friend list has been reloaded, but must be restarted for changes to take effect");
                                break;
                            #endregion
                            #region Chat Refesh
                            case "!chatrefresh":
                                GroupListPanel();
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
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Version 1.1.0");
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
                case "103582791437475688":
                    steamFriends.JoinChat(103582791437475688);
                    break;
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

        public static void FriendListPanel()
        {
            for (int i = 0; i < steamFriends.GetFriendCount(); i++)
            {

                string friends = steamFriends.GetFriendByIndex(i).ConvertToUInt64().ToString();

                StreamWriter friendList;
                StreamWriter friendListid;

                if (!File.Exists("friendList.txt"))
                {
                    friendList = new StreamWriter("friendList.txt");
                }
                else if (File.Exists("friendList.txt") && File.ReadAllLines("friendList.txt").Count() >= steamFriends.GetFriendCount())
                {
                    friendList = new StreamWriter("friendList.txt");
                }
                else if (File.Exists("friendList.txt") && File.ReadAllLines("friendList.txt").Count() == 0)
                {
                    friendList = File.AppendText("friendList.txt");
                }
                else
                {
                    friendList = File.AppendText("friendList.txt");
                }

                if (!File.Exists("friendListid.txt"))
                {
                    friendListid = new StreamWriter("friendListid.txt");
                }
                else if (File.Exists("friendListid.txt") && File.ReadAllLines("friendListid.txt").Count() >= steamFriends.GetFriendCount())
                {
                    friendListid = new StreamWriter("friendListid.txt");
                }
                else if (File.Exists("friendListid.txt") && File.ReadAllLines("friendListid.txt").Count() == 0)
                {
                    friendListid = File.AppendText("friendListid.txt");
                }
                else
                {
                    friendListid = File.AppendText("friendListid.txt");
                }

                friendList.WriteLine(steamFriends.GetFriendPersonaName(steamFriends.GetFriendByIndex(i)));
                friendList.Close();

                friendListid.WriteLine(friends);
                friendListid.Close();
            }
        }

        public static void GroupListPanel()
        {
            for (int i = 0; i < steamFriends.GetClanCount(); i++)
            {
                string clans = steamFriends.GetClanByIndex(i).ConvertToUInt64().ToString();

                StreamWriter clanList;
                StreamWriter clanListId;

                if (!File.Exists("clanList.txt"))
                {
                    clanList = new StreamWriter("clanList.txt");
                }
                else if (File.Exists("clanList.txt") && File.ReadAllLines("clanList.txt").Count() >= steamFriends.GetClanCount())
                {
                    clanList = new StreamWriter("clanList.txt");
                }
                else if (File.Exists("clanList.txt") && File.ReadAllLines("clanList.txt").Count() == 0)
                {
                    clanList = File.AppendText("clanList.txt");
                }
                else
                {
                    clanList = File.AppendText("clanList.txt");
                }

                if (!File.Exists("clanListId.txt"))
                {
                    clanListId = new StreamWriter("clanListId.txt");
                }
                else if (File.Exists("clanListId.txt") && File.ReadAllLines("clanListId.txt").Count() >= steamFriends.GetClanCount())
                {
                    clanListId = new StreamWriter("clanListId.txt");
                }
                else if (File.Exists("clanListId.txt") && File.ReadAllLines("clanListId.txt").Count() == 0)
                {
                    clanListId = File.AppendText("clanListId.txt");
                }
                else
                {
                    clanListId = File.AppendText("clanListId.txt");
                }

                clanList.WriteLine(steamFriends.GetClanName(steamFriends.GetClanByIndex(i)));
                clanList.Close();

                clanListId.WriteLine(clans);
                clanListId.Close();

            }
        }

        public static string nicknametoSteamIdPanel(string nickname)
        {
            string[] namesLine = File.ReadAllLines("friendList.txt");
            string[] namesidline = File.ReadAllLines("friendListid.txt");
            
            foreach (var name in namesLine)
            {
                if (name == nickname)
                {
                    foreach (var nameid in namesidline)
                    {

                        SteamID steamnameid = Convert.ToUInt64(nameid);
                        var nicknamefromid = steamFriends.GetFriendPersonaName(steamnameid);

                        

                        if (nicknamefromid == nickname)
                        {
                            return nameid;
                        }
                    }
                }
            }
            return string.Empty;
        }

        public static string groupnametogroupIdPanel(string groupname)
        {
            string[] groupLine = File.ReadAllLines("clanList.txt");
            string[] groupidline = File.ReadAllLines("clanListid.txt");

            foreach (var group in groupLine)
            {
                if (group == groupname)
                {
                    foreach (var groupid in groupidline)
                    {

                        SteamID groupnameid = Convert.ToUInt64(groupid);
                        var groupnamefromid = steamFriends.GetClanName(groupnameid);



                        if (groupnamefromid == groupname)
                        {
                            return groupid;
                        }
                    }
                }
            }
            return string.Empty;
        }

        public static void MessageFriendPanel(ulong SteamID, string Message)
        {
            steamFriends.SendChatMessage(Convert.ToUInt64(SteamID), EChatEntryType.ChatMsg, Message + ": Sent from bot control panel");
        }
    }
}