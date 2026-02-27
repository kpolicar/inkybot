using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Inkybot
{
    partial class WaitingForDofusForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WaitingForDofusForm));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.logoPictureBox = new System.Windows.Forms.PictureBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.errorTopRule = new System.Windows.Forms.Panel();
            this.errorWrapper = new System.Windows.Forms.Panel();
            this.errorBottomMargin = new System.Windows.Forms.Panel();
            this.errorMessage = new System.Windows.Forms.Label();
            this.waitingLabel = new System.Windows.Forms.Label();
            this.processListView = new System.Windows.Forms.ListView();
            this.dofusFileDialog = new System.Windows.Forms.OpenFileDialog();
            ((System.ComponentModel.ISupportInitialize) (this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize) (this.logoPictureBox)).BeginInit();
            this.panel1.SuspendLayout();
            this.errorWrapper.SuspendLayout();
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
            // logoPictureBox
            // 
            resources.ApplyResources(this.logoPictureBox, "logoPictureBox");
            this.logoPictureBox.Name = "logoPictureBox";
            this.logoPictureBox.TabStop = false;
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
            this.panel1.Controls.Add(this.processListView);
            this.panel1.Controls.Add(this.waitingLabel);
            this.panel1.Controls.Add(this.errorBottomMargin);
            this.panel1.Controls.Add(this.errorTopRule);
            this.panel1.Controls.Add(this.errorWrapper);
            this.panel1.Name = "panel1";
            //
            // errorTopRule
            //
            this.errorTopRule.Dock = DockStyle.Top;
            this.errorTopRule.Height = 2;
            this.errorTopRule.BackColor = System.Drawing.Color.Firebrick;
            this.errorTopRule.Name = "errorTopRule";
            this.errorTopRule.Visible = false;
            //
            // errorWrapper
            //
            this.errorWrapper.Dock = DockStyle.Top;
            this.errorWrapper.BackColor = System.Drawing.Color.MistyRose;
            this.errorWrapper.Padding = new System.Windows.Forms.Padding(8, 6, 8, 6);
            this.errorWrapper.AutoSize = true;
            this.errorWrapper.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.errorWrapper.Name = "errorWrapper";
            this.errorWrapper.Visible = false;
            this.errorWrapper.Controls.Add(this.errorMessage);
            //
            // errorBottomMargin
            //
            this.errorBottomMargin.Dock = DockStyle.Top;
            this.errorBottomMargin.Height = 6;
            this.errorBottomMargin.Name = "errorBottomMargin";
            this.errorBottomMargin.Visible = false;
            //
            // errorMessage
            //
            resources.ApplyResources(this.errorMessage, "errorMessage");
            this.errorMessage.Font = new System.Drawing.Font("Calibri", 8.25f, System.Drawing.FontStyle.Bold);
            this.errorMessage.ForeColor = System.Drawing.Color.Firebrick;
            this.errorMessage.BackColor = System.Drawing.Color.Transparent;
            this.errorMessage.Name = "errorMessage";
            this.errorMessage.Dock = DockStyle.Top;
            // 
            // waitingLabel
            // 
            resources.ApplyResources(this.waitingLabel, "waitingLabel");
            this.waitingLabel.Name = "waitingLabel";
            this.waitingLabel.Dock = DockStyle.Top;
            // 
            // waitingLabel
            // 
            resources.ApplyResources(this.waitingLabel, "processListView");
            this.processListView.Name = "processListView";
            this.processListView.Size = new Size(172, 56);
            this.processListView.Location = new Point(23, 150);
            this.processListView.Dock = DockStyle.Top;
            this.processListView.FullRowSelect = true;
            this.processListView.MultiSelect = false;
            this.processListView.View = View.Details;
            this.processListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.processListView.GridLines = false;
            this.processListView.Columns.Add("Process");
            this.processListView.ItemActivate += OnItemActivated;
            this.processListView.Visible = false;
            processListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            processListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            // 
            // dofusFileDialog
            // 
            this.dofusFileDialog.DefaultExt = "exe";
            resources.ApplyResources(this.dofusFileDialog, "dofusFileDialog");
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
            this.Name = "WaitingForDofusForm";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize) (this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize) (this.logoPictureBox)).EndInit();
            this.errorWrapper.ResumeLayout(false);
            this.errorWrapper.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.OpenFileDialog dofusFileDialog;

        private System.Windows.Forms.Label waitingLabel;

        private System.Windows.Forms.PictureBox logoPictureBox;

        private System.Windows.Forms.Label errorMessage;

        private System.Windows.Forms.Panel errorWrapper;
        private System.Windows.Forms.Panel errorTopRule;
        private System.Windows.Forms.Panel errorBottomMargin;

        private System.Windows.Forms.Panel panel1;

        private System.Windows.Forms.Label label2;

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListView processListView;

        private System.Windows.Forms.SplitContainer splitContainer1;

        #endregion
    }
}