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
            this.mageQueueGroupBoxesPanel = new System.Windows.Forms.TableLayoutPanel();
            this.emptyLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // mageQueueGroupBoxesPanel
            // 
            this.mageQueueGroupBoxesPanel.AutoScroll = true;
            this.mageQueueGroupBoxesPanel.ColumnCount = 1;
            this.mageQueueGroupBoxesPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mageQueueGroupBoxesPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mageQueueGroupBoxesPanel.Location = new System.Drawing.Point(0, 0);
            this.mageQueueGroupBoxesPanel.Name = "mageQueueGroupBoxesPanel";
            this.mageQueueGroupBoxesPanel.Size = new System.Drawing.Size(642, 450);
            this.mageQueueGroupBoxesPanel.TabIndex = 0;
            this.mageQueueGroupBoxesPanel.Visible = false;
            // 
            // emptyLabel
            // 
            this.emptyLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.emptyLabel.ForeColor = System.Drawing.SystemColors.Control;
            this.emptyLabel.Location = new System.Drawing.Point(0, 0);
            this.emptyLabel.Name = "emptyLabel";
            this.emptyLabel.Size = new System.Drawing.Size(642, 450);
            this.emptyLabel.TabIndex = 1;
            this.emptyLabel.Text = "There are no items in the queue";
            this.emptyLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // MageQueueForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (30)))), ((int) (((byte) (30)))), ((int) (((byte) (30)))));
            this.ClientSize = new System.Drawing.Size(642, 450);
            this.MinimumSize = new System.Drawing.Size(512, 384);
            this.MinimizeBox = false;
            this.MaximizeBox = false;
            this.Controls.Add(this.emptyLabel);
            this.Controls.Add(this.mageQueueGroupBoxesPanel);
            this.Name = "MageQueueForm";
            this.Text = "Inkybot - Mage Queue";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.MageQueueForm_Closing);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Label emptyLabel;

        private System.Windows.Forms.TableLayoutPanel mageQueueGroupBoxesPanel;

        #endregion
    }
}

