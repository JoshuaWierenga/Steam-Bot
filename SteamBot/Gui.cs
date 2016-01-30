using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace SteamBot
{
    public partial class Gui : Form
    {
        public Gui()
        {
            InitializeComponent();
        }

        private void GCButton_Click(object sender, EventArgs e)
        {
            SteamBot.steamFriends.SendChatRoomMessage(SteamBot.config.groupID, SteamKit2.EChatEntryType.ChatMsg, GCBox.Text);
        }

        private void Kick_Click(object sender, EventArgs e)
        {
            Console.WriteLine(UserList.SelectedItem);
            if (SteamBot.isUserinChat(SteamBot.nameToID(UserList.SelectedItem.ToString())))
            {
                Console.WriteLine("kicking");
                SteamBot.steamFriends.KickChatMember(SteamBot.config.groupID, SteamBot.nameToID(UserList.SelectedItem.ToString()));
            }
        }

        private void Ban_Click(object sender, EventArgs e)
        {
            if (SteamBot.isUserinChat(SteamBot.nameToID(UserList.SelectedItem.ToString())))
            {
                SteamBot.steamFriends.BanChatMember(SteamBot.config.groupID, SteamBot.nameToID(UserList.SelectedItem.ToString()));
            }
        }

        delegate void SetUserListCallback(List<string> userList);

        public void SetUserList(List<string> userList)
        {
            if (UserList.InvokeRequired)
            {
                SetUserListCallback d = new SetUserListCallback(SetUserList);
                Invoke(d, new object[] { userList });
            }
            else
            {
                UserList.DataSource = userList;
            }

        }
    }
}