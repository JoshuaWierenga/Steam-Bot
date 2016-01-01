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
        static string quizWin;

        //Defining steam stuff
        static SteamClient steamClient;
        static CallbackManager manager;
        public static SteamUser steamUser;
        public static SteamFriends steamFriends;

        //Defining bools
        static bool isRunning = false;
        static bool isQuizRunning = false;
        static bool isQuizQuestionAnswered = false;

        //Defining ints
        static int LogTotal;
        static int votingtokick;
        static int votingtoban;
        static int kicknumbers;
        static int bannumbers;
        static int quiztotalquestions;
        static int quizcurrentnumber;

        //Defining dictionarys

        public static Dictionary<SteamID, EClanPermission> chatusers = new Dictionary<SteamID, EClanPermission>();

        public static Dictionary<SteamID, List<SteamID>> kickList = new Dictionary<SteamID, List<SteamID>>();

        public static Dictionary<SteamID, List<SteamID>> banList = new Dictionary<SteamID, List<SteamID>>();

        static Dictionary<string, string> quizquestions = new Dictionary<string, string>();

        static Dictionary<SteamID, int> quizresults = new Dictionary<SteamID, int>();

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
            Startup.startconsole();
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
            chatusers.Clear();

            Console.WriteLine("\n{0} disconnected from Steam, reconnecting in 5...\n", config.User);

            Thread.Sleep(TimeSpan.FromSeconds(5));

            steamClient.Connect();
        }

        static void OnAccountInfo(SteamUser.AccountInfoCallback callback)
        {
            steamFriends.SetPersonaState(EPersonaState.Online);
            steamFriends.JoinChat();
        }

        static void OnChatMessage(SteamFriends.FriendMsgCallback callback)
        {
            if (callback.Message.Length < 1)
            {
                return;
            }

            Console.WriteLine(callback.Message + " From: " + steamFriends.GetFriendPersonaName(callback.Sender));

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
                            #region help
                            case "!help":
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Bot help:");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "This bot was made by mrjosheyhalo");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Commands:");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "!startquiz  : Start a quiz");
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "");
                                break;
                            #endregion
                            #region quizstart
                            case "!startquiz":
                                Console.WriteLine("test");
                                args = seperate(2, ' ', callback.Message);
                                Dictionary<string, string> questions = new Dictionary<string, string>();
                                questions.Add("test", "test");
                                questions.Add("test2", "test2");
                                questions.Add("test3", "test3");
                                quizstarter(ulong.Parse(args[1]), args[2], questions);
                                break;
                            #endregion
                        }
                    }
                }
            }
        }

        static void OnChatEnter(SteamFriends.ChatEnterCallback callback)
        {
            Console.WriteLine("Bot has joined " + steamFriends.GetClanName(callback.ClanID) + "`s group chat");
            Console.WriteLine("Number of people in this chat: " + callback.NumChatMembers);
            for (var i = 0; i < callback.ChatMembers.Count; i++)
            {
                chatusers.Add(callback.ChatMembers[i].SteamID, callback.ChatMembers[i].Details);
                Console.WriteLine(steamFriends.GetFriendPersonaName(callback.ChatMembers[i].SteamID) + " : " + callback.ChatMembers[i].Details);
            }
        }

        static void OnGroupMessage(SteamFriends.ChatMsgCallback callback)
        {
            #region quizresponse
            if (isQuizRunning == true && isQuizQuestionAnswered == false && callback.Message == quizquestions.First().Value)
            {
                isQuizQuestionAnswered = true;
                GroupMessage(callback.ChatRoomID, steamFriends.GetFriendPersonaName(callback.ChatterID) + " answered the question");
                GroupMessage(callback.ChatRoomID, "The answer was " + quizquestions.First().Value);
                quizquestions.Remove(quizquestions.First().Key);

                if (quizresults.ContainsKey(callback.ChatterID))
                { quizresults[callback.ChatterID]++; }
                else
                { quizresults.Add(callback.ChatterID, 1); }

                if (quizcurrentnumber < quiztotalquestions)
                {
                    quizcurrentnumber++;
                    GroupMessage(callback.ChatRoomID, "Question " + quizcurrentnumber);
                    GroupMessage(callback.ChatRoomID, quizquestions.First().Key);
                }
                else
                {
                    GroupMessage(callback.ChatRoomID, "Quiz is over");
                    isQuizRunning = false;

                    quizresults.OrderByDescending(key => key.Value);

                    Console.WriteLine("Quiz Results");

                    foreach (KeyValuePair<SteamID, int> winner in quizresults)
                    {
                        Console.WriteLine(winner.Key + " : " + winner.Value);
                    }

                    GroupMessage(callback.ChatRoomID, steamFriends.GetFriendPersonaName(quizresults.Last().Key) + " won the quiz with " + quizresults.Last().Value + " questions answered");
                    PrivateMessage(quizresults.Last().Key, "you win " + quizWin);
                    quizresults.Clear();
                }
                isQuizQuestionAnswered = false;

            }
            #endregion

            Console.WriteLine(callback.Message + " From: " + steamFriends.GetFriendPersonaName(callback.ChatterID));

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
                    //TODO: Fix kick command
                    #region kick
                    case "kick":
                    /*args = seperate(1, ' ', callback.Message);
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

                                kickList.Add(callback.ChatRoomID, new List<SteamID> { callback.ChatterID });

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
                    break;*/
                    #endregion
                    //TODO: Fix ban command
                    #region ban
                    /*case "ban":
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

                                    banList.Add(callback.ChatRoomID, new Dictionary<SteamID, List<SteamID>> { { callback.ChatRoomID, new List<SteamID> { callback.ChatterID } } });

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
                        break;*/
                    #endregion

                    #region quizstart
                    case "startquiz":
                        int countspaces = callback.Message.Count(c => c == ' ');

                        if (countspaces > 0)
                        {
                            List<string> arguments = callback.Message.Split('|').ToList();
                            List<string> prizelist = arguments.First().Split(' ').ToList();
                            List<string> questionslist = arguments.Last().Split(' ').ToList();

                            prizelist.Remove("startquiz");
                            prizelist.Remove(prizelist.Last());
                            questionslist.Remove(questionslist.First());

                            string prize = string.Join(" ", prizelist.ToArray());

                            string[] questionsarray = questionslist.ToArray();
                            Dictionary<string, string> questions = new Dictionary<string, string>();

                            for (int i = 1; i < questionsarray.Count(); i++)
                            {
                                if (i % 2 == 1)
                                {
                                    questions.Add(questionsarray[i - 1], questionsarray[i]);
                                }
                            }

                            quizstarter(103582791437475688, prize, questions);
                        }

                        break;
                        #endregion
                }
            }
        }

        static void OnGroupEvent(SteamFriends.ChatMemberInfoCallback callback)
        {
            if (callback.StateChangeInfo.StateChange == EChatMemberStateChange.Left || callback.StateChangeInfo.StateChange == EChatMemberStateChange.Entered)
            {
                Console.WriteLine(steamFriends.GetFriendPersonaName(callback.StateChangeInfo.ChatterActedBy) + " " + callback.StateChangeInfo.StateChange.ToString().ToLower() + " the chat");
            }
            else if (callback.StateChangeInfo.StateChange == EChatMemberStateChange.Kicked || callback.StateChangeInfo.StateChange == EChatMemberStateChange.Banned)
            {
                Console.WriteLine(steamFriends.GetFriendPersonaName(callback.StateChangeInfo.ChatterActedOn) + " was " + callback.StateChangeInfo.StateChange.ToString().ToLower() + " from the chat by " + steamFriends.GetFriendPersonaName(callback.StateChangeInfo.ChatterActedBy));
            }
            else if (callback.StateChangeInfo.StateChange == EChatMemberStateChange.Disconnected)
            {
                Console.WriteLine(steamFriends.GetFriendPersonaName(callback.StateChangeInfo.ChatterActedBy) + " disconnected from the Chat");
            }

            if (callback.StateChangeInfo.StateChange == EChatMemberStateChange.Left || callback.StateChangeInfo.StateChange == EChatMemberStateChange.Disconnected || callback.StateChangeInfo.StateChange == EChatMemberStateChange.Kicked || callback.StateChangeInfo.StateChange == EChatMemberStateChange.Banned)
            {

                chatusers.Remove(callback.StateChangeInfo.ChatterActedOn);

                if (callback.StateChangeInfo.ChatterActedOn == steamUser.SteamID)
                {
                    chatusers.Clear();
                }
            }
            else if (callback.StateChangeInfo.StateChange == EChatMemberStateChange.Entered)
            {
                chatusers.Add(callback.StateChangeInfo.ChatterActedBy, callback.StateChangeInfo.MemberInfo.Details);
            }
            
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
        /// Send a message in a group chat, Must have already joined the chat.
        /// </summary>
        /// <param name="GroupID">The id of the group.</param>
        /// <param name="GroupMessage">The message.</param>
        public static void GroupMessage(ulong GroupID, string GroupMessage)
        {
            steamFriends.SendChatRoomMessage(GroupID, EChatEntryType.ChatMsg, GroupMessage);
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

                if (Function == 0) { steamFriends.KickChatMember(clanid, Userid); }
                else if (Function == 1) { steamFriends.BanChatMember(clanid, Userid); }
            }      
        }

        //TODO: Change to json.net
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

        /// <summary>
        /// Starts a quiz in a group chat, must already be in that group chat
        /// </summary>
        /// <param name="GroupID">The group id of the group to run the quiz in</param>
        /// <param name="prize">What the user gets when they win the give away, the string just gets sent to then so it could be a steam code or just text for something else</param>
        /// <param name="questions">A dictionary of questions and answers the key must be the question and the value must be the answer</param>
        public static void quizstarter(ulong GroupID, string prize, Dictionary<string, string> questions)
        {
            if (isQuizRunning == false && prize != "" && questions.Count != 0)
            {
                isQuizRunning = true;
                quizquestions = questions;
                quizWin = prize;
                quizcurrentnumber = 1;
                quiztotalquestions = quizquestions.Count();
                GroupMessage(GroupID, "Question " + quizcurrentnumber);
                GroupMessage(GroupID, quizquestions.First().Key);
            }
        }
    }
}