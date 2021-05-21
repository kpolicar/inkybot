using System.ComponentModel;

namespace Inkybot
{
    partial class StatisticsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StatisticsForm));
            this.webBrowser = new System.Windows.Forms.WebBrowser();
            this.openInBrowserLabelLink = new System.Windows.Forms.LinkLabel();
            this.tooltip = new System.Windows.Forms.ToolTip();
            this.refreshButton = new System.Windows.Forms.Button();
            this.topPanel = new System.Windows.Forms.Panel();
            this.sidebarPanel = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // webBrowser
            // 
            resources.ApplyResources(this.webBrowser, "webBrowser");
            this.webBrowser.Name = "webBrowser";
            this.webBrowser.ScriptErrorsSuppressed = true;
            // 
            // refreshButton
            // 
            resources.ApplyResources(this.refreshButton, "refreshButton");
            this.refreshButton.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (30)))), ((int) (((byte) (30)))), ((int) (((byte) (30)))));
            this.refreshButton.FlatAppearance.BorderSize = 0;
            this.refreshButton.ForeColor = System.Drawing.SystemColors.Control;
            this.refreshButton.Name = "refreshButton";
            this.refreshButton.UseVisualStyleBackColor = false;
            this.refreshButton.Padding = System.Windows.Forms.Padding.Empty;
            this.refreshButton.Margin = System.Windows.Forms.Padding.Empty;
            this.refreshButton.Click += new System.EventHandler(this.refreshButton_Click);
            this.refreshButton.Paint += new System.Windows.Forms.PaintEventHandler(this.OnRefreshButtonPaint);
            //
            // sidebarPanel
            // 
            resources.ApplyResources(this.sidebarPanel, "sidebarPanel");
            this.sidebarPanel.Controls.Add(this.refreshButton);
            this.sidebarPanel.Name = "sidebarPanel";
            this.sidebarPanel.Dock = System.Windows.Forms.DockStyle.Right;
            this.sidebarPanel.AutoSize = true;
            //
            // mainPanel
            // 
            resources.ApplyResources(this.topPanel, "topPanel");
            this.topPanel.Height = 20;
            this.topPanel.Controls.Add(this.openInBrowserLabelLink);
            this.topPanel.Controls.Add(this.sidebarPanel);
            this.topPanel.Name = "topPanel";
            this.topPanel.Dock = System.Windows.Forms.DockStyle.Top;
            // 
            // refreshButtonTooltip
            // 
            tooltip.AutomaticDelay = 50;
            tooltip.AutoPopDelay = int.MaxValue;
            tooltip.SetToolTip(this.refreshButton, resources.GetString("refreshButton.ToolTipText"));
            // 
            // openInBrowserLabelLink
            // 
            this.openInBrowserLabelLink.ActiveLinkColor = System.Drawing.Color.FromArgb(((int) (((byte) (150)))), ((int) (((byte) (150)))), ((int) (((byte) (150)))));
            this.openInBrowserLabelLink.BackColor = System.Drawing.Color.Black;
            resources.ApplyResources(this.openInBrowserLabelLink, "openInBrowserLabelLink");
            this.openInBrowserLabelLink.LinkColor = System.Drawing.Color.White;
            this.openInBrowserLabelLink.Name = "openInBrowserLabelLink";
            this.openInBrowserLabelLink.TabStop = true;
            this.openInBrowserLabelLink.VisitedLinkColor = System.Drawing.Color.FromArgb(((int) (((byte) (150)))), ((int) (((byte) (150)))), ((int) (((byte) (150)))));
            this.openInBrowserLabelLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.openInBrowserLabelLink_LinkClicked);
            // 
            // StatisticsForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.webBrowser);
            this.Controls.Add(this.topPanel);
            this.Name = "StatisticsForm";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.StatisticsForm_Closing);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Button refreshButton;
        private System.Windows.Forms.ToolTip tooltip;
        private System.Windows.Forms.Panel topPanel;
        private System.Windows.Forms.Panel sidebarPanel;
        private System.Windows.Forms.LinkLabel openInBrowserLabelLink;

        private System.Windows.Forms.WebBrowser webBrowser;

        #endregion
    }
}

