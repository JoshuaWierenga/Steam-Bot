namespace SteamBot
{
    partial class LoginGui
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.OkLoginButton = new System.Windows.Forms.Button();
            this.CancelLoginButton = new System.Windows.Forms.Button();
            this.UsernameTextBox = new System.Windows.Forms.TextBox();
            this.PasswordTextBox = new System.Windows.Forms.TextBox();
            this.UsernameLabel = new System.Windows.Forms.Label();
            this.PasswordLabel = new System.Windows.Forms.Label();
            this.RequestLabel = new System.Windows.Forms.Label();
            this.LoggingInLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // OkLoginButton
            // 
            this.OkLoginButton.Location = new System.Drawing.Point(37, 101);
            this.OkLoginButton.Name = "OkLoginButton";
            this.OkLoginButton.Size = new System.Drawing.Size(75, 23);
            this.OkLoginButton.TabIndex = 0;
            this.OkLoginButton.Text = "Login";
            this.OkLoginButton.UseVisualStyleBackColor = true;
            this.OkLoginButton.Click += new System.EventHandler(this.OkLoginButton_Click);
            // 
            // CancelLoginButton
            // 
            this.CancelLoginButton.Location = new System.Drawing.Point(146, 101);
            this.CancelLoginButton.Name = "CancelLoginButton";
            this.CancelLoginButton.Size = new System.Drawing.Size(75, 23);
            this.CancelLoginButton.TabIndex = 1;
            this.CancelLoginButton.Text = "Cancel";
            this.CancelLoginButton.UseVisualStyleBackColor = true;
            this.CancelLoginButton.Click += new System.EventHandler(this.CancelLoginButton_Click);
            // 
            // UsernameTextBox
            // 
            this.UsernameTextBox.Location = new System.Drawing.Point(80, 32);
            this.UsernameTextBox.Name = "UsernameTextBox";
            this.UsernameTextBox.Size = new System.Drawing.Size(162, 20);
            this.UsernameTextBox.TabIndex = 2;
            // 
            // PasswordTextBox
            // 
            this.PasswordTextBox.Location = new System.Drawing.Point(80, 62);
            this.PasswordTextBox.Name = "PasswordTextBox";
            this.PasswordTextBox.Size = new System.Drawing.Size(162, 20);
            this.PasswordTextBox.TabIndex = 3;
            // 
            // UsernameLabel
            // 
            this.UsernameLabel.AutoSize = true;
            this.UsernameLabel.Location = new System.Drawing.Point(15, 35);
            this.UsernameLabel.Name = "UsernameLabel";
            this.UsernameLabel.Size = new System.Drawing.Size(58, 13);
            this.UsernameLabel.TabIndex = 4;
            this.UsernameLabel.Text = "Username:";
            // 
            // PasswordLabel
            // 
            this.PasswordLabel.AutoSize = true;
            this.PasswordLabel.Location = new System.Drawing.Point(15, 65);
            this.PasswordLabel.Name = "PasswordLabel";
            this.PasswordLabel.Size = new System.Drawing.Size(56, 13);
            this.PasswordLabel.TabIndex = 5;
            this.PasswordLabel.Text = "Password:";
            // 
            // RequestLabel
            // 
            this.RequestLabel.AutoSize = true;
            this.RequestLabel.Location = new System.Drawing.Point(10, 10);
            this.RequestLabel.Name = "RequestLabel";
            this.RequestLabel.Size = new System.Drawing.Size(238, 13);
            this.RequestLabel.TabIndex = 6;
            this.RequestLabel.Text = "Please enter your steam username and password";
            // 
            // LoggingInLabel
            // 
            this.LoggingInLabel.AutoSize = true;
            this.LoggingInLabel.Location = new System.Drawing.Point(77, 55);
            this.LoggingInLabel.Name = "LoggingInLabel";
            this.LoggingInLabel.Size = new System.Drawing.Size(99, 13);
            this.LoggingInLabel.TabIndex = 7;
            this.LoggingInLabel.Text = "Logging Into Steam";
            this.LoggingInLabel.Visible = false;
            // 
            // LoginGui
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(257, 136);
            this.Controls.Add(this.LoggingInLabel);
            this.Controls.Add(this.RequestLabel);
            this.Controls.Add(this.PasswordLabel);
            this.Controls.Add(this.UsernameLabel);
            this.Controls.Add(this.PasswordTextBox);
            this.Controls.Add(this.UsernameTextBox);
            this.Controls.Add(this.CancelLoginButton);
            this.Controls.Add(this.OkLoginButton);
            this.Name = "LoginGui";
            this.Text = "Steam Login";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button OkLoginButton;
        private System.Windows.Forms.Button CancelLoginButton;
        private System.Windows.Forms.TextBox UsernameTextBox;
        private System.Windows.Forms.TextBox PasswordTextBox;
        private System.Windows.Forms.Label UsernameLabel;
        private System.Windows.Forms.Label PasswordLabel;
        private System.Windows.Forms.Label RequestLabel;
        private System.Windows.Forms.Label LoggingInLabel;
    }
}

