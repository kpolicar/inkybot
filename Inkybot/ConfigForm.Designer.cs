using System.ComponentModel;

namespace Inkybot
{
    partial class ConfigForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConfigForm));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.statsDataGridView = new System.Windows.Forms.DataGridView();
            this.StatColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.PaRuneThresholdColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.RaRuneThresholdColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.MaxSmRuneCanHitColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.MaxPaRuneCanHitColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.restoreHighSinkStatsCheckbox = new System.Windows.Forms.CheckBox();
            this.creditsNameLabel = new System.Windows.Forms.Label();
            this.bottomPanel = new System.Windows.Forms.Panel();
            this.creditsLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize) (this.statsDataGridView)).BeginInit();
            this.bottomPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // statsDataGridView
            // 
            resources.ApplyResources(this.statsDataGridView, "statsDataGridView");
            this.statsDataGridView.AllowUserToAddRows = false;
            this.statsDataGridView.AllowUserToResizeColumns = false;
            this.statsDataGridView.AllowUserToResizeRows = false;
            this.statsDataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.statsDataGridView.BackgroundColor = System.Drawing.Color.FromArgb(((int) (((byte) (30)))), ((int) (((byte) (30)))), ((int) (((byte) (30)))));
            this.statsDataGridView.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (15)))), ((int) (((byte) (15)))), ((int) (((byte) (15)))));
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Calibri", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.Color.FromArgb(((int) (((byte) (15)))), ((int) (((byte) (15)))), ((int) (((byte) (15)))));
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.statsDataGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.statsDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.statsDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {this.StatColumn, this.PaRuneThresholdColumn, this.RaRuneThresholdColumn, this.MaxSmRuneCanHitColumn, this.MaxPaRuneCanHitColumn});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (30)))), ((int) (((byte) (30)))), ((int) (((byte) (30)))));
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Calibri", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(((int) (((byte) (60)))), ((int) (((byte) (60)))), ((int) (((byte) (60)))));
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.statsDataGridView.DefaultCellStyle = dataGridViewCellStyle2;
            this.statsDataGridView.EnableHeadersVisualStyles = false;
            this.statsDataGridView.GridColor = System.Drawing.Color.FromArgb(((int) (((byte) (70)))), ((int) (((byte) (70)))), ((int) (((byte) (70)))));
            this.statsDataGridView.MultiSelect = false;
            this.statsDataGridView.Name = "statsDataGridView";
            this.statsDataGridView.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (64)))), ((int) (((byte) (64)))), ((int) (((byte) (64)))));
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Segoe UI", 9F);
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.FromArgb(((int) (((byte) (15)))), ((int) (((byte) (15)))), ((int) (((byte) (15)))));
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.Color.FromArgb(((int) (((byte) (64)))), ((int) (((byte) (64)))), ((int) (((byte) (64)))));
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.statsDataGridView.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.statsDataGridView.RowHeadersVisible = false;
            this.statsDataGridView.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.statsDataGridView.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.ConfigForm_OnChangeValue);
            this.statsDataGridView.CellValidating += new System.Windows.Forms.DataGridViewCellValidatingEventHandler(Inkybot.Helpers.DataGridView.OnValidatingDataGridViewCellNumeric);
            // 
            // StatColumn
            // 
            resources.ApplyResources(this.StatColumn, "StatColumn");
            this.StatColumn.Name = "StatColumn";
            this.StatColumn.ReadOnly = true;
            // 
            // PaRuneThresholdColumn
            // 
            resources.ApplyResources(this.PaRuneThresholdColumn, "PaRuneThresholdColumn");
            this.PaRuneThresholdColumn.Name = "PaRuneThresholdColumn";
            // 
            // RaRuneThresholdColumn
            // 
            resources.ApplyResources(this.RaRuneThresholdColumn, "RaRuneThresholdColumn");
            this.RaRuneThresholdColumn.Name = "RaRuneThresholdColumn";
            // 
            // MaxSmRuneCanHitColumn
            // 
            resources.ApplyResources(this.MaxSmRuneCanHitColumn, "MaxSmRuneCanHitColumn");
            this.MaxSmRuneCanHitColumn.Name = "MaxSmRuneCanHitColumn";
            // 
            // MaxPaRuneCanHitColumn
            // 
            resources.ApplyResources(this.MaxPaRuneCanHitColumn, "MaxPaRuneCanHitColumn");
            this.MaxPaRuneCanHitColumn.Name = "MaxPaRuneCanHitColumn";
            // 
            // restoreHighSinkStatsCheckbox
            // 
            resources.ApplyResources(this.restoreHighSinkStatsCheckbox, "restoreHighSinkStatsCheckbox");
            this.restoreHighSinkStatsCheckbox.ForeColor = System.Drawing.SystemColors.Control;
            this.restoreHighSinkStatsCheckbox.Name = "restoreHighSinkStatsCheckbox";
            this.restoreHighSinkStatsCheckbox.UseVisualStyleBackColor = true;
            this.restoreHighSinkStatsCheckbox.CheckedChanged += new System.EventHandler(this.ConfigForm_OnRestoreHighSinkStatsCheckboxCheckedChanged);
            // 
            // creditsNameLabel
            // 
            resources.ApplyResources(this.creditsNameLabel, "creditsNameLabel");
            this.creditsNameLabel.ForeColor = System.Drawing.SystemColors.Control;
            this.creditsNameLabel.Name = "creditsNameLabel";
            // 
            // bottomPanel
            // 
            resources.ApplyResources(this.bottomPanel, "bottomPanel");
            this.bottomPanel.Controls.Add(this.creditsLabel);
            this.bottomPanel.Controls.Add(this.creditsNameLabel);
            this.bottomPanel.Controls.Add(this.restoreHighSinkStatsCheckbox);
            this.bottomPanel.Name = "bottomPanel";
            // 
            // creditsLabel
            // 
            resources.ApplyResources(this.creditsLabel, "creditsLabel");
            this.creditsLabel.ForeColor = System.Drawing.SystemColors.Control;
            this.creditsLabel.Name = "creditsLabel";
            // 
            // ConfigForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (30)))), ((int) (((byte) (30)))), ((int) (((byte) (30)))));
            this.Controls.Add(this.statsDataGridView);
            this.Controls.Add(this.bottomPanel);
            this.Name = "ConfigForm";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.ConfigForm_Closing);
            this.Load += new System.EventHandler(this.ConfigForm_OnLoad);
            ((System.ComponentModel.ISupportInitialize) (this.statsDataGridView)).EndInit();
            this.bottomPanel.ResumeLayout(false);
            this.bottomPanel.PerformLayout();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Panel bottomPanel;

        private System.Windows.Forms.Label creditsLabel;
        private System.Windows.Forms.Label creditsNameLabel;

        private System.Windows.Forms.CheckBox restoreHighSinkStatsCheckbox;

        private System.Windows.Forms.DataGridViewTextBoxColumn MaxPaRuneCanHitColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn MaxSmRuneCanHitColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn PaRuneThresholdColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn RaRuneThresholdColumn;

        private System.Windows.Forms.DataGridView statsDataGridView;

        private System.Windows.Forms.DataGridViewTextBoxColumn StatColumn;

        #endregion
    }
}

