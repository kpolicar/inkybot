using System.Drawing;
using System.Timers;
using Inkybot.Controls;

namespace Inkybot
{
    partial class MainForm
    {
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.dofusClientPanel = new System.Windows.Forms.Panel();
            this.ocrIndicatorPanelRect1 = new System.Windows.Forms.Panel();
            this.toastPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.toastIconPictureBox = new System.Windows.Forms.PictureBox();
            this.toastLabel = new System.Windows.Forms.Label();
            this.toastPanelCloseButton = new System.Windows.Forms.Button();
            this.sidebarPanel = new System.Windows.Forms.Panel();
            this.buttonsPanel = new System.Windows.Forms.Panel();
            this.primaryButtonsPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.toggleMageButton = new System.Windows.Forms.Button();
            this.statsButton = new System.Windows.Forms.Button();
            this.secondaryButtonsPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.helpButton = new System.Windows.Forms.Button();
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
            this.ocrIndicatorPanel = new Inkybot.Controls.TransparentPanel();
            this.paintTimer = new System.Windows.Forms.Timer(this.components);
            this.subscriptionCheckTimer = new System.Windows.Forms.Timer(this.components);
            this.dofusClientPanel.SuspendLayout();
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
            this.dofusClientPanel.AutoSize = true;
            this.dofusClientPanel.Controls.Add(this.ocrIndicatorPanelRect1);
            this.dofusClientPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dofusClientPanel.Location = new System.Drawing.Point(0, 0);
            this.dofusClientPanel.Margin = new System.Windows.Forms.Padding(0);
            this.dofusClientPanel.Name = "dofusClientPanel";
            this.dofusClientPanel.Size = new System.Drawing.Size(1083, 590);
            this.dofusClientPanel.TabIndex = 0;
            // 
            // ocrIndicatorPanelRect1
            // 
            this.ocrIndicatorPanelRect1.BackColor = System.Drawing.Color.Black;
            this.ocrIndicatorPanelRect1.Location = new System.Drawing.Point(186, 95);
            this.ocrIndicatorPanelRect1.Name = "ocrIndicatorPanelRect1";
            this.ocrIndicatorPanelRect1.Size = new System.Drawing.Size(2, 446);
            this.ocrIndicatorPanelRect1.TabIndex = 0;
            // 
            // toastPanel
            // 
            this.toastPanel.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.toastPanel.AutoSize = true;
            this.toastPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.toastPanel.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (15)))), ((int) (((byte) (15)))), ((int) (((byte) (15)))));
            this.toastPanel.Controls.Add(this.toastIconPictureBox);
            this.toastPanel.Controls.Add(this.toastLabel);
            this.toastPanel.Controls.Add(this.toastPanelCloseButton);
            this.toastPanel.Location = new System.Drawing.Point(697, 6);
            this.toastPanel.Name = "toastPanel";
            this.toastPanel.Padding = new System.Windows.Forms.Padding(8);
            this.toastPanel.Size = new System.Drawing.Size(374, 54);
            this.toastPanel.TabIndex = 0;
            // 
            // toastIconPictureBox
            // 
            this.toastIconPictureBox.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.toastIconPictureBox.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.toastIconPictureBox.Image = ((System.Drawing.Image) (resources.GetObject("toastIconPictureBox.Image")));
            this.toastIconPictureBox.Location = new System.Drawing.Point(11, 11);
            this.toastIconPictureBox.Name = "toastIconPictureBox";
            this.toastIconPictureBox.Size = new System.Drawing.Size(33, 31);
            this.toastIconPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.toastIconPictureBox.TabIndex = 7;
            this.toastIconPictureBox.TabStop = false;
            // 
            // toastLabel
            // 
            this.toastLabel.AutoSize = true;
            this.toastLabel.Font = new System.Drawing.Font("Calibri", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            this.toastLabel.ForeColor = System.Drawing.SystemColors.Control;
            this.toastLabel.Location = new System.Drawing.Point(50, 8);
            this.toastLabel.Name = "toastLabel";
            this.toastLabel.Padding = new System.Windows.Forms.Padding(3, 12, 30, 12);
            this.toastLabel.Size = new System.Drawing.Size(291, 38);
            this.toastLabel.TabIndex = 5;
            this.toastLabel.Text = "There has been an error processing the image";
            // 
            // toastPanelCloseButton
            // 
            this.toastPanelCloseButton.BackgroundImage = ((System.Drawing.Image) (resources.GetObject("toastPanelCloseButton.BackgroundImage")));
            this.toastPanelCloseButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.toastPanelCloseButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.toastPanelCloseButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.toastPanelCloseButton.Location = new System.Drawing.Point(347, 11);
            this.toastPanelCloseButton.Name = "toastPanelCloseButton";
            this.toastPanelCloseButton.Size = new System.Drawing.Size(16, 16);
            this.toastPanelCloseButton.TabIndex = 8;
            this.toastPanelCloseButton.UseVisualStyleBackColor = true;
            this.toastPanelCloseButton.Click += new System.EventHandler(this.toastPanelCloseButton_Click);
            // 
            // sidebarPanel
            // 
            this.sidebarPanel.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left)));
            this.sidebarPanel.AutoSize = true;
            this.sidebarPanel.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (15)))), ((int) (((byte) (15)))), ((int) (((byte) (15)))));
            this.sidebarPanel.Controls.Add(this.buttonsPanel);
            this.sidebarPanel.Controls.Add(this.mageInfoPanel);
            this.sidebarPanel.Controls.Add(this.userInfoPanel);
            this.sidebarPanel.Controls.Add(this.mousePositionLabel);
            this.sidebarPanel.Location = new System.Drawing.Point(0, 0);
            this.sidebarPanel.Name = "sidebarPanel";
            this.sidebarPanel.Size = new System.Drawing.Size(113, 590);
            this.sidebarPanel.TabIndex = 0;
            // 
            // buttonsPanel
            // 
            this.buttonsPanel.AutoSize = true;
            this.buttonsPanel.BackColor = System.Drawing.Color.Transparent;
            this.buttonsPanel.Controls.Add(this.primaryButtonsPanel);
            this.buttonsPanel.Controls.Add(this.secondaryButtonsPanel);
            this.buttonsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonsPanel.Location = new System.Drawing.Point(0, 96);
            this.buttonsPanel.Name = "buttonsPanel";
            this.buttonsPanel.Size = new System.Drawing.Size(113, 393);
            this.buttonsPanel.TabIndex = 1;
            // 
            // primaryButtonsPanel
            // 
            this.primaryButtonsPanel.Controls.Add(this.toggleMageButton);
            this.primaryButtonsPanel.Controls.Add(this.statsButton);
            this.primaryButtonsPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.primaryButtonsPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.primaryButtonsPanel.Location = new System.Drawing.Point(0, 0);
            this.primaryButtonsPanel.Name = "primaryButtonsPanel";
            this.primaryButtonsPanel.Size = new System.Drawing.Size(113, 128);
            this.primaryButtonsPanel.TabIndex = 1;
            // 
            // toggleMageButton
            // 
            this.toggleMageButton.AutoSize = true;
            this.toggleMageButton.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (30)))), ((int) (((byte) (30)))), ((int) (((byte) (30)))));
            this.toggleMageButton.FlatAppearance.BorderSize = 0;
            this.toggleMageButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.toggleMageButton.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            this.toggleMageButton.ForeColor = System.Drawing.SystemColors.Control;
            this.toggleMageButton.Location = new System.Drawing.Point(0, 0);
            this.toggleMageButton.Margin = new System.Windows.Forms.Padding(0, 0, 0, 3);
            this.toggleMageButton.Name = "toggleMageButton";
            this.toggleMageButton.Size = new System.Drawing.Size(113, 61);
            this.toggleMageButton.TabIndex = 1;
            this.toggleMageButton.Text = "START";
            this.toggleMageButton.UseVisualStyleBackColor = false;
            this.toggleMageButton.Click += new System.EventHandler(this.toggleMageButton_Click);
            // 
            // statsButton
            // 
            this.statsButton.AutoSize = true;
            this.statsButton.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (30)))), ((int) (((byte) (30)))), ((int) (((byte) (30)))));
            this.statsButton.FlatAppearance.BorderSize = 0;
            this.statsButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.statsButton.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            this.statsButton.ForeColor = System.Drawing.SystemColors.Control;
            this.statsButton.Location = new System.Drawing.Point(0, 64);
            this.statsButton.Margin = new System.Windows.Forms.Padding(0, 0, 0, 3);
            this.statsButton.Name = "statsButton";
            this.statsButton.Size = new System.Drawing.Size(113, 61);
            this.statsButton.TabIndex = 2;
            this.statsButton.Text = "STATS";
            this.statsButton.UseVisualStyleBackColor = false;
            this.statsButton.Click += new System.EventHandler(this.statsButton_Click);
            // 
            // secondaryButtonsPanel
            // 
            this.secondaryButtonsPanel.Controls.Add(this.helpButton);
            this.secondaryButtonsPanel.Controls.Add(this.debugScreenshotButton);
            this.secondaryButtonsPanel.Controls.Add(this.debugButton);
            this.secondaryButtonsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.secondaryButtonsPanel.FlowDirection = System.Windows.Forms.FlowDirection.BottomUp;
            this.secondaryButtonsPanel.Location = new System.Drawing.Point(0, 235);
            this.secondaryButtonsPanel.Name = "secondaryButtonsPanel";
            this.secondaryButtonsPanel.Size = new System.Drawing.Size(113, 158);
            this.secondaryButtonsPanel.TabIndex = 2;
            // 
            // helpButton
            // 
            this.helpButton.AutoSize = true;
            this.helpButton.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (30)))), ((int) (((byte) (30)))), ((int) (((byte) (30)))));
            this.helpButton.FlatAppearance.BorderSize = 0;
            this.helpButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.helpButton.Font = new System.Drawing.Font("Calibri", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            this.helpButton.ForeColor = System.Drawing.SystemColors.Control;
            this.helpButton.Location = new System.Drawing.Point(0, 113);
            this.helpButton.Margin = new System.Windows.Forms.Padding(0, 3, 0, 0);
            this.helpButton.Name = "helpButton";
            this.helpButton.Size = new System.Drawing.Size(113, 45);
            this.helpButton.TabIndex = 3;
            this.helpButton.Text = "HELP";
            this.helpButton.UseVisualStyleBackColor = false;
            this.helpButton.Click += new System.EventHandler(this.helpButton_Click);
            // 
            // debugScreenshotButton
            // 
            this.debugScreenshotButton.AutoSize = true;
            this.debugScreenshotButton.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (30)))), ((int) (((byte) (30)))), ((int) (((byte) (30)))));
            this.debugScreenshotButton.FlatAppearance.BorderSize = 0;
            this.debugScreenshotButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.debugScreenshotButton.Font = new System.Drawing.Font("Calibri", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            this.debugScreenshotButton.ForeColor = System.Drawing.SystemColors.Control;
            this.debugScreenshotButton.Location = new System.Drawing.Point(0, 65);
            this.debugScreenshotButton.Margin = new System.Windows.Forms.Padding(0, 3, 0, 0);
            this.debugScreenshotButton.Name = "debugScreenshotButton";
            this.debugScreenshotButton.Size = new System.Drawing.Size(113, 45);
            this.debugScreenshotButton.TabIndex = 5;
            this.debugScreenshotButton.Text = "SCREENSHOT";
            this.debugScreenshotButton.UseVisualStyleBackColor = false;
            this.debugScreenshotButton.Click += new System.EventHandler(this.debugScreenshotButton_Click);
            // 
            // debugButton
            // 
            this.debugButton.AutoSize = true;
            this.debugButton.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (30)))), ((int) (((byte) (30)))), ((int) (((byte) (30)))));
            this.debugButton.FlatAppearance.BorderSize = 0;
            this.debugButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.debugButton.Font = new System.Drawing.Font("Calibri", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            this.debugButton.ForeColor = System.Drawing.SystemColors.Control;
            this.debugButton.Location = new System.Drawing.Point(0, 17);
            this.debugButton.Margin = new System.Windows.Forms.Padding(0, 3, 0, 0);
            this.debugButton.Name = "debugButton";
            this.debugButton.Size = new System.Drawing.Size(113, 45);
            this.debugButton.TabIndex = 4;
            this.debugButton.Text = "DEBUG";
            this.debugButton.UseVisualStyleBackColor = false;
            this.debugButton.Click += new System.EventHandler(this.debugButton_Click);
            // 
            // mageInfoPanel
            // 
            this.mageInfoPanel.AutoSize = true;
            this.mageInfoPanel.BackColor = System.Drawing.Color.Transparent;
            this.mageInfoPanel.Controls.Add(this.sinkValueLabel);
            this.mageInfoPanel.Controls.Add(this.sinkLabel);
            this.mageInfoPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.mageInfoPanel.Location = new System.Drawing.Point(0, 489);
            this.mageInfoPanel.Name = "mageInfoPanel";
            this.mageInfoPanel.Padding = new System.Windows.Forms.Padding(0, 15, 0, 5);
            this.mageInfoPanel.Size = new System.Drawing.Size(113, 57);
            this.mageInfoPanel.TabIndex = 7;
            // 
            // sinkValueLabel
            // 
            this.sinkValueLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.sinkValueLabel.Font = new System.Drawing.Font("Trebuchet MS", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            this.sinkValueLabel.ForeColor = System.Drawing.SystemColors.Control;
            this.sinkValueLabel.Location = new System.Drawing.Point(0, 28);
            this.sinkValueLabel.Name = "sinkValueLabel";
            this.sinkValueLabel.Size = new System.Drawing.Size(113, 24);
            this.sinkValueLabel.TabIndex = 7;
            this.sinkValueLabel.Text = "0";
            this.sinkValueLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // sinkLabel
            // 
            this.sinkLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.sinkLabel.Font = new System.Drawing.Font("Calibri", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            this.sinkLabel.ForeColor = System.Drawing.SystemColors.Control;
            this.sinkLabel.Location = new System.Drawing.Point(0, 15);
            this.sinkLabel.Name = "sinkLabel";
            this.sinkLabel.Size = new System.Drawing.Size(113, 13);
            this.sinkLabel.TabIndex = 6;
            this.sinkLabel.Text = "Sink";
            this.sinkLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // userInfoPanel
            // 
            this.userInfoPanel.AutoSize = true;
            this.userInfoPanel.BackColor = System.Drawing.Color.Transparent;
            this.userInfoPanel.Controls.Add(this.subscribedInfoLabel);
            this.userInfoPanel.Controls.Add(this.usernameLabel);
            this.userInfoPanel.Controls.Add(this.loggedInAsLabel);
            this.userInfoPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.userInfoPanel.Location = new System.Drawing.Point(0, 0);
            this.userInfoPanel.Name = "userInfoPanel";
            this.userInfoPanel.Padding = new System.Windows.Forms.Padding(0, 15, 0, 15);
            this.userInfoPanel.Size = new System.Drawing.Size(113, 96);
            this.userInfoPanel.TabIndex = 4;
            // 
            // subscribedInfoLabel
            // 
            this.subscribedInfoLabel.AutoSize = true;
            this.subscribedInfoLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.subscribedInfoLabel.Font = new System.Drawing.Font("Calibri", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            this.subscribedInfoLabel.ForeColor = System.Drawing.SystemColors.Control;
            this.subscribedInfoLabel.Location = new System.Drawing.Point(0, 52);
            this.subscribedInfoLabel.Name = "subscribedInfoLabel";
            this.subscribedInfoLabel.Padding = new System.Windows.Forms.Padding(0, 3, 0, 0);
            this.subscribedInfoLabel.Size = new System.Drawing.Size(95, 29);
            this.subscribedInfoLabel.TabIndex = 3;
            this.subscribedInfoLabel.Text = "SUBSCRIBED UNTIL:\r\ndd/mm/yyyy";
            // 
            // usernameLabel
            // 
            this.usernameLabel.AutoSize = true;
            this.usernameLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.usernameLabel.Font = new System.Drawing.Font("Trebuchet MS", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            this.usernameLabel.ForeColor = System.Drawing.SystemColors.Control;
            this.usernameLabel.Location = new System.Drawing.Point(0, 28);
            this.usernameLabel.Name = "usernameLabel";
            this.usernameLabel.Size = new System.Drawing.Size(77, 24);
            this.usernameLabel.TabIndex = 2;
            this.usernameLabel.Text = "Klemen";
            // 
            // loggedInAsLabel
            // 
            this.loggedInAsLabel.AutoSize = true;
            this.loggedInAsLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.loggedInAsLabel.Font = new System.Drawing.Font("Calibri", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            this.loggedInAsLabel.ForeColor = System.Drawing.SystemColors.Control;
            this.loggedInAsLabel.Location = new System.Drawing.Point(0, 15);
            this.loggedInAsLabel.Name = "loggedInAsLabel";
            this.loggedInAsLabel.Size = new System.Drawing.Size(70, 13);
            this.loggedInAsLabel.TabIndex = 4;
            this.loggedInAsLabel.Text = "LOGGED IN AS";
            // 
            // mousePositionLabel
            // 
            this.mousePositionLabel.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (60)))), ((int) (((byte) (60)))), ((int) (((byte) (60)))));
            this.mousePositionLabel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.mousePositionLabel.Font = new System.Drawing.Font("Trebuchet MS", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            this.mousePositionLabel.ForeColor = System.Drawing.SystemColors.Control;
            this.mousePositionLabel.Location = new System.Drawing.Point(0, 546);
            this.mousePositionLabel.Name = "mousePositionLabel";
            this.mousePositionLabel.Padding = new System.Windows.Forms.Padding(0, 5, 0, 0);
            this.mousePositionLabel.Size = new System.Drawing.Size(113, 44);
            this.mousePositionLabel.TabIndex = 8;
            this.mousePositionLabel.Text = "(0,0)\r\n(0,0)";
            this.mousePositionLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.mousePositionLabel.Visible = false;
            // 
            // ocrIndicatorPanel
            // 
            this.ocrIndicatorPanel.Anchor = ((System.Windows.Forms.AnchorStyles) ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.ocrIndicatorPanel.AutoSize = true;
            this.ocrIndicatorPanel.BackColor = System.Drawing.Color.Transparent;
            this.ocrIndicatorPanel.Location = new System.Drawing.Point(0, 0);
            this.ocrIndicatorPanel.Name = "ocrIndicatorPanel";
            this.ocrIndicatorPanel.Size = new System.Drawing.Size(1083, 590);
            this.ocrIndicatorPanel.TabIndex = 0;
            this.ocrIndicatorPanel.Visible = false;
            this.ocrIndicatorPanel.VisibleChanged += new System.EventHandler(this.ocrIndicatorPanel_VisibleChanged);
            this.ocrIndicatorPanel.Click += new System.EventHandler(this.ocrIndicatorPanel_Click);
            // 
            // paintTimer
            // 
            this.paintTimer.Interval = 10;
            this.paintTimer.Tick += new System.EventHandler(this.paintOcrIndicators);
            // 
            // subscriptionCheckTimer
            // 
            this.subscriptionCheckTimer.Interval = 25000;
            this.subscriptionCheckTimer.Tick += new System.EventHandler(this.OnSubscriptionCheckTimer);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1083, 590);
            this.Controls.Add(this.toastPanel);
            this.Controls.Add(this.sidebarPanel);
            this.Controls.Add(this.dofusClientPanel);
            this.Controls.Add(this.ocrIndicatorPanel);
            this.HelpButton = true;
            this.Icon = ((System.Drawing.Icon) (resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Text = "Inkybot";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.VisibleChanged += new System.EventHandler(this.MainForm_VisibleChanged);
            this.dofusClientPanel.ResumeLayout(false);
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

        private System.Windows.Forms.Panel ocrIndicatorPanelRect1;

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
        private TransparentPanel ocrIndicatorPanel;
        private System.Windows.Forms.Timer paintTimer;
        private System.Windows.Forms.Timer subscriptionCheckTimer;
    }
}