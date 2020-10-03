using System.Drawing;

namespace WindowsFormsApp
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
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.dofusClientPanel = new System.Windows.Forms.Panel();
            this.sidebarPanel = new System.Windows.Forms.Panel();
            this.buttonsPanel = new System.Windows.Forms.Panel();
            this.primaryButtonsPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.toggleMageButton = new System.Windows.Forms.Button();
            this.secondaryButtonsPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.debugButton = new System.Windows.Forms.Button();
            this.helpButton = new System.Windows.Forms.Button();
            this.mageInfoPanel = new System.Windows.Forms.Panel();
            this.mousePositionLabel = new System.Windows.Forms.Label();
            this.sinkValueLabel = new System.Windows.Forms.Label();
            this.sinkLabel = new System.Windows.Forms.Label();
            this.userInfoPanel = new System.Windows.Forms.Panel();
            this.subscribedInfoLabel = new System.Windows.Forms.Label();
            this.usernameLabel = new System.Windows.Forms.Label();
            this.ocrIndicatorPanel = new WindowsFormsApp.Controls.TransparentPanel();
            this.paintTimer = new System.Windows.Forms.Timer(this.components);
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
            this.dofusClientPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dofusClientPanel.Location = new System.Drawing.Point(0, 0);
            this.dofusClientPanel.Margin = new System.Windows.Forms.Padding(0);
            this.dofusClientPanel.Name = "dofusClientPanel";
            this.dofusClientPanel.Size = new System.Drawing.Size(1083, 590);
            this.dofusClientPanel.TabIndex = 0;
            // 
            // sidebarPanel
            // 
            this.sidebarPanel.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left)));
            this.sidebarPanel.AutoSize = true;
            this.sidebarPanel.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (172)))), ((int) (((byte) (211)))), ((int) (((byte) (40)))));
            this.sidebarPanel.Controls.Add(this.buttonsPanel);
            this.sidebarPanel.Controls.Add(this.mageInfoPanel);
            this.sidebarPanel.Controls.Add(this.userInfoPanel);
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
            this.buttonsPanel.Location = new System.Drawing.Point(0, 70);
            this.buttonsPanel.Name = "buttonsPanel";
            this.buttonsPanel.Size = new System.Drawing.Size(113, 433);
            this.buttonsPanel.TabIndex = 8;
            // 
            // primaryButtonsPanel
            // 
            this.primaryButtonsPanel.AutoSize = true;
            this.primaryButtonsPanel.Controls.Add(this.toggleMageButton);
            this.primaryButtonsPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.primaryButtonsPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.primaryButtonsPanel.Location = new System.Drawing.Point(0, 0);
            this.primaryButtonsPanel.Name = "primaryButtonsPanel";
            this.primaryButtonsPanel.Size = new System.Drawing.Size(113, 64);
            this.primaryButtonsPanel.TabIndex = 4;
            // 
            // toggleMageButton
            // 
            this.toggleMageButton.AutoSize = true;
            this.toggleMageButton.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (136)))), ((int) (((byte) (165)))), ((int) (((byte) (31)))));
            this.toggleMageButton.FlatAppearance.BorderSize = 0;
            this.toggleMageButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.toggleMageButton.Location = new System.Drawing.Point(0, 0);
            this.toggleMageButton.Margin = new System.Windows.Forms.Padding(0, 0, 0, 3);
            this.toggleMageButton.Name = "toggleMageButton";
            this.toggleMageButton.Size = new System.Drawing.Size(113, 61);
            this.toggleMageButton.TabIndex = 0;
            this.toggleMageButton.Text = "Start\n(F2)";
            this.toggleMageButton.UseVisualStyleBackColor = false;
            this.toggleMageButton.Click += new System.EventHandler(this.toggleMageButton_Click);
            // 
            // secondaryButtonsPanel
            // 
            this.secondaryButtonsPanel.Controls.Add(this.debugButton);
            this.secondaryButtonsPanel.Controls.Add(this.helpButton);
            this.secondaryButtonsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.secondaryButtonsPanel.FlowDirection = System.Windows.Forms.FlowDirection.BottomUp;
            this.secondaryButtonsPanel.Location = new System.Drawing.Point(0, 318);
            this.secondaryButtonsPanel.Name = "secondaryButtonsPanel";
            this.secondaryButtonsPanel.Size = new System.Drawing.Size(113, 115);
            this.secondaryButtonsPanel.TabIndex = 3;
            // 
            // debugButton
            // 
            this.debugButton.AutoSize = true;
            this.debugButton.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (136)))), ((int) (((byte) (165)))), ((int) (((byte) (31)))));
            this.debugButton.FlatAppearance.BorderSize = 0;
            this.debugButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.debugButton.Location = new System.Drawing.Point(0, 70);
            this.debugButton.Margin = new System.Windows.Forms.Padding(0, 3, 0, 0);
            this.debugButton.Name = "debugButton";
            this.debugButton.Size = new System.Drawing.Size(113, 45);
            this.debugButton.TabIndex = 2;
            this.debugButton.Text = "Debug";
            this.debugButton.UseVisualStyleBackColor = false;
            this.debugButton.Click += new System.EventHandler(this.debugButton_Click);
            // 
            // helpButton
            // 
            this.helpButton.AutoSize = true;
            this.helpButton.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (136)))), ((int) (((byte) (165)))), ((int) (((byte) (31)))));
            this.helpButton.FlatAppearance.BorderSize = 0;
            this.helpButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.helpButton.Location = new System.Drawing.Point(0, 22);
            this.helpButton.Margin = new System.Windows.Forms.Padding(0, 3, 0, 0);
            this.helpButton.Name = "helpButton";
            this.helpButton.Size = new System.Drawing.Size(113, 45);
            this.helpButton.TabIndex = 1;
            this.helpButton.Text = "Help";
            this.helpButton.UseVisualStyleBackColor = false;
            this.helpButton.Click += new System.EventHandler(this.helpButton_Click);
            // 
            // mageInfoPanel
            // 
            this.mageInfoPanel.AutoSize = true;
            this.mageInfoPanel.BackColor = System.Drawing.Color.Transparent;
            this.mageInfoPanel.Controls.Add(this.mousePositionLabel);
            this.mageInfoPanel.Controls.Add(this.sinkValueLabel);
            this.mageInfoPanel.Controls.Add(this.sinkLabel);
            this.mageInfoPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.mageInfoPanel.Location = new System.Drawing.Point(0, 503);
            this.mageInfoPanel.Name = "mageInfoPanel";
            this.mageInfoPanel.Padding = new System.Windows.Forms.Padding(0, 15, 0, 5);
            this.mageInfoPanel.Size = new System.Drawing.Size(113, 87);
            this.mageInfoPanel.TabIndex = 7;
            // 
            // mousePositionLabel
            // 
            this.mousePositionLabel.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (172)))), ((int) (((byte) (211)))), ((int) (((byte) (40)))));
            this.mousePositionLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.mousePositionLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            this.mousePositionLabel.Location = new System.Drawing.Point(0, 52);
            this.mousePositionLabel.Name = "mousePositionLabel";
            this.mousePositionLabel.Padding = new System.Windows.Forms.Padding(0, 5, 0, 0);
            this.mousePositionLabel.Size = new System.Drawing.Size(113, 30);
            this.mousePositionLabel.TabIndex = 8;
            this.mousePositionLabel.Text = "(0,0)";
            this.mousePositionLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.mousePositionLabel.Visible = false;
            // 
            // sinkValueLabel
            // 
            this.sinkValueLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.sinkValueLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
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
            this.userInfoPanel.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (155)))), ((int) (((byte) (188)))), ((int) (((byte) (35)))));
            this.userInfoPanel.Controls.Add(this.subscribedInfoLabel);
            this.userInfoPanel.Controls.Add(this.usernameLabel);
            this.userInfoPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.userInfoPanel.Location = new System.Drawing.Point(0, 0);
            this.userInfoPanel.Name = "userInfoPanel";
            this.userInfoPanel.Padding = new System.Windows.Forms.Padding(0, 15, 0, 0);
            this.userInfoPanel.Size = new System.Drawing.Size(113, 70);
            this.userInfoPanel.TabIndex = 4;
            // 
            // subscribedInfoLabel
            // 
            this.subscribedInfoLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.subscribedInfoLabel.Location = new System.Drawing.Point(0, 35);
            this.subscribedInfoLabel.Name = "subscribedInfoLabel";
            this.subscribedInfoLabel.Size = new System.Drawing.Size(113, 35);
            this.subscribedInfoLabel.TabIndex = 3;
            this.subscribedInfoLabel.Text = "Subscribed until: 24/09/2022";
            // 
            // usernameLabel
            // 
            this.usernameLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.usernameLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            this.usernameLabel.Location = new System.Drawing.Point(0, 15);
            this.usernameLabel.Name = "usernameLabel";
            this.usernameLabel.Size = new System.Drawing.Size(113, 20);
            this.usernameLabel.TabIndex = 2;
            this.usernameLabel.Text = "Klemen";
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
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1083, 590);
            this.Controls.Add(this.sidebarPanel);
            this.Controls.Add(this.dofusClientPanel);
            this.Controls.Add(this.ocrIndicatorPanel);
            this.HelpButton = true;
            this.Icon = ((System.Drawing.Icon) (resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Text = "Inkybot";
            this.sidebarPanel.ResumeLayout(false);
            this.sidebarPanel.PerformLayout();
            this.buttonsPanel.ResumeLayout(false);
            this.buttonsPanel.PerformLayout();
            this.primaryButtonsPanel.ResumeLayout(false);
            this.primaryButtonsPanel.PerformLayout();
            this.secondaryButtonsPanel.ResumeLayout(false);
            this.secondaryButtonsPanel.PerformLayout();
            this.mageInfoPanel.ResumeLayout(false);
            this.userInfoPanel.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

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
        private System.Windows.Forms.Panel sidebarPanel;
        private WindowsFormsApp.Controls.TransparentPanel ocrIndicatorPanel;
        private System.Windows.Forms.Timer paintTimer;
    }
}