using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Inkybot
{
    partial class StatsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

        private System.ComponentModel.ComponentResourceManager resources;

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

        private void InitializeCustomComponents() {
            exoCellStyle = new DataGridViewCellStyle();
            exoCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            exoCellStyle.BackColor = System.Drawing.Color.FromArgb(20, 20, 20);
            exoCellStyle.Font = new System.Drawing.Font("Calibri", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            exoCellStyle.ForeColor = System.Drawing.SystemColors.Control;
            exoCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(60, 60, 60);
            exoCellStyle.SelectionForeColor = System.Drawing.Color.FromArgb(20, 20, 20);
            exoCellStyle.WrapMode = DataGridViewTriState.True;
            
            unmageableCellStyle = new DataGridViewCellStyle();
            unmageableCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            unmageableCellStyle.BackColor = System.Drawing.Color.FromArgb(50,50,50);
            unmageableCellStyle.Font = new System.Drawing.Font("Calibri", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            unmageableCellStyle.ForeColor = System.Drawing.SystemColors.Control;
            unmageableCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(50,50,50);
            unmageableCellStyle.SelectionForeColor = System.Drawing.SystemColors.Control;
            unmageableCellStyle.WrapMode = DataGridViewTriState.True;
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            resources = new System.ComponentModel.ComponentResourceManager(typeof(StatsForm));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.TargetColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ValueColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.StatColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.statsDataGridView = new System.Windows.Forms.DataGridView();
            this.actionsPanel = new System.Windows.Forms.Panel();
            this.addExoPanel = new System.Windows.Forms.Panel();
            this.presetPanel = new System.Windows.Forms.Panel();
            this.selectPresetPanel = new System.Windows.Forms.Panel();
            this.addExoButton = new System.Windows.Forms.Button();
            this.exoStatComboBox = new Inkybot.Controls.ComboBox();
            this.presetsComboBox = new Inkybot.Controls.ComboBox();
            this.clearExosButton = new System.Windows.Forms.Button();
            this.deletePresetButton = new System.Windows.Forms.Button();
            this.addPresetButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize) (this.statsDataGridView)).BeginInit();
            this.actionsPanel.SuspendLayout();
            this.addExoPanel.SuspendLayout();
            this.presetPanel.SuspendLayout();
            this.selectPresetPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // TargetColumn
            // 
            resources.ApplyResources(this.TargetColumn, "TargetColumn");
            this.TargetColumn.Name = "TargetColumn";
            this.TargetColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // ValueColumn
            // 
            resources.ApplyResources(this.ValueColumn, "ValueColumn");
            this.ValueColumn.Name = "ValueColumn";
            this.ValueColumn.ReadOnly = true;
            this.ValueColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // StatColumn
            // 
            resources.ApplyResources(this.StatColumn, "StatColumn");
            this.StatColumn.Name = "StatColumn";
            this.StatColumn.ReadOnly = true;
            this.StatColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
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
            this.statsDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {this.StatColumn, this.ValueColumn, this.TargetColumn});
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
            this.statsDataGridView.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.StatsForm_OnChangeValue);
            // 
            // actionsPanel
            // 
            resources.ApplyResources(this.actionsPanel, "actionsPanel");
            this.actionsPanel.Controls.Add(this.addExoPanel);
            this.actionsPanel.Controls.Add(this.presetPanel);
            this.actionsPanel.Controls.Add(this.clearExosButton);
            this.actionsPanel.Name = "actionsPanel";
            // 
            // addExoPanel
            // 
            resources.ApplyResources(this.addExoPanel, "addExoPanel");
            this.addExoPanel.Controls.Add(this.addExoButton);
            this.addExoPanel.Controls.Add(this.exoStatComboBox);
            this.addExoPanel.Name = "addExoPanel";
            this.addExoPanel.Dock = DockStyle.Left;
            this.addExoPanel.Width = 130;
            // 
            // selectPresetPanel
            // 
            resources.ApplyResources(this.presetPanel, "selectPresetPanel");
            this.selectPresetPanel.Controls.Add(this.presetsComboBox);
            this.selectPresetPanel.Controls.Add(this.deletePresetButton);
            this.selectPresetPanel.Name = "presetPanel";
            this.selectPresetPanel.Dock = DockStyle.Top;
            this.selectPresetPanel.Height = 21;
            // 
            // presetPanel
            // 
            resources.ApplyResources(this.presetPanel, "presetPanel");
            this.presetPanel.Controls.Add(this.addPresetButton);
            this.presetPanel.Controls.Add(this.selectPresetPanel);
            this.presetPanel.Name = "presetPanel";
            this.presetPanel.Width = 200;
            this.presetPanel.Dock = DockStyle.Right;
            // 
            // addExoButton
            // 
            resources.ApplyResources(this.addExoButton, "addExoButton");
            this.addExoButton.BackColor = System.Drawing.Color.Black;
            this.addExoButton.FlatAppearance.BorderSize = 0;
            this.addExoButton.ForeColor = System.Drawing.SystemColors.Control;
            this.addExoButton.Name = "addExoButton";
            this.addExoButton.UseVisualStyleBackColor = false;
            this.addExoButton.Click += new System.EventHandler(this.addExoButton_Click);
            this.addExoButton.Dock = DockStyle.Bottom;
            // 
            // presetsComboBox
            // 
            resources.ApplyResources(this.presetsComboBox, "presetsComboBox");
            this.presetsComboBox.BackColor = System.Drawing.Color.Black;
            this.presetsComboBox.ForeColor = System.Drawing.SystemColors.Control;
            this.presetsComboBox.FormattingEnabled = true;
            this.presetsComboBox.Name = "presetsComboBox";
            this.presetsComboBox.SelectedIndexChanged += new System.EventHandler(this.presetsComboBox_SelectedIndexChanged);
            this.presetsComboBox.FlatStyle = FlatStyle.Flat;
            this.presetsComboBox.DropDownStyle = ComboBoxStyle.DropDown;
            this.presetsComboBox.Dock = DockStyle.Left;
            // 
            // deletePresetButton
            // 
            resources.ApplyResources(this.deletePresetButton, "deletePresetButton");
            this.deletePresetButton.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (30)))), ((int) (((byte) (30)))), ((int) (((byte) (30)))));
            this.deletePresetButton.FlatStyle = FlatStyle.Flat;
            this.deletePresetButton.FlatAppearance.BorderSize = 0;
            this.deletePresetButton.ForeColor = System.Drawing.Color.FromArgb(((int) (((byte) (30)))), ((int) (((byte) (30)))), ((int) (((byte) (30)))));
            this.deletePresetButton.Name = "deletePresetButton";
            this.deletePresetButton.UseVisualStyleBackColor = false;
            this.deletePresetButton.Dock = DockStyle.Right;
            this.deletePresetButton.Cursor = Cursors.Hand;
            this.deletePresetButton.Location = new Point(0, 0);
            this.deletePresetButton.Click += new System.EventHandler(this.deletePresetButton_Click);
            // 
            // exoStatComboBox
            // 
            resources.ApplyResources(this.exoStatComboBox, "exoStatComboBox");
            this.exoStatComboBox.BackColor = System.Drawing.Color.Black;
            this.exoStatComboBox.ForeColor = System.Drawing.SystemColors.Control;
            this.exoStatComboBox.FormattingEnabled = true;
            this.exoStatComboBox.Name = "exoStatComboBox";
            this.exoStatComboBox.SelectedIndexChanged += new System.EventHandler(this.exoStatComboBox_SelectedIndexChanged);
            this.exoStatComboBox.Dock = DockStyle.Top;
            this.exoStatComboBox.FlatStyle = FlatStyle.Flat;
            this.exoStatComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            // 
            // clearExosButton
            // 
            resources.ApplyResources(this.clearExosButton, "clearExosButton");
            this.clearExosButton.BackColor = System.Drawing.Color.Black;
            this.clearExosButton.FlatAppearance.BorderSize = 0;
            this.clearExosButton.ForeColor = System.Drawing.SystemColors.Control;
            this.clearExosButton.Name = "clearExosButton";
            this.clearExosButton.UseVisualStyleBackColor = false;
            this.clearExosButton.Location = new Point(133, 25);
            this.clearExosButton.Click += new System.EventHandler(this.clearExosButton_Click);
            // 
            // addPresetButton
            // 
            resources.ApplyResources(this.addPresetButton, "addPresetButton");
            this.addPresetButton.BackColor = System.Drawing.Color.Black;
            this.addPresetButton.FlatAppearance.BorderSize = 0;
            this.addPresetButton.ForeColor = System.Drawing.SystemColors.Control;
            this.addPresetButton.Name = "addPresetButton";
            this.addPresetButton.UseVisualStyleBackColor = false;
            this.addPresetButton.Dock = DockStyle.Bottom;
            this.addPresetButton.Location = new Point(0, 0);
            this.addPresetButton.Click += new System.EventHandler(this.addPresetButton_Click);
            // 
            // StatsForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (30)))), ((int) (((byte) (30)))), ((int) (((byte) (30)))));
            this.Controls.Add(this.statsDataGridView);
            this.Controls.Add(this.actionsPanel);
            this.Name = "StatsForm";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.StatsForm_Closing);
            this.Load += new System.EventHandler(this.StatsForm_Loaded);
            this.VisibleChanged += new System.EventHandler(this.StatsForm_VisibleChanged);
            ((System.ComponentModel.ISupportInitialize) (this.statsDataGridView)).EndInit();
            this.actionsPanel.ResumeLayout(false);
            this.actionsPanel.PerformLayout();
            this.addExoPanel.ResumeLayout(false);
            this.addExoPanel.PerformLayout();
            this.selectPresetPanel.ResumeLayout(false);
            this.selectPresetPanel.PerformLayout();
            this.presetPanel.ResumeLayout(false);
            this.presetPanel.PerformLayout();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Button addPresetButton;
        private System.Windows.Forms.Button deletePresetButton;
        private System.Windows.Forms.Button clearExosButton;

        private Inkybot.Controls.ComboBox presetsComboBox;
        private Inkybot.Controls.ComboBox exoStatComboBox;

        private System.Windows.Forms.Button addExoButton;

        private System.Windows.Forms.Panel actionsPanel;
        private System.Windows.Forms.Panel addExoPanel;
        private System.Windows.Forms.Panel presetPanel;
        private System.Windows.Forms.Panel selectPresetPanel;

        #endregion
        
        private DataGridViewCellStyle exoCellStyle;
        private DataGridViewCellStyle unmageableCellStyle;
        private System.Windows.Forms.DataGridView statsDataGridView;
        private System.Windows.Forms.DataGridViewTextBoxColumn TargetColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn ValueColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn StatColumn;
    }
}