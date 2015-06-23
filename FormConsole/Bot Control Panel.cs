using System;
using System.IO;
using System.Windows.Forms;

namespace FormConsole

{
    public partial class Bot_Control_Panel : Form
    {
        

        public Bot_Control_Panel()
        {
            InitializeComponent();           
            if (File.Exists("friendList.txt"))
            {
                string[] friendlist;
                friendlist = File.ReadAllLines("friendList.txt");
                MFDropDown.DataSource = friendlist;
            }
            else
            {
                MFDropDown.Visible = false;
                MFText.Visible = false;
                MFButton.Visible = false;
                MFTextBox.Visible = false;
            }
            if (File.Exists("clanList.txt"))
            {
                string[] clanList;
                clanList = File.ReadAllLines("clanList.txt");
                GroupDropDown.DataSource = clanList;
            }
            else
            {
                GroupDropDown.Visible = false;
                GroupText.Visible = false;
                GroupButton.Visible = false;
            }

        }

        private void State_SelectedIndexChanged(object sender, EventArgs e)
        {
            SteamBot.StatePanel(StateDropDown.SelectedItem.ToString());
        }

        private void NameButton_Click(object sender, EventArgs e)
        {
            SteamBot.NamePanel(NameBox.Text);
        }

        private void GroupButton_Click(object sender, EventArgs e)
        {
            SteamBot.GroupPanel(GroupDropDown.SelectedItem.ToString());
        }

        private void GCButton_Click(object sender, EventArgs e)
        {
            SteamBot.GroupMessagePanel(GroupDropDown.SelectedItem.ToString(), GCBox.Text);
        }

        private void InviteFriendButton_Click(object sender, EventArgs e)
        {
            var steamid = UInt64.Parse(SteamBot.nicknametoSteamIdPanel(MFDropDown.SelectedItem.ToString()));
            SteamBot.MessageFriendPanel(steamid, MFTextBox.Text);
        }

        private void GroupDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
       
}
