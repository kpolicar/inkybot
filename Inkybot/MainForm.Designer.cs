using System.Drawing;
using System.Timers;

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
            this.toastLabel = new System.Windows.Forms.Label();
            this.toastPanelCloseButton = new System.Windows.Forms.Button();
            this.sidebarPanel = new System.Windows.Forms.Panel();
            this.buttonsPanel = new System.Windows.Forms.Panel();
            this.primaryButtonsPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.toggleMageButton = new System.Windows.Forms.Button();
            this.statsButton = new System.Windows.Forms.Button();
            this.exoAttemptsLabel = new System.Windows.Forms.Label();
            this.exoAttemptsValueLabel = new System.Windows.Forms.Label();
            this.secondaryButtonsPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.helpButton = new System.Windows.Forms.Button();
            this.configButton = new System.Windows.Forms.Button();
            this.debugScreenshotButton = new System.Windows.Forms.Button();
            this.debugButton = new System.Windows.Forms.Button();
            this.mageInfoPanel = new System.Windows.Forms.Panel();
            this.sinkValueLabel = new System.Windows.Forms.Label();
            this.sinkLabel = new System.Windows.Forms.Label();
            this.userInfoPanel = new System.Windows.Forms.Panel();
            this.subscribedInfoLabel = new System.Windows.Forms.Label();
            this.usernameLabel = new System.Windows.Forms.Label();
            this.loggedInAsLabel = new System.Windows.Forms.Label();
            this.mousePositionLabel = new System.Windows.Forms.Label();
            this.subscriptionCheckTimer = new System.Windows.Forms.Timer(this.components);
            this.toastPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize) (this.toastIconPictureBox)).BeginInit();
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
            this.primaryButtonsPanel.Controls.Add(this.statsButton);
            this.primaryButtonsPanel.Controls.Add(this.exoAttemptsLabel);
            this.primaryButtonsPanel.Controls.Add(this.exoAttemptsValueLabel);
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
            // statsButton
            // 
            resources.ApplyResources(this.statsButton, "statsButton");
            this.statsButton.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (30)))), ((int) (((byte) (30)))), ((int) (((byte) (30)))));
            this.statsButton.FlatAppearance.BorderSize = 0;
            this.statsButton.ForeColor = System.Drawing.SystemColors.Control;
            this.statsButton.Name = "statsButton";
            this.statsButton.UseVisualStyleBackColor = false;
            this.statsButton.Click += new System.EventHandler(this.statsButton_Click);
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
            // MainForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.toastPanel);
            this.Controls.Add(this.sidebarPanel);
            this.Controls.Add(this.dofusClientPanel);
            this.HelpButton = true;
            this.Name = "MainForm";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.VisibleChanged += new System.EventHandler(this.MainForm_VisibleChanged);
            this.toastPanel.ResumeLayout(false);
            this.toastPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize) (this.toastIconPictureBox)).EndInit();
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

        private System.Windows.Forms.Button debugScreenshotButton;

        private System.Windows.Forms.PictureBox toastIconPictureBox;
        private System.Windows.Forms.Label toastLabel;
        private System.Windows.Forms.Button toastPanelCloseButton;

        private System.Windows.Forms.Button statsButton;

        private System.Windows.Forms.Label loggedInAsLabel;

        private System.Windows.Forms.Label mousePositionLabel;

        private System.Windows.Forms.FlowLayoutPanel primaryButtonsPanel;

        private System.Windows.Forms.FlowLayoutPanel secondaryButtonsPanel;

        private System.Windows.Forms.Button debugButton;
        private System.Windows.Forms.Button helpButton;
        private System.Windows.Forms.Label subscribedInfoLabel;
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
        private System.Windows.Forms.Panel sidebarPanel;
        private System.Windows.Forms.Timer subscriptionCheckTimer;
    }
}