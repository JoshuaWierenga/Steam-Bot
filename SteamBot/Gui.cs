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
                GroupMissingText.Visible = false;
                string[] file = File.ReadAllLines("groupList.txt");
                List<string> grouplist = new List<string>();
                foreach (var line in file)
                {
                    string[] seperatedLine = SteamBot.seperate(1, '✏', line);
                    grouplist.Add(seperatedLine[0]);
                }
                GroupDropDown.DataSource = grouplist;
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
                /*IGCMissingText.Visible = false;

                string[] friendfile = File.ReadAllLines("friendList.txt");
                List<string> friendlist = new List<string>();
                foreach (var line in friendfile)
                {
                    string[] seperatedLine = SteamBot.seperate(1, '✏', line);
                    friendlist.Add(seperatedLine[0]);
                }
                IGCFriendDropDown.DataSource = friendlist;

                string[] groupfile = File.ReadAllLines("groupList.txt");
                List<string> grouplist = new List<string>();
                foreach (var line in groupfile)
                {
                    string[] seperatedLine = SteamBot.seperate(1, '✏', line);
                    grouplist.Add(seperatedLine[0]);
                }
                IGCGroupDropDown.DataSource = grouplist;*/
                IGCText.Visible = false;
                IGCFriendDropDown.Visible = false;
                IGCGroupDropDown.Visible = false;
                IGCButton.Visible = false;
                IGCMissingText.Visible = true;
                IGCMissingText.Text = "Group messaging is currently broken, will be fixed soon!!!";
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
            SteamBot.SetState(StateDropDown.SelectedItem.ToString());
        }

        private void NameButton_Click(object sender, EventArgs e)
        {
            SteamBot.SetName(NameBox.Text);
        }

        private void GroupButton_Click(object sender, EventArgs e)
        {
            var groupid = SteamBot.NamestosteamId(GroupDropDown.SelectedItem.ToString(), "groupList.txt", 1);
            ulong id;
            bool issteamid = UInt64.TryParse(groupid.ToString(), out id);
            if (issteamid)
            {
                var steamid = UInt64.Parse(groupid);
                SteamBot.JoinGroupChat(steamid);
            }
            
        }

        private void GCButton_Click(object sender, EventArgs e)
        {
            //SteamBot.GroupMessagePanel(GroupDropDown.SelectedItem.ToString(), GCBox.Text);
            var groupid = UInt64.Parse(SteamBot.NamestosteamId(GroupDropDown.SelectedItem.ToString(), "groupList.txt", 1));
            SteamBot.JoinGroupChat(groupid);
            SteamBot.GroupMessage(groupid, GCBox.Text);
        }

        private void InviteFriendButton_Click(object sender, EventArgs e)
        {
            var steamid = UInt64.Parse(SteamBot.NamestosteamId(MFDropDown.SelectedItem.ToString(), "friendList.txt", 0));
            SteamBot.PrivateMessage(steamid, MFTextBox.Text);
        }

        private void GroupDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void IGCButton_Click(object sender, EventArgs e)
        {
            var steamid = UInt64.Parse(SteamBot.NamestosteamId(IGCFriendDropDown.SelectedItem.ToString(), "friendList.txt", 0));
            var groupid = UInt64.Parse(SteamBot.NamestosteamId(IGCGroupDropDown.SelectedItem.ToString(), "groupList.txt", 1));
            SteamBot.GroupChatInviter(steamid,groupid);
        }
    }
}
