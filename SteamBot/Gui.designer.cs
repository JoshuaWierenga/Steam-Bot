namespace SteamBot
{
    partial class Gui
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
            this.StateDropDown = new System.Windows.Forms.ComboBox();
            this.NameBox = new System.Windows.Forms.TextBox();
            this.NameText = new System.Windows.Forms.Label();
            this.StateText = new System.Windows.Forms.Label();
            this.NameButton = new System.Windows.Forms.Button();
            this.GroupDropDown = new System.Windows.Forms.ComboBox();
            this.GroupButton = new System.Windows.Forms.Button();
            this.GroupText = new System.Windows.Forms.Label();
            this.GCText = new System.Windows.Forms.Label();
            this.GCBox = new System.Windows.Forms.TextBox();
            this.GCButton = new System.Windows.Forms.Button();
            this.MFButton = new System.Windows.Forms.Button();
            this.MFText = new System.Windows.Forms.Label();
            this.MFDropDown = new System.Windows.Forms.ComboBox();
            this.MFTextBox = new System.Windows.Forms.TextBox();
            this.GroupMissingText = new System.Windows.Forms.Label();
            this.MFMissingText = new System.Windows.Forms.Label();
            this.IGCGroupDropDown = new System.Windows.Forms.ComboBox();
            this.IGCFriendDropDown = new System.Windows.Forms.ComboBox();
            this.IGCText = new System.Windows.Forms.Label();
            this.IGCButton = new System.Windows.Forms.Button();
            this.IGCMissingText = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // StateDropDown
            // 
            this.StateDropDown.BackColor = System.Drawing.SystemColors.ControlDark;
            this.StateDropDown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.StateDropDown.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.StateDropDown.FormattingEnabled = true;
            this.StateDropDown.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.StateDropDown.Items.AddRange(new object[] {
            "Online",
            "Offline",
            "Away",
            "Busy",
            "Snooze"});
            this.StateDropDown.Location = new System.Drawing.Point(99, 12);
            this.StateDropDown.Name = "StateDropDown";
            this.StateDropDown.Size = new System.Drawing.Size(156, 21);
            this.StateDropDown.TabIndex = 0;
            this.StateDropDown.SelectedIndexChanged += new System.EventHandler(this.State_SelectedIndexChanged);
            // 
            // NameBox
            // 
            this.NameBox.Location = new System.Drawing.Point(129, 58);
            this.NameBox.Name = "NameBox";
            this.NameBox.Size = new System.Drawing.Size(100, 20);
            this.NameBox.TabIndex = 1;
            // 
            // NameText
            // 
            this.NameText.AutoSize = true;
            this.NameText.Location = new System.Drawing.Point(26, 61);
            this.NameText.Name = "NameText";
            this.NameText.Size = new System.Drawing.Size(44, 13);
            this.NameText.TabIndex = 3;
            this.NameText.Text = "Name : ";
            // 
            // StateText
            // 
            this.StateText.AutoSize = true;
            this.StateText.Location = new System.Drawing.Point(26, 15);
            this.StateText.Name = "StateText";
            this.StateText.Size = new System.Drawing.Size(38, 13);
            this.StateText.TabIndex = 4;
            this.StateText.Text = "State :\r\n";
            // 
            // NameButton
            // 
            this.NameButton.Location = new System.Drawing.Point(277, 57);
            this.NameButton.Name = "NameButton";
            this.NameButton.Size = new System.Drawing.Size(75, 20);
            this.NameButton.TabIndex = 5;
            this.NameButton.Text = "Set Name";
            this.NameButton.UseVisualStyleBackColor = true;
            this.NameButton.Click += new System.EventHandler(this.NameButton_Click);
            // 
            // GroupDropDown
            // 
            this.GroupDropDown.BackColor = System.Drawing.SystemColors.ControlDark;
            this.GroupDropDown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.GroupDropDown.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.GroupDropDown.FormattingEnabled = true;
            this.GroupDropDown.Items.AddRange(new object[] {
            "Requiem"});
            this.GroupDropDown.Location = new System.Drawing.Point(129, 105);
            this.GroupDropDown.Name = "GroupDropDown";
            this.GroupDropDown.Size = new System.Drawing.Size(100, 21);
            this.GroupDropDown.TabIndex = 6;
            this.GroupDropDown.SelectedIndexChanged += new System.EventHandler(this.GroupDropDown_SelectedIndexChanged);
            // 
            // GroupButton
            // 
            this.GroupButton.Location = new System.Drawing.Point(277, 104);
            this.GroupButton.Name = "GroupButton";
            this.GroupButton.Size = new System.Drawing.Size(75, 20);
            this.GroupButton.TabIndex = 7;
            this.GroupButton.Text = "Join Group\r\n";
            this.GroupButton.UseVisualStyleBackColor = true;
            this.GroupButton.Click += new System.EventHandler(this.GroupButton_Click);
            // 
            // GroupText
            // 
            this.GroupText.AutoSize = true;
            this.GroupText.Location = new System.Drawing.Point(26, 108);
            this.GroupText.Name = "GroupText";
            this.GroupText.Size = new System.Drawing.Size(42, 13);
            this.GroupText.TabIndex = 8;
            this.GroupText.Text = "Group :";
            // 
            // GCText
            // 
            this.GCText.AutoSize = true;
            this.GCText.Location = new System.Drawing.Point(12, 152);
            this.GCText.Name = "GCText";
            this.GCText.Size = new System.Drawing.Size(88, 13);
            this.GCText.TabIndex = 9;
            this.GCText.Text = "Group Message :";
            // 
            // GCBox
            // 
            this.GCBox.Location = new System.Drawing.Point(129, 149);
            this.GCBox.Name = "GCBox";
            this.GCBox.Size = new System.Drawing.Size(100, 20);
            this.GCBox.TabIndex = 12;
            // 
            // GCButton
            // 
            this.GCButton.Location = new System.Drawing.Point(277, 148);
            this.GCButton.Name = "GCButton";
            this.GCButton.Size = new System.Drawing.Size(75, 20);
            this.GCButton.TabIndex = 11;
            this.GCButton.Text = "Send";
            this.GCButton.UseVisualStyleBackColor = true;
            this.GCButton.Click += new System.EventHandler(this.GCButton_Click);
            // 
            // MFButton
            // 
            this.MFButton.Location = new System.Drawing.Point(277, 201);
            this.MFButton.Name = "MFButton";
            this.MFButton.Size = new System.Drawing.Size(75, 23);
            this.MFButton.TabIndex = 13;
            this.MFButton.Text = "Message";
            this.MFButton.UseVisualStyleBackColor = true;
            this.MFButton.Click += new System.EventHandler(this.InviteFriendButton_Click);
            // 
            // MFText
            // 
            this.MFText.Location = new System.Drawing.Point(12, 201);
            this.MFText.Name = "MFText";
            this.MFText.Size = new System.Drawing.Size(138, 23);
            this.MFText.TabIndex = 0;
            this.MFText.Text = "Message user :\r\n";
            // 
            // MFDropDown
            // 
            this.MFDropDown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.MFDropDown.FormattingEnabled = true;
            this.MFDropDown.Location = new System.Drawing.Point(99, 189);
            this.MFDropDown.Name = "MFDropDown";
            this.MFDropDown.Size = new System.Drawing.Size(156, 21);
            this.MFDropDown.TabIndex = 14;
            // 
            // MFTextBox
            // 
            this.MFTextBox.Location = new System.Drawing.Point(99, 218);
            this.MFTextBox.Name = "MFTextBox";
            this.MFTextBox.Size = new System.Drawing.Size(156, 20);
            this.MFTextBox.TabIndex = 15;
            // 
            // GroupMissingText
            // 
            this.GroupMissingText.AutoSize = true;
            this.GroupMissingText.Location = new System.Drawing.Point(65, 129);
            this.GroupMissingText.Name = "GroupMissingText";
            this.GroupMissingText.Size = new System.Drawing.Size(250, 13);
            this.GroupMissingText.TabIndex = 16;
            this.GroupMissingText.Text = "Send !chatrefresh to the bot to download group info";
            // 
            // MFMissingText
            // 
            this.MFMissingText.AutoSize = true;
            this.MFMissingText.Location = new System.Drawing.Point(61, 201);
            this.MFMissingText.Name = "MFMissingText";
            this.MFMissingText.Size = new System.Drawing.Size(254, 13);
            this.MFMissingText.TabIndex = 17;
            this.MFMissingText.Text = "Send !friendrefresh to the bot to download friend info";
            // 
            // IGCGroupDropDown
            // 
            this.IGCGroupDropDown.BackColor = System.Drawing.SystemColors.ControlDark;
            this.IGCGroupDropDown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.IGCGroupDropDown.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.IGCGroupDropDown.FormattingEnabled = true;
            this.IGCGroupDropDown.Items.AddRange(new object[] {
            "Requiem"});
            this.IGCGroupDropDown.Location = new System.Drawing.Point(129, 255);
            this.IGCGroupDropDown.Name = "IGCGroupDropDown";
            this.IGCGroupDropDown.Size = new System.Drawing.Size(100, 21);
            this.IGCGroupDropDown.TabIndex = 18;
            // 
            // IGCFriendDropDown
            // 
            this.IGCFriendDropDown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.IGCFriendDropDown.FormattingEnabled = true;
            this.IGCFriendDropDown.Location = new System.Drawing.Point(99, 302);
            this.IGCFriendDropDown.Name = "IGCFriendDropDown";
            this.IGCFriendDropDown.Size = new System.Drawing.Size(156, 21);
            this.IGCFriendDropDown.TabIndex = 19;
            // 
            // IGCText
            // 
            this.IGCText.Location = new System.Drawing.Point(1, 276);
            this.IGCText.Name = "IGCText";
            this.IGCText.Size = new System.Drawing.Size(132, 23);
            this.IGCText.TabIndex = 20;
            this.IGCText.Text = "Invite user to group chat :";
            this.IGCText.Click += new System.EventHandler(this.label1_Click);
            // 
            // IGCButton
            // 
            this.IGCButton.Location = new System.Drawing.Point(277, 271);
            this.IGCButton.Name = "IGCButton";
            this.IGCButton.Size = new System.Drawing.Size(75, 23);
            this.IGCButton.TabIndex = 21;
            this.IGCButton.Text = "Invite";
            this.IGCButton.UseVisualStyleBackColor = true;
            this.IGCButton.Click += new System.EventHandler(this.IGCButton_Click);
            // 
            // IGCMissingText
            // 
            this.IGCMissingText.AutoSize = true;
            this.IGCMissingText.Location = new System.Drawing.Point(26, 279);
            this.IGCMissingText.Name = "IGCMissingText";
            this.IGCMissingText.Size = new System.Drawing.Size(346, 13);
            this.IGCMissingText.TabIndex = 22;
            this.IGCMissingText.Text = "Send !chatrefresh and !friendrefresh to the bot to download required info";
            // 
            // Gui
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(388, 331);
            this.Controls.Add(this.IGCMissingText);
            this.Controls.Add(this.IGCButton);
            this.Controls.Add(this.IGCText);
            this.Controls.Add(this.IGCFriendDropDown);
            this.Controls.Add(this.IGCGroupDropDown);
            this.Controls.Add(this.MFMissingText);
            this.Controls.Add(this.GroupMissingText);
            this.Controls.Add(this.MFTextBox);
            this.Controls.Add(this.MFDropDown);
            this.Controls.Add(this.MFText);
            this.Controls.Add(this.MFButton);
            this.Controls.Add(this.GCButton);
            this.Controls.Add(this.GCBox);
            this.Controls.Add(this.GCText);
            this.Controls.Add(this.GroupText);
            this.Controls.Add(this.GroupButton);
            this.Controls.Add(this.GroupDropDown);
            this.Controls.Add(this.NameButton);
            this.Controls.Add(this.StateText);
            this.Controls.Add(this.NameText);
            this.Controls.Add(this.NameBox);
            this.Controls.Add(this.StateDropDown);
            this.Name = "Gui";
            this.Text = "Bot Control";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox StateDropDown;
        private System.Windows.Forms.TextBox NameBox;
        private System.Windows.Forms.Label NameText;
        private System.Windows.Forms.Label StateText;
        private System.Windows.Forms.Button NameButton;
        private System.Windows.Forms.ComboBox GroupDropDown;
        private System.Windows.Forms.Button GroupButton;
        private System.Windows.Forms.Label GroupText;
        private System.Windows.Forms.Label GCText;
        private System.Windows.Forms.TextBox GCBox;
        private System.Windows.Forms.Button GCButton;
        private System.Windows.Forms.Button MFButton;
        private System.Windows.Forms.Label MFText;
        private System.Windows.Forms.ComboBox MFDropDown;
        private System.Windows.Forms.TextBox MFTextBox;
        private System.Windows.Forms.Label GroupMissingText;
        private System.Windows.Forms.Label MFMissingText;
        private System.Windows.Forms.ComboBox IGCGroupDropDown;
        private System.Windows.Forms.ComboBox IGCFriendDropDown;
        private System.Windows.Forms.Label IGCText;
        private System.Windows.Forms.Button IGCButton;
        private System.Windows.Forms.Label IGCMissingText;
    }
}