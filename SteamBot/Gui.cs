using System;
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
    }
}