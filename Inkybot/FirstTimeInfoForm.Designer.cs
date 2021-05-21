using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Inkybot
{
    partial class FirstTimeInfoForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

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
        private ComponentResourceManager resources;

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            resources = new System.ComponentModel.ComponentResourceManager(typeof(FirstTimeInfoForm));
            this.components = new System.ComponentModel.Container();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.logoPictureBox = new System.Windows.Forms.PictureBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.bottomPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.discordLabel = new System.Windows.Forms.Label();
            this.greetingsLabel = new System.Windows.Forms.Label();
            this.usageInstructionsLinkLabel = new System.Windows.Forms.LinkLabel();
            this.discordLinkLabel = new System.Windows.Forms.LinkLabel();
            this.continueButton = new System.Windows.Forms.Button();
            this.enableContinueTimer = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize) (this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize) (this.logoPictureBox)).BeginInit();
            this.panel1.SuspendLayout();
            this.bottomPanel.SuspendLayout();
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
            this.splitContainer1.Panel1.Controls.Add(this.logoPictureBox);
            this.splitContainer1.Panel1.Controls.Add(this.label2);
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            // 
            // splitContainer1.Panel2
            // 
            resources.ApplyResources(this.splitContainer1.Panel2, "splitContainer1.Panel2");
            this.splitContainer1.Panel2.Controls.Add(this.panel1);
            // 
            // enableContinueTimer
            // 
            this.enableContinueTimer.Interval = 1000;
            this.enableContinueTimer.Enabled = false;
            this.enableContinueTimer.Tick += OnEnableContinueTimerTick;
            // 
            // logoPictureBox
            // 
            resources.ApplyResources(this.logoPictureBox, "logoPictureBox");
            this.logoPictureBox.Name = "logoPictureBox";
            this.logoPictureBox.TabStop = false;
            // 
            // usageInstructionsLinkLabel
            // 
            resources.ApplyResources(this.usageInstructionsLinkLabel, "usageInstructionsLinkLabel");
            this.usageInstructionsLinkLabel.LinkColor = System.Drawing.SystemColors.ControlDarkDark;
            this.usageInstructionsLinkLabel.ActiveLinkColor = System.Drawing.SystemColors.ControlDark;
            this.usageInstructionsLinkLabel.Name = "usageInstructionsLinkLabel";
            this.usageInstructionsLinkLabel.Dock = DockStyle.Top;
            this.usageInstructionsLinkLabel.Padding = Padding.Empty;
            this.usageInstructionsLinkLabel.Margin = new Padding(0, 4, 0, 0);
            this.usageInstructionsLinkLabel.Click += OnUsageInstructionsLinkLabelClick;
            // 
            // discordLinkLabel
            // 
            resources.ApplyResources(this.discordLinkLabel, "discordLinkLabel");
            this.discordLinkLabel.LinkColor = System.Drawing.SystemColors.ControlDarkDark;
            this.discordLinkLabel.ActiveLinkColor = System.Drawing.SystemColors.ControlDark;
            this.discordLinkLabel.Name = "discordLinkLabel";
            this.discordLinkLabel.Dock = DockStyle.Top;
            this.discordLinkLabel.Padding = Padding.Empty;
            this.discordLinkLabel.Margin = new Padding(0, 4, 0, 0);
            this.discordLinkLabel.Click += OnDiscordLinkLabelClick;
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
            // panel1
            // 
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Controls.Add(this.discordLinkLabel);
            this.panel1.Controls.Add(this.discordLabel);
            this.panel1.Controls.Add(this.usageInstructionsLinkLabel);
            this.panel1.Controls.Add(this.greetingsLabel);
            this.panel1.Controls.Add(this.bottomPanel);
            this.panel1.Name = "panel1";
            // 
            // bottomPanel
            // 
            resources.ApplyResources(this.panel1, "bottomPanel");
            this.bottomPanel.Controls.Add(this.continueButton);
            this.bottomPanel.Name = "bottomPanel";
            this.bottomPanel.Dock = DockStyle.Bottom;
            this.bottomPanel.Height = 28;
            this.bottomPanel.FlowDirection = FlowDirection.RightToLeft;
            // 
            // errorMessage
            // 
            resources.ApplyResources(this.discordLabel, "discordLabel");
            this.discordLabel.Name = "discordLabel";
            this.discordLabel.Dock = DockStyle.Top;
            this.discordLabel.Padding = Padding.Empty;
            this.discordLabel.Margin = Padding.Empty;
            // 
            // selectPathLabel
            // 
            resources.ApplyResources(this.greetingsLabel, "greetingsLabel");
            this.greetingsLabel.Name = "greetingsLabel";
            this.greetingsLabel.Dock = DockStyle.Top;
            this.greetingsLabel.Padding = Padding.Empty;
            this.greetingsLabel.Margin = Padding.Empty;
            // 
            // continueButton
            // 
            resources.ApplyResources(this.continueButton, "continueButton");
            this.continueButton.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (15)))), ((int) (((byte) (15)))), ((int) (((byte) (15)))));
            this.continueButton.ForeColor = System.Drawing.SystemColors.Control;
            this.continueButton.Name = "continueButton";
            this.continueButton.UseVisualStyleBackColor = false;
            this.continueButton.Click += new System.EventHandler(this.continueButton_Click);
            // 
            // DofusPathForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.Controls.Add(this.splitContainer1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Shown += OnFirstTimeInfoFormShown;
            this.Name = "DofusPathForm";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.bottomPanel.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize) (this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize) (this.logoPictureBox)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.bottomPanel.ResumeLayout(false);
            this.bottomPanel.PerformLayout();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Button continueButton;
        private System.Windows.Forms.Label greetingsLabel;

        private System.Windows.Forms.PictureBox logoPictureBox;

        private System.Windows.Forms.Label discordLabel;

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.FlowLayoutPanel bottomPanel;
        private System.Windows.Forms.LinkLabel usageInstructionsLinkLabel;
        private System.Windows.Forms.LinkLabel discordLinkLabel;

        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Timer enableContinueTimer;
        private System.Windows.Forms.Label label1;

        private System.Windows.Forms.SplitContainer splitContainer1;

        #endregion
    }
}