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
            this.GCText = new System.Windows.Forms.Label();
            this.GCBox = new System.Windows.Forms.TextBox();
            this.GCButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // GCText
            // 
            this.GCText.AutoSize = true;
            this.GCText.Location = new System.Drawing.Point(12, 26);
            this.GCText.Name = "GCText";
            this.GCText.Size = new System.Drawing.Size(88, 13);
            this.GCText.TabIndex = 9;
            this.GCText.Text = "Group Message :";
            // 
            // GCBox
            // 
            this.GCBox.Location = new System.Drawing.Point(119, 23);
            this.GCBox.Name = "GCBox";
            this.GCBox.Size = new System.Drawing.Size(100, 20);
            this.GCBox.TabIndex = 12;
            // 
            // GCButton
            // 
            this.GCButton.Location = new System.Drawing.Point(246, 22);
            this.GCButton.Name = "GCButton";
            this.GCButton.Size = new System.Drawing.Size(75, 20);
            this.GCButton.TabIndex = 11;
            this.GCButton.Text = "Send";
            this.GCButton.UseVisualStyleBackColor = true;
            this.GCButton.Click += new System.EventHandler(this.GCButton_Click);
            // 
            // Gui
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(348, 75);
            this.Controls.Add(this.GCButton);
            this.Controls.Add(this.GCBox);
            this.Controls.Add(this.GCText);
            this.Name = "Gui";
            this.Text = "Bot Control";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label GCText;
        private System.Windows.Forms.TextBox GCBox;
        private System.Windows.Forms.Button GCButton;
    }
}