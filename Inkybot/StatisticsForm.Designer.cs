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
            this.SuspendLayout();
            // 
            // webBrowser
            // 
            resources.ApplyResources(this.webBrowser, "webBrowser");
            this.webBrowser.Name = "webBrowser";
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
            this.Controls.Add(this.openInBrowserLabelLink);
            this.Name = "StatisticsForm";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.StatisticsForm_Closing);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.LinkLabel openInBrowserLabelLink;

        private System.Windows.Forms.WebBrowser webBrowser;

        #endregion
    }
}

