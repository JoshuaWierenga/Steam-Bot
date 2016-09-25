using SteamKit2;
using System;
using System.Threading;
using System.Windows.Forms;

namespace SteamBot
{
    class SteamBot
    {
        internal static string userName, password;
        internal static bool enteredDetails = false;
        static bool runBot = true;

        static SteamClient steamClient = new SteamClient();
        static CallbackManager steamCallbackManager = new CallbackManager(steamClient);
        static SteamUser steamUser = steamClient.GetHandler<SteamUser>();

        internal static void SteamConnect()
        {
            steamCallbackManager.Subscribe<SteamClient.ConnectedCallback>(OnConnected);
            steamCallbackManager.Subscribe<SteamClient.DisconnectedCallback>(OnDisconnected);
            steamCallbackManager.Subscribe<SteamUser.LoggedOnCallback>(OnLoggedOn);

            steamClient.Connect();

            while (runBot)
            {
                steamCallbackManager.RunWaitCallbacks(TimeSpan.FromSeconds(1));
            }
        }

        private static void OnConnected(SteamClient.ConnectedCallback callback)
        {
            if (callback.Result != EResult.OK)
            {
                DialogResult retry = MessageBox.Show("Unable to connect to Steam: " + callback.Result, "Steam Connection Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Warning);

                if (retry == DialogResult.Cancel)
                {
                    MessageBox.Show("Cancelling");
                    runBot = false;
                    
                    return;
                }
                else if (retry == DialogResult.Retry)
                {
                    MessageBox.Show("Retrying");
                    runBot = false;
                    return;
                }
            }

            Thread loginGui = new Thread(Startup.LoginGui);
            loginGui.Start();

            while (!enteredDetails)
            {
                Thread.Sleep(250);
            }

            SteamUser.LogOnDetails userDetails = new SteamUser.LogOnDetails
            {
                Username = userName,
                Password = password,
            };

            steamUser.LogOn(userDetails);

        }

        private static void OnDisconnected(SteamClient.DisconnectedCallback callback)
        {
            MessageBox.Show("Disconnected from Steam");

            runBot = false;
        }

        private static void OnLoggedOn(SteamUser.LoggedOnCallback callback)
        {
            if (callback.Result != EResult.OK)
            {
                MessageBox.Show("Unable to logon to Steam: " + callback.Result + " / " + callback.ExtendedResult, "Steam Login Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Warning);

                runBot = false;
                return;
            }
            MessageBox.Show(callback.ClientSteamID + " is now logged in." ,"Login Completed");
        }
    }
}