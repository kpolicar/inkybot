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
            this.MinColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.TargetColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ValueColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.StatColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.PriorityColumn = new Inkybot.Controls.DataGridViewNumericUpDownColumn();
            this.statsDataGridView = new System.Windows.Forms.DataGridView();
            this.actionsPanel = new System.Windows.Forms.Panel();
            this.helpPanel = new System.Windows.Forms.Panel();
            this.mainPanel = new System.Windows.Forms.Panel();
            this.dataGridViewSidebarPanel = new System.Windows.Forms.Panel();
            this.addExoPanel = new System.Windows.Forms.Panel();
            this.presetPanel = new System.Windows.Forms.Panel();
            this.selectPresetPanel = new System.Windows.Forms.Panel();
            this.addExoButton = new System.Windows.Forms.Button();
            this.exoStatComboBox = new Inkybot.Controls.ComboBox();
            this.presetsComboBox = new Inkybot.Controls.ComboBox();
            this.clearExosButton = new System.Windows.Forms.Button();
            this.deletePresetButton = new System.Windows.Forms.Button();
            this.tooltip = new System.Windows.Forms.ToolTip();
            this.refreshButton = new System.Windows.Forms.Button();
            this.showAdvancedOptionsButton = new System.Windows.Forms.Button();
            this.addPresetButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize) (this.statsDataGridView)).BeginInit();
            this.actionsPanel.SuspendLayout();
            this.addExoPanel.SuspendLayout();
            this.presetPanel.SuspendLayout();
            this.selectPresetPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // refreshButtonTooltip
            // 
            tooltip.AutomaticDelay = 50;
            tooltip.AutoPopDelay = int.MaxValue;
            tooltip.SetToolTip(this.showAdvancedOptionsButton, resources.GetString("showAdvancedOptionsButton.ToolTipText"));
            tooltip.SetToolTip(this.refreshButton, resources.GetString("refreshButton.ToolTipText"));
            // 
            // TargetColumn
            // 
            resources.ApplyResources(this.TargetColumn, "TargetColumn");
            this.TargetColumn.Name = "TargetColumn";
            this.TargetColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // MinColumn
            // 
            resources.ApplyResources(this.MinColumn, "MinColumn");
            this.MinColumn.Name = "MinColumn";
            this.MinColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.MinColumn.Visible = false;
            // 
            // ValueColumn
            // 
            resources.ApplyResources(this.ValueColumn, "ValueColumn");
            this.ValueColumn.Name = "ValueColumn";
            this.ValueColumn.ReadOnly = true;
            this.ValueColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // PriorityColumn
            // 
            resources.ApplyResources(this.PriorityColumn, "PriorityColumn");
            this.PriorityColumn.Name = "PriorityColumn";
            this.PriorityColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.PriorityColumn.Visible = false;
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
            this.statsDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {this.StatColumn, this.ValueColumn, this.TargetColumn, this.MinColumn, this.PriorityColumn});
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
            this.statsDataGridView.CellValidating += new System.Windows.Forms.DataGridViewCellValidatingEventHandler(this.StatsForm_OnStatsDataGridViewValidating);
            // 
            // actionsPanel
            // 
            resources.ApplyResources(this.actionsPanel, "actionsPanel");
            this.actionsPanel.Controls.Add(this.addExoPanel);
            this.actionsPanel.Controls.Add(this.presetPanel);
            this.actionsPanel.Controls.Add(this.clearExosButton);
            this.actionsPanel.Name = "actionsPanel";
            //
            // helpPanel
            // 
            resources.ApplyResources(this.helpPanel, "helpPanel");
            this.helpPanel.Controls.Add(this.linkLabel1);
            this.helpPanel.Name = "helpPanel";
            this.helpPanel.Dock = DockStyle.Bottom;
            this.helpPanel.AutoSize = true;
            //
            // mainPanel
            // 
            resources.ApplyResources(this.mainPanel, "mainPanel");
            this.mainPanel.Controls.Add(this.statsDataGridView);
            this.mainPanel.Controls.Add(this.dataGridViewSidebarPanel);
            this.mainPanel.Name = "mainPanel";
            this.mainPanel.Dock = DockStyle.Fill;
            this.mainPanel.AutoSize = true;
            //
            // dataGridViewSidebarPanel
            // 
            resources.ApplyResources(this.dataGridViewSidebarPanel, "dataGridViewSidebarPanel");
            this.dataGridViewSidebarPanel.Controls.Add(this.showAdvancedOptionsButton);
            this.dataGridViewSidebarPanel.Controls.Add(this.refreshButton);
            this.dataGridViewSidebarPanel.Name = "dataGridViewSidebarPanel";
            this.dataGridViewSidebarPanel.Dock = DockStyle.Right;
            this.dataGridViewSidebarPanel.AutoSize = true;
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
            var tooltip2 = new ToolTip();
            tooltip2.SetToolTip(this.deletePresetButton, resources.GetString("deletePresetButton.TooltipText"));
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
            // showAdvancedOptionsButton
            // 
            resources.ApplyResources(this.showAdvancedOptionsButton, "showAdvancedOptionsButton");
            this.showAdvancedOptionsButton.BackColor = System.Drawing.Color.Black;
            this.showAdvancedOptionsButton.FlatAppearance.BorderSize = 0;
            this.showAdvancedOptionsButton.ForeColor = System.Drawing.SystemColors.Control;
            this.showAdvancedOptionsButton.Name = "showAdvancedOptionsButton";
            this.showAdvancedOptionsButton.UseVisualStyleBackColor = false;
            this.showAdvancedOptionsButton.Location = new Point(0, 0);
            this.showAdvancedOptionsButton.Text = "+";
            this.showAdvancedOptionsButton.Padding = System.Windows.Forms.Padding.Empty;
            this.showAdvancedOptionsButton.Margin = System.Windows.Forms.Padding.Empty;
            this.showAdvancedOptionsButton.Click += new System.EventHandler(this.showAdvancedOptionsButton_Click);
            // 
            // refreshButton
            // 
            resources.ApplyResources(this.refreshButton, "refreshButton");
            this.refreshButton.BackColor = System.Drawing.Color.Black;
            this.refreshButton.FlatAppearance.BorderSize = 0;
            this.refreshButton.ForeColor = System.Drawing.SystemColors.Control;
            this.refreshButton.Name = "refreshButton";
            this.refreshButton.UseVisualStyleBackColor = false;
            this.refreshButton.Padding = System.Windows.Forms.Padding.Empty;
            this.refreshButton.Margin = System.Windows.Forms.Padding.Empty;
            this.refreshButton.Click += new System.EventHandler(this.refreshButton_Click);
            this.refreshButton.Paint += new System.Windows.Forms.PaintEventHandler(this.OnRefreshButtonPaint);
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
            // linkLabel1
            // 
            this.linkLabel1.ActiveLinkColor = System.Drawing.SystemColors.ControlLight;
            resources.ApplyResources(this.linkLabel1, "linkLabel1");
            this.linkLabel1.LinkColor = System.Drawing.SystemColors.Control;
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Click += new System.EventHandler(this.linkLabel1_LinkClicked_1);
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Padding = new Padding(0, 5, 0, 5);
            // 
            // StatsForm
            // 
            resources.ApplyResources(this, "$this");
            this.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (30)))), ((int) (((byte) (30)))), ((int) (((byte) (30)))));
            this.Controls.Add(this.mainPanel);
            this.Controls.Add(this.helpPanel);
            this.Controls.Add(this.actionsPanel);
            this.Name = "StatsForm";
            this.Dock = DockStyle.Fill;
            ((System.ComponentModel.ISupportInitialize) (this.statsDataGridView)).EndInit();
            this.helpPanel.ResumeLayout(false);
            this.helpPanel.PerformLayout();
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
        private System.Windows.Forms.Button showAdvancedOptionsButton;
        private System.Windows.Forms.Button refreshButton;
        private System.Windows.Forms.Button deletePresetButton;
        private System.Windows.Forms.Button clearExosButton;

        private Inkybot.Controls.ComboBox presetsComboBox;
        private Inkybot.Controls.ComboBox exoStatComboBox;

        private System.Windows.Forms.ToolTip tooltip;
        private System.Windows.Forms.Button addExoButton;

        private System.Windows.Forms.Panel mainPanel;
        private System.Windows.Forms.Panel dataGridViewSidebarPanel;
        private System.Windows.Forms.Panel helpPanel;
        private System.Windows.Forms.Panel actionsPanel;
        private System.Windows.Forms.Panel addExoPanel;
        private System.Windows.Forms.Panel presetPanel;
        private System.Windows.Forms.Panel selectPresetPanel;
        private System.Windows.Forms.LinkLabel linkLabel1;

        #endregion
        
        private DataGridViewCellStyle exoCellStyle;
        private DataGridViewCellStyle unmageableCellStyle;
        private System.Windows.Forms.DataGridView statsDataGridView;
        private System.Windows.Forms.DataGridViewTextBoxColumn MinColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn TargetColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn ValueColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn StatColumn;
        private Inkybot.Controls.DataGridViewNumericUpDownColumn PriorityColumn;
    }
}