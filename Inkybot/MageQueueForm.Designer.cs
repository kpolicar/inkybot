using System.ComponentModel;

namespace Inkybot
{
    partial class MageQueueForm
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
            resources = new System.ComponentModel.ComponentResourceManager(typeof(MageQueueForm));
            this.mageQueueGroupBoxesPanel = new System.Windows.Forms.TableLayoutPanel();
            this.emptyLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // mageQueueGroupBoxesPanel
            // 
            resources.ApplyResources(this.mageQueueGroupBoxesPanel, "mageQueueGroupBoxesPanel");
            this.mageQueueGroupBoxesPanel.Name = "mageQueueGroupBoxesPanel";
            // 
            // emptyLabel
            // 
            resources.ApplyResources(this.emptyLabel, "emptyLabel");
            this.emptyLabel.ForeColor = System.Drawing.SystemColors.Control;
            this.emptyLabel.Name = "emptyLabel";
            // 
            // MageQueueForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (30)))), ((int) (((byte) (30)))), ((int) (((byte) (30)))));
            this.Controls.Add(this.emptyLabel);
            this.Controls.Add(this.mageQueueGroupBoxesPanel);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MageQueueForm";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.MageQueueForm_Closing);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Label emptyLabel;

        private System.Windows.Forms.TableLayoutPanel mageQueueGroupBoxesPanel;

        #endregion
    }
}

