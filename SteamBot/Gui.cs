using System;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;

namespace SteamBot

{
    public partial class Gui : Form
    {


        public Gui()
        {
            InitializeComponent();
            if (File.Exists("friendList.txt"))
            {
                MFMissingText.Visible = false;
                string[] file = File.ReadAllLines("friendList.txt");
                List<string> friendlist = new List<string>();
                foreach (var line in file)
                {
                    string[] seperatedLine = SteamBot.seperate(1, '✏', line);
                    friendlist.Add(seperatedLine[0]);            
                }
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
            if (File.Exists("groupList.txt"))
            {
                //GroupMissingText.Visible = false;
                //string[] grouplist = SteamBot.seperate(1, ' ', File.ReadAllLines("groupList.txt").ToString());
                //GroupDropDown.DataSource =  grouplist;
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
            if (File.Exists("groupList.txt") && (File.Exists("friendList.txt")))
            {
                //IGCMissingText.Visible = false;
                //IGCFriendDropDown.DataSource = File.ReadAllLines("friendList.txt");
                //IGCGroupDropDown.DataSource = File.ReadAllLines("groupList.txt");
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
            var groupid = UInt64.Parse(SteamBot.namestosteamId(GroupDropDown.SelectedItem.ToString(), "groupList.txt"));
            SteamBot.GroupChatPanel(groupid);
        }

        private void GCButton_Click(object sender, EventArgs e)
        {
            //SteamBot.GroupMessagePanel(GroupDropDown.SelectedItem.ToString(), GCBox.Text);
            var groupid = UInt64.Parse(SteamBot.namestosteamId(GroupDropDown.SelectedItem.ToString(), "groupList.txt"));
            SteamBot.GroupChatPanel(groupid);
            SteamBot.GroupMessagePanel(groupid, GCBox.Text);
        }

        private void InviteFriendButton_Click(object sender, EventArgs e)
        {
            var steamid = UInt64.Parse(SteamBot.namestosteamId(MFDropDown.SelectedItem.ToString(), "friendList.txt"));
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
            var steamid = UInt64.Parse(SteamBot.namestosteamId(IGCFriendDropDown.SelectedItem.ToString(), "friendList.txt"));
            var groupid = UInt64.Parse(SteamBot.namestosteamId(IGCGroupDropDown.SelectedItem.ToString(), "groupList.txt"));
            SteamBot.InvitetoGroupChat(steamid,groupid);
        }
    }
}
