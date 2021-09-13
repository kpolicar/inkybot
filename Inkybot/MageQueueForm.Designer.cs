using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

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

        private Pen borderPen = new Pen(Color.Black);
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            resources = new System.ComponentModel.ComponentResourceManager(typeof(MageQueueForm));
            this.mageQueueGroupBoxesPanel = new System.Windows.Forms.TableLayoutPanel();
            this.bottomPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.emptyLabel = new System.Windows.Forms.Label();
            this.clearQueueButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // mageQueueGroupBoxesPanel
            // 
            resources.ApplyResources(this.mageQueueGroupBoxesPanel, "mageQueueGroupBoxesPanel");
            this.mageQueueGroupBoxesPanel.Name = "mageQueueGroupBoxesPanel";
            // 
            // clearQueueButton
            // 
            resources.ApplyResources(this.clearQueueButton, "clearQueueButton");
            this.clearQueueButton.AutoSize = true;
            this.clearQueueButton.BackColor = System.Drawing.Color.Black;
            this.clearQueueButton.FlatAppearance.BorderSize = 0;
            this.clearQueueButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.clearQueueButton.ForeColor = System.Drawing.SystemColors.Control;
            this.clearQueueButton.Location = new System.Drawing.Point(0,0);
            this.clearQueueButton.Name = "clearQueueButton";
            this.clearQueueButton.TabIndex = 6;
            this.clearQueueButton.UseVisualStyleBackColor = false;
            this.clearQueueButton.Click += OnClearQueueButtonClick;
            this.clearQueueButton.Margin = Padding.Empty;
            this.clearQueueButton.Padding = Padding.Empty;
            // 
            // bottomPanel
            // 
            resources.ApplyResources(this.bottomPanel, "bottomPanel");
            this.bottomPanel.Name = "bottomPanel";
            this.bottomPanel.Dock = DockStyle.Bottom;
            this.bottomPanel.Height = 43;
            this.bottomPanel.Padding = new Padding(0, 3, 0, 3);
            this.bottomPanel.Margin = Padding.Empty;
            this.bottomPanel.Visible = false;
            this.bottomPanel.Controls.Add(clearQueueButton);
            this.bottomPanel.Paint += (sender, e) => {
                e.Graphics.DrawLine(borderPen, 0, 0, this.bottomPanel.ClientRectangle.Width, 1);
            };
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
            this.Controls.Add(this.bottomPanel);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MageQueueForm";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.MageQueueForm_Closing);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Label emptyLabel;

        private System.Windows.Forms.TableLayoutPanel mageQueueGroupBoxesPanel;
        private System.Windows.Forms.FlowLayoutPanel bottomPanel;
        private System.Windows.Forms.Button clearQueueButton;

        #endregion
    }
}

