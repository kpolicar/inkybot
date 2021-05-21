using System.Drawing;
using System.Timers;
using System.Windows.Forms;
using Inkybot.Domain;

namespace Inkybot
{
    partial class MainForm
    {
        System.ComponentModel.ComponentResourceManager resources;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
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
            this.resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.components = new System.ComponentModel.Container();
            this.dofusClientPanel = new System.Windows.Forms.Panel();
            this.toastPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.toastIconPictureBox = new System.Windows.Forms.PictureBox();
            this.toastIconPictureBox = new System.Windows.Forms.PictureBox();
            this.toastLabel = new System.Windows.Forms.Label();
            this.toastPanelCloseButton = new System.Windows.Forms.Button();
            this.shutdownToastPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.shutdownToastIconPictureBox = new System.Windows.Forms.PictureBox();
            this.shutdownToastIconPictureBox = new System.Windows.Forms.PictureBox();
            this.shutdownToastLabel = new System.Windows.Forms.Label();
            this.shutdownToastValueLabel = new System.Windows.Forms.Label();
            this.shutdownToastPanelCloseButton = new System.Windows.Forms.Button();
            this.sidebarPanel = new System.Windows.Forms.Panel();
            this.buttonsPanel = new System.Windows.Forms.Panel();
            this.primaryButtonsPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.toggleMageButton = new System.Windows.Forms.Button();
            this.setupButton = new System.Windows.Forms.Button();
            this.statisticsButton = new System.Windows.Forms.Button();
            this.exoAttemptsLabel = new System.Windows.Forms.Label();
            this.exoAttemptsValueLabel = new System.Windows.Forms.Label();
            this.kamasSpentLabel = new System.Windows.Forms.Label();
            this.kamasSpentValueLabel = new System.Windows.Forms.Label();
            this.secondaryButtonsPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.helpButton = new System.Windows.Forms.Button();
            this.hallOfFameButton = new System.Windows.Forms.Button();
            this.configButton = new System.Windows.Forms.Button();
            this.debugScreenshotButton = new System.Windows.Forms.Button();
            this.debugButton = new System.Windows.Forms.Button();
            this.mageInfoPanel = new System.Windows.Forms.Panel();
            this.sinkValueLabel = new System.Windows.Forms.Label();
            this.sinkLabel = new System.Windows.Forms.Label();
            this.userInfoPanel = new System.Windows.Forms.Panel();
            this.subscribedInfoLabel = new System.Windows.Forms.Label();
            this.subscribePlanUpgradeLinkLabel = new System.Windows.Forms.LinkLabel();
            this.usernameLabel = new System.Windows.Forms.Label();
            this.loggedInAsLabel = new System.Windows.Forms.Label();
            this.mousePositionLabel = new System.Windows.Forms.Label();
            this.subscriptionCheckTimer = new System.Windows.Forms.Timer(this.components);
            this.autoShutdownTimer = new System.Windows.Forms.Timer(this.components);
            this.shutdownToastPanel.SuspendLayout();
            this.toastPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize) (this.toastIconPictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize) (this.shutdownToastIconPictureBox)).BeginInit();
            this.sidebarPanel.SuspendLayout();
            this.buttonsPanel.SuspendLayout();
            this.primaryButtonsPanel.SuspendLayout();
            this.secondaryButtonsPanel.SuspendLayout();
            this.mageInfoPanel.SuspendLayout();
            this.userInfoPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // dofusClientPanel
            // 
            resources.ApplyResources(this.dofusClientPanel, "dofusClientPanel");
            this.dofusClientPanel.Name = "dofusClientPanel";
            // 
            // toastPanel
            // 
            resources.ApplyResources(this.toastPanel, "toastPanel");
            this.toastPanel.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (15)))), ((int) (((byte) (15)))), ((int) (((byte) (15)))));
            this.toastPanel.Controls.Add(this.toastIconPictureBox);
            this.toastPanel.Controls.Add(this.toastLabel);
            this.toastPanel.Controls.Add(this.toastPanelCloseButton);
            this.toastPanel.Name = "toastPanel";
            // 
            // toastIconPictureBox
            // 
            resources.ApplyResources(this.toastIconPictureBox, "toastIconPictureBox");
            this.toastIconPictureBox.Name = "toastIconPictureBox";
            this.toastIconPictureBox.TabStop = false;
            // 
            // toastLabel
            // 
            resources.ApplyResources(this.toastLabel, "toastLabel");
            this.toastLabel.ForeColor = System.Drawing.SystemColors.Control;
            this.toastLabel.Name = "toastLabel";
            // 
            // toastPanelCloseButton
            // 
            resources.ApplyResources(this.toastPanelCloseButton, "toastPanelCloseButton");
            this.toastPanelCloseButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.toastPanelCloseButton.Name = "toastPanelCloseButton";
            this.toastPanelCloseButton.UseVisualStyleBackColor = true;
            this.toastPanelCloseButton.Click += new System.EventHandler(this.toastPanelCloseButton_Click);
            // 
            // shutdownToastPanel
            // 
            resources.ApplyResources(this.shutdownToastPanel, "shutdownToastPanel");
            this.shutdownToastPanel.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (15)))), ((int) (((byte) (15)))), ((int) (((byte) (15)))));
            this.shutdownToastPanel.Controls.Add(this.shutdownToastIconPictureBox);
            this.shutdownToastPanel.Controls.Add(this.shutdownToastLabel);
            this.shutdownToastPanel.Controls.Add(this.shutdownToastValueLabel);
            this.shutdownToastPanel.Controls.Add(this.shutdownToastPanelCloseButton);
            this.shutdownToastPanel.Name = "shutdownToastPanel";
            this.shutdownToastPanel.Visible = false;
            this.shutdownToastPanel.Location = 
                new Point(ClientSize.Width / 2 - shutdownToastPanel.Size.Width / 2, 
                    ClientSize.Height / 2 - shutdownToastPanel.Size.Height);
            // 
            // shutdownToastIconPictureBox
            // 
            resources.ApplyResources(this.shutdownToastIconPictureBox, "shutdownToastIconPictureBox");
            this.shutdownToastIconPictureBox.Name = "shutdownToastIconPictureBox";
            this.shutdownToastIconPictureBox.TabStop = false;
            // 
            // shutdownToastLabel
            // 
            resources.ApplyResources(this.shutdownToastLabel, "shutdownToastLabel");
            this.shutdownToastLabel.ForeColor = System.Drawing.SystemColors.Control;
            this.shutdownToastLabel.Name = "shutdownToastLabel";
            // 
            // shutdownToastValueLabel
            // 
            resources.ApplyResources(this.shutdownToastValueLabel, "shutdownToastValueLabel");
            this.shutdownToastValueLabel.ForeColor = System.Drawing.SystemColors.Control;
            this.shutdownToastValueLabel.Name = "shutdownToastValueLabel";
            // 
            // shutdownToastPanelCloseButton
            // 
            resources.ApplyResources(this.shutdownToastPanelCloseButton, "shutdownToastPanelCloseButton");
            this.shutdownToastPanelCloseButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.shutdownToastPanelCloseButton.Name = "shutdownToastPanelCloseButton";
            this.shutdownToastPanelCloseButton.UseVisualStyleBackColor = true;
            this.shutdownToastPanelCloseButton.Click += new System.EventHandler(this.shutdownToastPanelCloseButton_Click);
            // 
            // sidebarPanel
            // 
            resources.ApplyResources(this.sidebarPanel, "sidebarPanel");
            this.sidebarPanel.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (15)))), ((int) (((byte) (15)))), ((int) (((byte) (15)))));
            this.sidebarPanel.Controls.Add(this.buttonsPanel);
            this.sidebarPanel.Controls.Add(this.mageInfoPanel);
            this.sidebarPanel.Controls.Add(this.userInfoPanel);
            this.sidebarPanel.Controls.Add(this.mousePositionLabel);
            this.sidebarPanel.Name = "sidebarPanel";
            // 
            // buttonsPanel
            // 
            resources.ApplyResources(this.buttonsPanel, "buttonsPanel");
            this.buttonsPanel.BackColor = System.Drawing.Color.Transparent;
            this.buttonsPanel.Controls.Add(this.primaryButtonsPanel);
            this.buttonsPanel.Controls.Add(this.secondaryButtonsPanel);
            this.buttonsPanel.Name = "buttonsPanel";
            // 
            // primaryButtonsPanel
            // 
            resources.ApplyResources(this.primaryButtonsPanel, "primaryButtonsPanel");
            this.primaryButtonsPanel.Controls.Add(this.toggleMageButton);
            this.primaryButtonsPanel.Controls.Add(this.setupButton);
            this.primaryButtonsPanel.Controls.Add(this.statisticsButton);
            this.primaryButtonsPanel.Controls.Add(this.exoAttemptsLabel);
            this.primaryButtonsPanel.Controls.Add(this.exoAttemptsValueLabel);
            this.primaryButtonsPanel.Controls.Add(this.kamasSpentLabel);
            this.primaryButtonsPanel.Controls.Add(this.kamasSpentValueLabel);
            this.primaryButtonsPanel.Name = "primaryButtonsPanel";
            // 
            // toggleMageButton
            // 
            resources.ApplyResources(this.toggleMageButton, "toggleMageButton");
            this.toggleMageButton.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (30)))), ((int) (((byte) (30)))), ((int) (((byte) (30)))));
            this.toggleMageButton.FlatAppearance.BorderSize = 0;
            this.toggleMageButton.ForeColor = System.Drawing.SystemColors.Control;
            this.toggleMageButton.Name = "toggleMageButton";
            this.toggleMageButton.UseVisualStyleBackColor = false;
            this.toggleMageButton.Click += new System.EventHandler(this.toggleMageButton_Click);
            // 
            // setupButton
            // 
            resources.ApplyResources(this.setupButton, "setupButton");
            this.setupButton.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (30)))), ((int) (((byte) (30)))), ((int) (((byte) (30)))));
            this.setupButton.FlatAppearance.BorderSize = 0;
            this.setupButton.ForeColor = System.Drawing.SystemColors.Control;
            this.setupButton.Name = "setupButton";
            this.setupButton.UseVisualStyleBackColor = false;
            this.setupButton.Click += new System.EventHandler(this.statsButton_Click);
            // 
            // statisticsButton
            // 
            resources.ApplyResources(this.statisticsButton, "statisticsButton");
            this.statisticsButton.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (30)))), ((int) (((byte) (30)))), ((int) (((byte) (30)))));
            this.statisticsButton.FlatAppearance.BorderSize = 0;
            this.statisticsButton.ForeColor = System.Drawing.SystemColors.Control;
            this.statisticsButton.Name = "statisticsButton";
            this.statisticsButton.UseVisualStyleBackColor = false;
            this.statisticsButton.Click += new System.EventHandler(this.statisticsButton_Click);
            // 
            // kamasSpentLabel
            // 
            resources.ApplyResources(this.kamasSpentLabel, "kamasSpentLabel");
            this.kamasSpentLabel.ForeColor = System.Drawing.SystemColors.Control;
            this.kamasSpentLabel.Name = "kamasSpentLabel";
            // 
            // kamasSpentValueLabel
            // 
            resources.ApplyResources(this.kamasSpentValueLabel, "kamasSpentValueLabel");
            this.kamasSpentValueLabel.ForeColor = System.Drawing.SystemColors.Control;
            this.kamasSpentValueLabel.Name = "kamasSpentValueLabel";
            // 
            // exoAttemptsLabel
            // 
            resources.ApplyResources(this.exoAttemptsLabel, "exoAttemptsLabel");
            this.exoAttemptsLabel.ForeColor = System.Drawing.SystemColors.Control;
            this.exoAttemptsLabel.Name = "exoAttemptsLabel";
            // 
            // exoAttemptsValueLabel
            // 
            resources.ApplyResources(this.exoAttemptsValueLabel, "exoAttemptsValueLabel");
            this.exoAttemptsValueLabel.ForeColor = System.Drawing.SystemColors.Control;
            this.exoAttemptsValueLabel.Name = "exoAttemptsValueLabel";
            // 
            // secondaryButtonsPanel
            // 
            resources.ApplyResources(this.secondaryButtonsPanel, "secondaryButtonsPanel");
            this.secondaryButtonsPanel.Controls.Add(this.hallOfFameButton);
            this.secondaryButtonsPanel.Controls.Add(this.helpButton);
            this.secondaryButtonsPanel.Controls.Add(this.configButton);
            this.secondaryButtonsPanel.Controls.Add(this.debugScreenshotButton);
            this.secondaryButtonsPanel.Controls.Add(this.debugButton);
            this.secondaryButtonsPanel.Name = "secondaryButtonsPanel";
            // 
            // helpButton
            // 
            resources.ApplyResources(this.helpButton, "helpButton");
            this.helpButton.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (30)))), ((int) (((byte) (30)))), ((int) (((byte) (30)))));
            this.helpButton.FlatAppearance.BorderSize = 0;
            this.helpButton.ForeColor = System.Drawing.SystemColors.Control;
            this.helpButton.Name = "helpButton";
            this.helpButton.UseVisualStyleBackColor = false;
            this.helpButton.Click += new System.EventHandler(this.helpButton_Click);
            System.Windows.Forms.ToolTip ToolTip1 = new System.Windows.Forms.ToolTip();
            ToolTip1.SetToolTip(this.helpButton, $"{Inkybot.Domain.Server.BaseUrl}/release/{Program.VersionEndpoint}#usage");
            // 
            // hallOfFameButton
            // 
            resources.ApplyResources(this.hallOfFameButton, "hallOfFameButton");
            this.hallOfFameButton.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (30)))), ((int) (((byte) (30)))), ((int) (((byte) (30)))));
            this.hallOfFameButton.FlatAppearance.BorderSize = 0;
            this.hallOfFameButton.ForeColor = System.Drawing.SystemColors.Control;
            this.hallOfFameButton.Name = "hallOfFameButton";
            this.hallOfFameButton.UseVisualStyleBackColor = false;
            this.hallOfFameButton.Click += new System.EventHandler(this.hallOfFameButton_Click);
            System.Windows.Forms.ToolTip ToolTip2 = new System.Windows.Forms.ToolTip();
            ToolTip2.SetToolTip(this.hallOfFameButton, Server.HallOfFameUrl);
            // 
            // configButton
            // 
            resources.ApplyResources(this.configButton, "configButton");
            this.configButton.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (30)))), ((int) (((byte) (30)))), ((int) (((byte) (30)))));
            this.configButton.FlatAppearance.BorderSize = 0;
            this.configButton.ForeColor = System.Drawing.SystemColors.Control;
            this.configButton.Name = "configButton";
            this.configButton.UseVisualStyleBackColor = false;
            this.configButton.Click += new System.EventHandler(this.configButton_Click);
            // 
            // debugScreenshotButton
            // 
            resources.ApplyResources(this.debugScreenshotButton, "debugScreenshotButton");
            this.debugScreenshotButton.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (30)))), ((int) (((byte) (30)))), ((int) (((byte) (30)))));
            this.debugScreenshotButton.FlatAppearance.BorderSize = 0;
            this.debugScreenshotButton.ForeColor = System.Drawing.SystemColors.Control;
            this.debugScreenshotButton.Name = "debugScreenshotButton";
            this.debugScreenshotButton.UseVisualStyleBackColor = false;
            this.debugScreenshotButton.Click += new System.EventHandler(this.debugScreenshotButton_Click);
            // 
            // debugButton
            // 
            resources.ApplyResources(this.debugButton, "debugButton");
            this.debugButton.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (30)))), ((int) (((byte) (30)))), ((int) (((byte) (30)))));
            this.debugButton.FlatAppearance.BorderSize = 0;
            this.debugButton.ForeColor = System.Drawing.SystemColors.Control;
            this.debugButton.Name = "debugButton";
            this.debugButton.UseVisualStyleBackColor = false;
            this.debugButton.Click += new System.EventHandler(this.debugButton_Click);
            // 
            // mageInfoPanel
            // 
            resources.ApplyResources(this.mageInfoPanel, "mageInfoPanel");
            this.mageInfoPanel.BackColor = System.Drawing.Color.Transparent;
            this.mageInfoPanel.Controls.Add(this.sinkValueLabel);
            this.mageInfoPanel.Controls.Add(this.sinkLabel);
            this.mageInfoPanel.Name = "mageInfoPanel";
            // 
            // sinkValueLabel
            // 
            resources.ApplyResources(this.sinkValueLabel, "sinkValueLabel");
            this.sinkValueLabel.ForeColor = System.Drawing.SystemColors.Control;
            this.sinkValueLabel.Name = "sinkValueLabel";
            // 
            // sinkLabel
            // 
            resources.ApplyResources(this.sinkLabel, "sinkLabel");
            this.sinkLabel.ForeColor = System.Drawing.SystemColors.Control;
            this.sinkLabel.Name = "sinkLabel";
            // 
            // userInfoPanel
            // 
            resources.ApplyResources(this.userInfoPanel, "userInfoPanel");
            this.userInfoPanel.BackColor = System.Drawing.Color.Transparent;
            this.userInfoPanel.Controls.Add(this.subscribePlanUpgradeLinkLabel);
            this.userInfoPanel.Controls.Add(this.subscribedInfoLabel);
            this.userInfoPanel.Controls.Add(this.usernameLabel);
            this.userInfoPanel.Controls.Add(this.loggedInAsLabel);
            this.userInfoPanel.Name = "userInfoPanel";
            // 
            // subscribedInfoLabel
            // 
            resources.ApplyResources(this.subscribedInfoLabel, "subscribedInfoLabel");
            this.subscribedInfoLabel.ForeColor = System.Drawing.SystemColors.Control;
            this.subscribedInfoLabel.Name = "subscribedInfoLabel";
            // 
            // subscribePlanUpgradeLinkLabel
            // 
            resources.ApplyResources(this.subscribePlanUpgradeLinkLabel, "subscribePlanUpgradeLinkLabel");
            this.subscribePlanUpgradeLinkLabel.Name = "subscribePlanUpgradeLinkLabel";
            this.subscribePlanUpgradeLinkLabel.ActiveLinkColor = System.Drawing.SystemColors.Control;
            this.subscribePlanUpgradeLinkLabel.LinkColor = System.Drawing.SystemColors.ControlLight;
            this.subscribePlanUpgradeLinkLabel.Click += new System.EventHandler(this.subscribePlanUpgradeLinkLabel_OnClick);
            // 
            // usernameLabel
            // 
            resources.ApplyResources(this.usernameLabel, "usernameLabel");
            this.usernameLabel.ForeColor = System.Drawing.SystemColors.Control;
            this.usernameLabel.Name = "usernameLabel";
            // 
            // loggedInAsLabel
            // 
            resources.ApplyResources(this.loggedInAsLabel, "loggedInAsLabel");
            this.loggedInAsLabel.ForeColor = System.Drawing.SystemColors.Control;
            this.loggedInAsLabel.Name = "loggedInAsLabel";
            this.loggedInAsLabel.MaximumSize = new Size(113, 0);
            // 
            // mousePositionLabel
            // 
            resources.ApplyResources(this.mousePositionLabel, "mousePositionLabel");
            this.mousePositionLabel.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (60)))), ((int) (((byte) (60)))), ((int) (((byte) (60)))));
            this.mousePositionLabel.ForeColor = System.Drawing.SystemColors.Control;
            this.mousePositionLabel.Name = "mousePositionLabel";
            // 
            // subscriptionCheckTimer
            // 
            this.subscriptionCheckTimer.Interval = 25000;
            this.subscriptionCheckTimer.Tick += new System.EventHandler(this.OnSubscriptionCheckTimer);
            // 
            // autoShutdownTimer
            // 
            this.autoShutdownTimer.Interval = 1000;
            this.autoShutdownTimer.Tick += new System.EventHandler(this.OnAutoShutdownTimer);
            // 
            // MainForm
            // 
            this.Resize += OnResize;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.toastPanel);
            this.Controls.Add(this.shutdownToastPanel);
            this.Controls.Add(this.sidebarPanel);
            this.Controls.Add(this.dofusClientPanel);
            this.HelpButton = true;
            this.MinimumSize = new System.Drawing.Size(720, 480);
            this.Name = "MainForm";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.VisibleChanged += new System.EventHandler(this.MainForm_VisibleChanged);
            this.toastPanel.ResumeLayout(false);
            this.toastPanel.PerformLayout();
            this.shutdownToastPanel.ResumeLayout(false);
            this.shutdownToastPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize) (this.toastIconPictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize) (this.shutdownToastIconPictureBox)).EndInit();
            this.sidebarPanel.ResumeLayout(false);
            this.sidebarPanel.PerformLayout();
            this.buttonsPanel.ResumeLayout(false);
            this.primaryButtonsPanel.ResumeLayout(false);
            this.primaryButtonsPanel.PerformLayout();
            this.secondaryButtonsPanel.ResumeLayout(false);
            this.secondaryButtonsPanel.PerformLayout();
            this.mageInfoPanel.ResumeLayout(false);
            this.userInfoPanel.ResumeLayout(false);
            this.userInfoPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Button configButton;

        private System.Windows.Forms.Label exoAttemptsLabel;
        private System.Windows.Forms.Label exoAttemptsValueLabel;
        private System.Windows.Forms.Label kamasSpentLabel;
        private System.Windows.Forms.Label kamasSpentValueLabel;

        private System.Windows.Forms.Button debugScreenshotButton;

        private System.Windows.Forms.PictureBox toastIconPictureBox;
        private System.Windows.Forms.Label toastLabel;
        private System.Windows.Forms.Button toastPanelCloseButton;
        
        private System.Windows.Forms.PictureBox shutdownToastIconPictureBox;
        private System.Windows.Forms.Label shutdownToastLabel;
        private System.Windows.Forms.Label shutdownToastValueLabel;
        private System.Windows.Forms.Button shutdownToastPanelCloseButton;

        private System.Windows.Forms.Button setupButton;
        private System.Windows.Forms.Button statisticsButton;

        private System.Windows.Forms.Label loggedInAsLabel;

        private System.Windows.Forms.Label mousePositionLabel;

        private System.Windows.Forms.FlowLayoutPanel primaryButtonsPanel;

        private System.Windows.Forms.FlowLayoutPanel secondaryButtonsPanel;

        private System.Windows.Forms.Button debugButton;
        private System.Windows.Forms.Button hallOfFameButton;
        private System.Windows.Forms.Button helpButton;
        private System.Windows.Forms.Label subscribedInfoLabel;
        private System.Windows.Forms.LinkLabel subscribePlanUpgradeLinkLabel;
        private System.Windows.Forms.Button toggleMageButton;

        private System.Windows.Forms.Panel buttonsPanel;
        private System.Windows.Forms.Panel mageInfoPanel;
        private System.Windows.Forms.Panel userInfoPanel;

        private System.Windows.Forms.Label sinkLabel;
        private System.Windows.Forms.Label sinkValueLabel;

        private System.Windows.Forms.Label usernameLabel;

        #endregion

        private System.Windows.Forms.Panel dofusClientPanel;
        private System.Windows.Forms.FlowLayoutPanel toastPanel;
        private System.Windows.Forms.FlowLayoutPanel shutdownToastPanel;
        private System.Windows.Forms.Panel sidebarPanel;
        private System.Windows.Forms.Timer subscriptionCheckTimer;
        private System.Windows.Forms.Timer autoShutdownTimer;
    }
}