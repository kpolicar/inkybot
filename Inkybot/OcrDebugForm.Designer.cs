using System.ComponentModel;

namespace Inkybot
{
    partial class OcrDebugForm
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
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.button1 = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.ocrResultsLabel = new System.Windows.Forms.Label();
            this.timerLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize) (this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(22, 16);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(116, 29);
            this.button1.TabIndex = 0;
            this.button1.Text = "Scan file";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Right;
            this.pictureBox1.Location = new System.Drawing.Point(192, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(601, 631);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 2;
            this.pictureBox1.TabStop = false;
            // 
            // ocrResultsLabel
            // 
            this.ocrResultsLabel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.ocrResultsLabel.Location = new System.Drawing.Point(0, 93);
            this.ocrResultsLabel.Name = "ocrResultsLabel";
            this.ocrResultsLabel.Size = new System.Drawing.Size(192, 538);
            this.ocrResultsLabel.TabIndex = 3;
            // 
            // timerLabel
            // 
            this.timerLabel.Location = new System.Drawing.Point(22, 48);
            this.timerLabel.Name = "timerLabel";
            this.timerLabel.Size = new System.Drawing.Size(113, 36);
            this.timerLabel.TabIndex = 4;
            this.timerLabel.Text = "0:00s\r\n0:00s";
            // 
            // OcrDebugForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(793, 631);
            this.Controls.Add(this.timerLabel);
            this.Controls.Add(this.ocrResultsLabel);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.button1);
            this.Name = "OcrDebugForm";
            this.Text = "OcrTest";
            ((System.ComponentModel.ISupportInitialize) (this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Label ocrResultsLabel;
        private System.Windows.Forms.Label timerLabel;

        private System.Windows.Forms.PictureBox pictureBox1;

        private System.Windows.Forms.Button button1;

        private System.Windows.Forms.OpenFileDialog openFileDialog1;

        #endregion
    }
}

