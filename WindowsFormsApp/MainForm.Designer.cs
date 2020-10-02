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
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel6 = new System.Windows.Forms.Panel();
            this.sinkValueLabel = new System.Windows.Forms.Label();
            this.sinkLabel = new System.Windows.Forms.Label();
            this.panel5 = new System.Windows.Forms.Panel();
            this.button1 = new System.Windows.Forms.Button();
            this.panel4 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.usernameLabel = new System.Windows.Forms.Label();
            this.panel3 = new WindowsFormsApp.Controls.TransparentPanel();
            this.paintTimer = new System.Windows.Forms.Timer(this.components);
            this.panel2.SuspendLayout();
            this.panel6.SuspendLayout();
            this.panel5.SuspendLayout();
            this.panel4.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1083, 590);
            this.panel1.TabIndex = 0;
            // 
            // panel2
            // 
            this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left)));
            this.panel2.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (172)))), ((int) (((byte) (211)))), ((int) (((byte) (40)))));
            this.panel2.Controls.Add(this.panel6);
            this.panel2.Controls.Add(this.panel5);
            this.panel2.Controls.Add(this.panel4);
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(113, 590);
            this.panel2.TabIndex = 0;
            // 
            // panel6
            // 
            this.panel6.Controls.Add(this.sinkValueLabel);
            this.panel6.Controls.Add(this.sinkLabel);
            this.panel6.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel6.Location = new System.Drawing.Point(0, 547);
            this.panel6.Name = "panel6";
            this.panel6.Size = new System.Drawing.Size(113, 43);
            this.panel6.TabIndex = 7;
            // 
            // sinkValueLabel
            // 
            this.sinkValueLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.sinkValueLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            this.sinkValueLabel.Location = new System.Drawing.Point(0, 15);
            this.sinkValueLabel.Name = "sinkValueLabel";
            this.sinkValueLabel.Size = new System.Drawing.Size(113, 19);
            this.sinkValueLabel.TabIndex = 7;
            this.sinkValueLabel.Text = "0";
            this.sinkValueLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // sinkLabel
            // 
            this.sinkLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.sinkLabel.Location = new System.Drawing.Point(0, 0);
            this.sinkLabel.Name = "sinkLabel";
            this.sinkLabel.Size = new System.Drawing.Size(113, 15);
            this.sinkLabel.TabIndex = 6;
            this.sinkLabel.Text = "Sink";
            this.sinkLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // panel5
            // 
            this.panel5.Controls.Add(this.button1);
            this.panel5.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel5.Location = new System.Drawing.Point(0, 75);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(113, 45);
            this.panel5.TabIndex = 5;
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (136)))), ((int) (((byte) (165)))), ((int) (((byte) (31)))));
            this.button1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.button1.FlatAppearance.BorderSize = 0;
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.Location = new System.Drawing.Point(0, 0);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(113, 45);
            this.button1.TabIndex = 0;
            this.button1.TabStop = false;
            this.button1.Text = "Show OCR";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.label1);
            this.panel4.Controls.Add(this.usernameLabel);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel4.Location = new System.Drawing.Point(0, 0);
            this.panel4.Margin = new System.Windows.Forms.Padding(15, 3, 3, 3);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(113, 75);
            this.panel4.TabIndex = 4;
            // 
            // label1
            // 
            this.label1.Dock = System.Windows.Forms.DockStyle.Top;
            this.label1.Location = new System.Drawing.Point(0, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(113, 35);
            this.label1.TabIndex = 3;
            this.label1.Text = "Subscribed until: 24/09/2022";
            // 
            // usernameLabel
            // 
            this.usernameLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.usernameLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            this.usernameLabel.Location = new System.Drawing.Point(0, 0);
            this.usernameLabel.Name = "usernameLabel";
            this.usernameLabel.Size = new System.Drawing.Size(113, 23);
            this.usernameLabel.TabIndex = 2;
            this.usernameLabel.Text = "Klemen";
            // 
            // panel3
            // 
            this.panel3.Anchor = ((System.Windows.Forms.AnchorStyles) ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.panel3.BackColor = System.Drawing.Color.Transparent;
            this.panel3.Location = new System.Drawing.Point(0, 0);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(1083, 590);
            this.panel3.TabIndex = 0;
            this.panel3.Visible = false;
            this.panel3.Click += new System.EventHandler(this.panel3_Click);
            this.panel3.VisibleChanged += new System.EventHandler(this.panel3_VisibleChanged);
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
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.panel3);
            this.HelpButton = true;
            this.Icon = ((System.Drawing.Icon) (resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Text = "Inkybot";
            this.panel2.ResumeLayout(false);
            this.panel6.ResumeLayout(false);
            this.panel5.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Label sinkLabel;
        private System.Windows.Forms.Label sinkValueLabel;

        private System.Windows.Forms.Panel panel6;

        private System.Windows.Forms.Panel panel5;

        private System.Windows.Forms.Panel panel4;

        private System.Windows.Forms.Label label1;

        private System.Windows.Forms.Label usernameLabel;

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private WindowsFormsApp.Controls.TransparentPanel panel3;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Timer paintTimer;
    }
}