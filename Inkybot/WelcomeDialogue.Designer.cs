using System.ComponentModel;

namespace Inkybot
{
    partial class WelcomeDialogue
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

        private System.ComponentModel.ComponentResourceManager resources;

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
        private void InitializeComponent() {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WelcomeDialogue));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.linkLabel2 = new System.Windows.Forms.LinkLabel();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.logoPictureBox = new System.Windows.Forms.PictureBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.switchLanguageLabel = new System.Windows.Forms.LinkLabel();
            this.newVersionLabel = new System.Windows.Forms.LinkLabel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.label3 = new System.Windows.Forms.Label();
            this.usernameTextBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.passwordTextBox = new System.Windows.Forms.TextBox();
            this.rememberPasswordCheckbox = new System.Windows.Forms.CheckBox();
            this.button1 = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.errorMessage = new System.Windows.Forms.Label();
            this.dofusPathLink = new System.Windows.Forms.LinkLabel();
            ((System.ComponentModel.ISupportInitialize) (this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize) (this.logoPictureBox)).BeginInit();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            resources.ApplyResources(this.splitContainer1, "splitContainer1");
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            resources.ApplyResources(this.splitContainer1.Panel1, "splitContainer1.Panel1");
            this.splitContainer1.Panel1.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (15)))), ((int) (((byte) (15)))), ((int) (((byte) (15)))));
            this.splitContainer1.Panel1.Controls.Add(this.linkLabel2);
            this.splitContainer1.Panel1.Controls.Add(this.label2);
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            this.splitContainer1.Panel1.Controls.Add(this.logoPictureBox);
            // 
            // splitContainer1.Panel2
            // 
            resources.ApplyResources(this.splitContainer1.Panel2, "splitContainer1.Panel2");
            this.splitContainer1.Panel2.Controls.Add(this.panel1);
            // 
            // linkLabel2
            // 
            resources.ApplyResources(this.linkLabel2, "linkLabel2");
            this.linkLabel2.ActiveLinkColor = System.Drawing.Color.FromArgb(((int) (((byte) (150)))), ((int) (((byte) (150)))), ((int) (((byte) (150)))));
            this.linkLabel2.LinkColor = System.Drawing.Color.White;
            this.linkLabel2.Name = "linkLabel2";
            this.linkLabel2.TabStop = true;
            this.linkLabel2.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel2_LinkClicked);
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.ForeColor = System.Drawing.SystemColors.Control;
            this.label2.Name = "label2";
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.ForeColor = System.Drawing.SystemColors.Control;
            this.label1.Name = "label1";
            // 
            // logoPictureBox
            // 
            resources.ApplyResources(this.logoPictureBox, "logoPictureBox");
            this.logoPictureBox.Name = "logoPictureBox";
            this.logoPictureBox.TabStop = false;
            // 
            // panel1
            // 
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Controls.Add(this.flowLayoutPanel1);
            this.panel1.Name = "panel1";
            // 
            // panel2
            // 
            resources.ApplyResources(this.panel2, "panel2");
            this.panel2.Controls.Add(this.switchLanguageLabel);
            this.panel2.Controls.Add(this.newVersionLabel);
            this.panel2.Name = "panel2";
            // 
            // switchLanguageLabel
            // 
            resources.ApplyResources(this.switchLanguageLabel, "switchLanguageLabel");
            this.switchLanguageLabel.ActiveLinkColor = System.Drawing.Color.Black;
            this.switchLanguageLabel.LinkColor = System.Drawing.Color.FromArgb(((int) (((byte) (50)))), ((int) (((byte) (50)))), ((int) (((byte) (50)))));
            this.switchLanguageLabel.Name = "switchLanguageLabel";
            this.switchLanguageLabel.TabStop = true;
            this.switchLanguageLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.switchLanguageLabel_LinkClicked);
            // 
            // newVersionLabel
            // 
            resources.ApplyResources(this.newVersionLabel, "newVersionLabel");
            this.newVersionLabel.ActiveLinkColor = System.Drawing.Color.Black;
            this.newVersionLabel.LinkColor = System.Drawing.Color.FromArgb(((int) (((byte) (50)))), ((int) (((byte) (50)))), ((int) (((byte) (50)))));
            this.newVersionLabel.Name = "newVersionLabel";
            this.newVersionLabel.TabStop = true;
            this.newVersionLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.newVersionLabel_LinkClicked);
            // 
            // flowLayoutPanel1
            // 
            resources.ApplyResources(this.flowLayoutPanel1, "flowLayoutPanel1");
            this.flowLayoutPanel1.Controls.Add(this.label3);
            this.flowLayoutPanel1.Controls.Add(this.usernameTextBox);
            this.flowLayoutPanel1.Controls.Add(this.label4);
            this.flowLayoutPanel1.Controls.Add(this.passwordTextBox);
            this.flowLayoutPanel1.Controls.Add(this.rememberPasswordCheckbox);
            this.flowLayoutPanel1.Controls.Add(this.button1);
            this.flowLayoutPanel1.Controls.Add(this.label5);
            this.flowLayoutPanel1.Controls.Add(this.linkLabel1);
            this.flowLayoutPanel1.Controls.Add(this.errorMessage);
            this.flowLayoutPanel1.Controls.Add(this.dofusPathLink);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // usernameTextBox
            // 
            resources.ApplyResources(this.usernameTextBox, "usernameTextBox");
            this.usernameTextBox.Name = "usernameTextBox";
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            // 
            // passwordTextBox
            // 
            resources.ApplyResources(this.passwordTextBox, "passwordTextBox");
            this.passwordTextBox.Name = "passwordTextBox";
            this.passwordTextBox.UseSystemPasswordChar = true;
            // 
            // rememberPasswordCheckbox
            // 
            resources.ApplyResources(this.rememberPasswordCheckbox, "rememberPasswordCheckbox");
            this.rememberPasswordCheckbox.Name = "rememberPasswordCheckbox";
            this.rememberPasswordCheckbox.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            resources.ApplyResources(this.button1, "button1");
            this.button1.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (15)))), ((int) (((byte) (15)))), ((int) (((byte) (15)))));
            this.button1.ForeColor = System.Drawing.SystemColors.Control;
            this.button1.Name = "button1";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label5
            // 
            resources.ApplyResources(this.label5, "label5");
            this.label5.Name = "label5";
            // 
            // linkLabel1
            // 
            resources.ApplyResources(this.linkLabel1, "linkLabel1");
            this.linkLabel1.ActiveLinkColor = System.Drawing.Color.Black;
            this.linkLabel1.LinkColor = System.Drawing.Color.FromArgb(((int) (((byte) (50)))), ((int) (((byte) (50)))), ((int) (((byte) (50)))));
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Click += new System.EventHandler(this.linkLabel1_LinkClicked_1);
            // 
            // errorMessage
            // 
            resources.ApplyResources(this.errorMessage, "errorMessage");
            this.errorMessage.ForeColor = System.Drawing.Color.Maroon;
            this.errorMessage.Name = "errorMessage";
            // 
            // dofusPathLink
            // 
            resources.ApplyResources(this.dofusPathLink, "dofusPathLink");
            this.dofusPathLink.ActiveLinkColor = System.Drawing.Color.Black;
            this.dofusPathLink.LinkColor = System.Drawing.Color.FromArgb(((int) (((byte) (50)))), ((int) (((byte) (50)))), ((int) (((byte) (50)))));
            this.dofusPathLink.Name = "dofusPathLink";
            this.dofusPathLink.TabStop = true;
            this.dofusPathLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.dofusPathLink_LinkClicked);
            // 
            // WelcomeDialogue
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.Controls.Add(this.splitContainer1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "WelcomeDialogue";
            this.Load += new System.EventHandler(this.LoginForm_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize) (this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize) (this.logoPictureBox)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.LinkLabel switchLanguageLabel;

        private System.Windows.Forms.Panel panel2;

        private System.Windows.Forms.CheckBox rememberPasswordCheckbox;

        private System.Windows.Forms.LinkLabel dofusPathLink;

        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;

        private System.Windows.Forms.LinkLabel linkLabel2;

        private System.Windows.Forms.LinkLabel newVersionLabel;

        private System.Windows.Forms.PictureBox logoPictureBox;

        private System.Windows.Forms.Label errorMessage;

        private System.Windows.Forms.TextBox passwordTextBox;
        private System.Windows.Forms.TextBox usernameTextBox;

        private System.Windows.Forms.Label label5;

        private System.Windows.Forms.LinkLabel linkLabel1;

        private System.Windows.Forms.Button button1;

        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Panel panel1;

        private System.Windows.Forms.Label label3;

        private System.Windows.Forms.Label label2;

        private System.Windows.Forms.Label label1;

        private System.Windows.Forms.SplitContainer splitContainer1;

        #endregion
    }
}