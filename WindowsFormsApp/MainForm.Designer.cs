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
        private void InitializeComponent() {
            System.ComponentModel.ComponentResourceManager resources =
                new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.panel1 = new System.Windows.Forms.Panel();
            this.statusBarPanel1 = new System.Windows.Forms.StatusBarPanel();
            this.statusBarPanel2 = new WindowsFormsApp.Controls.MousePositionStatusBarPanel();
            this.statusBar1 = new System.Windows.Forms.StatusBar();
            ((System.ComponentModel.ISupportInitialize) (this.statusBarPanel1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize) (this.statusBarPanel2)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1263, 681);
            this.panel1.TabIndex = 0;
            // 
            // statusBarPanel1
            // 
            this.statusBarPanel1.AutoSize = System.Windows.Forms.StatusBarPanelAutoSize.Spring;
            this.statusBarPanel1.Name = "statusBarPanel1";
            this.statusBarPanel1.Width = 1253;
            // 
            // statusBarPanel2
            // 
            this.statusBarPanel2.Alignment = System.Windows.Forms.HorizontalAlignment.Right;
            this.statusBarPanel2.AutoSize = System.Windows.Forms.StatusBarPanelAutoSize.Contents;
            this.statusBarPanel2.BorderStyle = System.Windows.Forms.StatusBarPanelBorderStyle.Raised;
            this.statusBarPanel2.Name = "statusBarPanel2";
            this.statusBarPanel2.Width = 10;
            // 
            // statusBar1
            // 
            this.statusBar1.Location = new System.Drawing.Point(0, 659);
            this.statusBar1.Name = "statusBar1";
            this.statusBar1.Panels.AddRange(new System.Windows.Forms.StatusBarPanel[]
                {this.statusBarPanel1, this.statusBarPanel2});
            this.statusBar1.ShowPanels = true;
            this.statusBar1.Size = new System.Drawing.Size(1263, 22);
            this.statusBar1.SizingGrip = false;
            this.statusBar1.TabIndex = 0;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1263, 681);
            this.Controls.Add(this.statusBar1);
            this.Controls.Add(this.panel1);
            this.HelpButton = true;
            this.Icon = ((System.Drawing.Icon) (resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Text = "Inkybot";
            ((System.ComponentModel.ISupportInitialize) (this.statusBarPanel1)).EndInit();
            ((System.ComponentModel.ISupportInitialize) (this.statusBarPanel2)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.StatusBar statusBar1;
        private System.Windows.Forms.StatusBarPanel statusBarPanel1;
        private WindowsFormsApp.Controls.MousePositionStatusBarPanel statusBarPanel2;
    }
}