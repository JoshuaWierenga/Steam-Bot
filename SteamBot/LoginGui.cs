using System;
using System.Windows.Forms;

namespace SteamBot
{
    public partial class LoginGui : Form
    {
        public LoginGui()
        {
            InitializeComponent();
        }

        private void OkLoginButton_Click(object sender, EventArgs e)
        {
            if (UsernameTextBox.Text != "" && PasswordTextBox.Text != "")
            {
                SteamBot.userName = UsernameTextBox.Text;
                SteamBot.password = PasswordTextBox.Text;
                SteamBot.enteredDetails = true;
                OkLoginButton.Visible = false;
                CancelLoginButton.Visible = false;
                UsernameTextBox.Visible = false;
                PasswordTextBox.Visible = false;
                UsernameLabel.Visible = false;
                PasswordLabel.Visible = false;
                RequestLabel.Visible = false;
                LoggingInLabel.Visible = true;

            }
        }

        private void CancelLoginButton_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }
    }
}
