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
                MFMissingText.Visible = false;
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
                MFMissingText.Visible = true;
            }
            if (File.Exists("clanList.txt"))
            {
                GroupMissingText.Visible = false;
                string[] clanList;
                clanList = File.ReadAllLines("clanList.txt");
                GroupDropDown.DataSource = clanList;
            }
            else
            {
                GroupDropDown.Visible = false;
                GroupText.Visible = false;
                GroupButton.Visible = false;
                GCButton.Visible = false;
                GCBox.Visible = false;
                GCText.Visible = false;
                GroupMissingText.Visible = true;
            }
            if (File.Exists("clanList.txt") && (File.Exists("friendList.txt")))
            {
                IGCMissingText.Visible = false;
                IGCFriendDropDown.DataSource = File.ReadAllLines("friendList.txt");
                IGCGroupDropDown.DataSource = File.ReadAllLines("clanList.txt");
            }
            else
            {
                IGCText.Visible = false;
                IGCFriendDropDown.Visible = false;
                IGCGroupDropDown.Visible = false;
                IGCButton.Visible = false;
                IGCMissingText.Visible = true;
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
            var groupid = UInt64.Parse(SteamBot.groupnametogroupIdPanel(GroupDropDown.SelectedItem.ToString()));
            SteamBot.GroupChatPanel(groupid);
        }

        private void GCButton_Click(object sender, EventArgs e)
        {
            //SteamBot.GroupMessagePanel(GroupDropDown.SelectedItem.ToString(), GCBox.Text);
            var groupid = UInt64.Parse(SteamBot.groupnametogroupIdPanel(GroupDropDown.SelectedItem.ToString()));
            SteamBot.GroupChatPanel(groupid);
            SteamBot.GroupMessagePanel(groupid, GCBox.Text);
        }

        private void InviteFriendButton_Click(object sender, EventArgs e)
        {
            var steamid = UInt64.Parse(SteamBot.nicknametoSteamIdPanel(MFDropDown.SelectedItem.ToString()));
            SteamBot.MessageFriendPanel(steamid, MFTextBox.Text);
        }

        private void GroupDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void IGCButton_Click(object sender, EventArgs e)
        {
            var steamid = UInt64.Parse(SteamBot.nicknametoSteamIdPanel(IGCFriendDropDown.SelectedItem.ToString()));
            var groupid = UInt64.Parse(SteamBot.groupnametogroupIdPanel(IGCGroupDropDown.SelectedItem.ToString()));
            SteamBot.InvitetoGroupChat(steamid,groupid);
        }
    }
}
