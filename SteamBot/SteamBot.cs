using System;
using SteamKit2;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using Newtonsoft.Json;

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

        //Defining dictionarys
        public static Dictionary<SteamID, EClanPermission> chatusers = new Dictionary<SteamID, EClanPermission>();

        //Defining other stuff
        static Random random = new Random();
        public static ConfigItems config;

        public static void Main()
        {
            Console.Title = "Steam Bot";
            Console.WriteLine("Press CTRL+C to quit.");

            reloadConfig();
            if (config.Password.Length == 0)
            {
                Console.Write("Please Update config.json with account info.");

                ConfigItems temp = new ConfigItems
                {
                    steamUsername = "",
                    Password = "",
                    groupID = 0
                };
                File.WriteAllText("config.json", JsonConvert.SerializeObject(temp, Formatting.Indented));

                Thread.Sleep(2000);
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

            manager.Subscribe<SteamFriends.ChatEnterCallback>(OnChatEnter);

            manager.Subscribe<SteamFriends.ChatMemberInfoCallback>(OnGroupEvent);

            Console.WriteLine("\nConnecting to Steam...\n");

            var loadServersTask = SteamDirectory.Initialize(0u);
            loadServersTask.Wait();

            steamClient.Connect();

            while (true)
            {
                manager.RunWaitCallbacks(TimeSpan.FromSeconds(1));
            }
        }

        //Callbacks
        static void OnConnected(SteamClient.ConnectedCallback callback)
        {
            if (callback.Result != EResult.OK)
            {
                Console.WriteLine("Unable to connect to Steam: {0}", callback.Result);
                return;
            }

            Console.WriteLine("Connected to Steam. \nLogging in {0}...\n", config.steamUsername);

            byte[] sentryHash = null;
            if (File.Exists("sentry.bin"))
            {
                byte[] sentryFile = File.ReadAllBytes("sentry.bin");

                sentryHash = CryptoHelper.SHAHash(sentryFile);
            }

            steamUser.LogOn(new SteamUser.LogOnDetails
            {                
                Username = config.steamUsername,
                Password = config.Password,
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
                return;
            }

            Console.WriteLine("{0} successfully Logged in!", config.steamUsername);

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

            Console.WriteLine(config.steamUsername + " disconnected from Steam, reconnecting in 5...");

            Thread.Sleep(5000);

            steamClient.Connect();
        }

        static void OnAccountInfo(SteamUser.AccountInfoCallback callback)
        {
            steamFriends.SetPersonaState(EPersonaState.Online);
            steamFriends.JoinChat(config.groupID);
        }

        static void OnChatMessage(SteamFriends.FriendMsgCallback callback)
        {
            if (callback.Message.Length < 1)
            {
                return;
            }

            Console.WriteLine("Received " + callback.Message + " From: " + steamFriends.GetFriendPersonaName(callback.Sender) + ".");
        }

        static void OnChatEnter(SteamFriends.ChatEnterCallback callback)
        {
            Console.WriteLine("Joined " + steamFriends.GetClanName(callback.ClanID) + "`s group chat.");
            Console.WriteLine("There are currently " + callback.NumChatMembers + " people in this group chat.");
            for (var i = 0; i < callback.ChatMembers.Count; i++)
            {
                chatusers.Add(callback.ChatMembers[i].SteamID, callback.ChatMembers[i].Details);
                Console.WriteLine(steamFriends.GetFriendPersonaName(callback.ChatMembers[i].SteamID) + " : " + callback.ChatMembers[i].Details);
            }
            Startup.startconsole();
        }

        static void OnGroupMessage(SteamFriends.ChatMsgCallback callback)
        {
            Console.WriteLine(callback.Message + " From: " + steamFriends.GetFriendPersonaName(callback.ChatterID));
        }

        static void OnGroupEvent(SteamFriends.ChatMemberInfoCallback callback)
        {
            if(callback.StateChangeInfo.StateChange == EChatMemberStateChange.Entered)
            {
                Console.WriteLine(steamFriends.GetFriendPersonaName(callback.StateChangeInfo.ChatterActedBy) + " has connected.");
            }
            else if (callback.StateChangeInfo.StateChange == EChatMemberStateChange.Kicked || callback.StateChangeInfo.StateChange == EChatMemberStateChange.Banned)
            {
                Console.WriteLine(steamFriends.GetFriendPersonaName(callback.StateChangeInfo.ChatterActedOn) + " was " + callback.StateChangeInfo.StateChange.ToString().ToLower() + " by " + steamFriends.GetFriendPersonaName(callback.StateChangeInfo.ChatterActedBy));
            }
            else if (callback.StateChangeInfo.StateChange == EChatMemberStateChange.Left || callback.StateChangeInfo.StateChange == EChatMemberStateChange.Disconnected)
            {
                Console.WriteLine(steamFriends.GetFriendPersonaName(callback.StateChangeInfo.ChatterActedBy) + " has disconnected.");
            }

            if (callback.StateChangeInfo.StateChange == EChatMemberStateChange.Entered)
            {
                chatusers.Add(callback.StateChangeInfo.ChatterActedBy, callback.StateChangeInfo.MemberInfo.Details);
            }
            else
            {
                chatusers.Remove(callback.StateChangeInfo.ChatterActedOn);

                if (callback.StateChangeInfo.ChatterActedOn == steamUser.SteamID)
                {
                    Environment.Exit(1);
                }
            }            
        }

        public static void reloadConfig()
        {
            config = JsonConvert.DeserializeObject<ConfigItems>(File.ReadAllText("config.json"));
        }

        public static void saveConfig()
        {
            File.WriteAllText("config.json", JsonConvert.SerializeObject(config, Formatting.Indented));
        }
    }
}