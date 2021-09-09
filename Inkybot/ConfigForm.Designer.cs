using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Inkybot
{
    partial class ConfigForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;
        System.ComponentModel.ComponentResourceManager resources;

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
        
        private Pen borderPen = new Pen(Color.Black);
        private DataGridViewCellStyle readonlyCellStyle;
        private void InitializeCustomComponents() {
            readonlyCellStyle = new DataGridViewCellStyle();
            readonlyCellStyle.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (30)))), ((int) (((byte) (30)))), ((int) (((byte) (30)))));
            readonlyCellStyle.Font = new System.Drawing.Font("Calibri", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            readonlyCellStyle.ForeColor = System.Drawing.SystemColors.ControlDark;
            readonlyCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(((int) (((byte) (60)))), ((int) (((byte) (60)))), ((int) (((byte) (60)))));
            readonlyCellStyle.SelectionForeColor = System.Drawing.SystemColors.ControlDark;
            readonlyCellStyle.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            
            
            modifiedStyle = new DataGridViewCellStyle();
            modifiedStyle.BackColor = System.Drawing.Color.FromArgb(20, 20, 20);
            modifiedStyle.Font = new System.Drawing.Font("Calibri", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            modifiedStyle.ForeColor = System.Drawing.SystemColors.Control;
            modifiedStyle.SelectionBackColor = System.Drawing.Color.FromArgb(60, 60, 60);
            modifiedStyle.SelectionForeColor = System.Drawing.Color.FromArgb(20, 20, 20);
            modifiedStyle.WrapMode = DataGridViewTriState.True;
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            resources = new System.ComponentModel.ComponentResourceManager(typeof(ConfigForm));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.scriptValidPictureBox = new System.Windows.Forms.PictureBox();
            this.scriptFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.statsDataGridView = new System.Windows.Forms.DataGridView();
            statsDataGridView.ShowCellToolTips = true;
            this.StatColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.UseSmRunesColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.UsePaRunesColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.UseRaRunesColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.PaRuneThresholdColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.RaRuneThresholdColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.MaxSmRuneCanHitColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.customScriptLabel = new System.Windows.Forms.Label();
            this.automaticShutdownLabel = new System.Windows.Forms.Label();
            this.customScriptPathLabel = new System.Windows.Forms.Label();
            this.tooltipLabelExtra = new System.Windows.Forms.Label();
            this.exampleScriptsLinkLabel = new System.Windows.Forms.LinkLabel();
            this.MaxPaRuneCanHitColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.restoreHighSinkStatsCheckbox = new System.Windows.Forms.CheckBox();
            this.autoRestartBotCheckbox = new System.Windows.Forms.CheckBox();
            this.showWarningsCheckbox = new System.Windows.Forms.CheckBox();
            this.publishExosCheckbox = new System.Windows.Forms.CheckBox();
            this.enableMageQueueingCheckbox = new System.Windows.Forms.CheckBox();
            this.autoShutdownComboBox = new Inkybot.Controls.ComboBox();
            this.customResizeRatioNumericUpDownLabel = new System.Windows.Forms.Label();
            this.customResizeRatioNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.mainPanel = new System.Windows.Forms.Panel();
            this.automaticShutdownPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.customResizeRatioPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.dataGridViewSidebarPanel = new System.Windows.Forms.Panel();
            this.enableRuneCheckingCheckbox = new System.Windows.Forms.CheckBox();
            this.enableKamasCalculationCheckbox = new System.Windows.Forms.CheckBox();
            this.bottomPanel = new System.Windows.Forms.TableLayoutPanel();
            this.customMagingAIPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.scriptChangeButton = new System.Windows.Forms.Button();
            this.scriptResetButton = new System.Windows.Forms.Button();
            this.showAdvancedOptionsButton = new System.Windows.Forms.Button();
            this.tooltip = new System.Windows.Forms.ToolTip();
            ((System.ComponentModel.ISupportInitialize) (this.statsDataGridView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize) (this.scriptValidPictureBox)).BeginInit();
            this.bottomPanel.SuspendLayout();
            this.customMagingAIPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // refreshButtonTooltip
            // 
            tooltip.AutomaticDelay = 50;
            tooltip.AutoPopDelay = int.MaxValue;
            tooltip.SetToolTip(this.showAdvancedOptionsButton, resources.GetString("showAdvancedOptionsButton.ToolTipText"));
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
            this.statsDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {this.StatColumn, this.UseSmRunesColumn, this.UsePaRunesColumn, this.UseRaRunesColumn, this.PaRuneThresholdColumn, this.RaRuneThresholdColumn, this.MaxSmRuneCanHitColumn, this.MaxPaRuneCanHitColumn});
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
            this.statsDataGridView.CellEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.ConfigForm_OnCellEnter);
            this.statsDataGridView.CellValidating += new System.Windows.Forms.DataGridViewCellValidatingEventHandler(this.ConfigForm_OnStatsDataGridViewValidating);
            // 
            // StatColumn
            // 
            resources.ApplyResources(this.StatColumn, "StatColumn");
            this.StatColumn.Name = "StatColumn";
            this.StatColumn.ReadOnly = true;
            // 
            // UseSmRunesColumn
            // 
            resources.ApplyResources(this.UseSmRunesColumn, "UseSmRunesColumn");
            this.UseSmRunesColumn.Name = "UseSmRunesColumn";
            // 
            // UsePaRunesColumn
            // 
            resources.ApplyResources(this.UsePaRunesColumn, "UsePaRunesColumn");
            this.UsePaRunesColumn.Name = "UsePaRunesColumn";
            // 
            // UseRaRunesColumn
            // 
            resources.ApplyResources(this.UseRaRunesColumn, "UseRaRunesColumn");
            this.UseRaRunesColumn.Name = "UseRaRunesColumn";
            // 
            // PaRuneThresholdColumn
            // 
            resources.ApplyResources(this.PaRuneThresholdColumn, "PaRuneThresholdColumn");
            this.PaRuneThresholdColumn.Name = "PaRuneThresholdColumn";
            this.PaRuneThresholdColumn.Visible = false;
            // 
            // RaRuneThresholdColumn
            // 
            resources.ApplyResources(this.RaRuneThresholdColumn, "RaRuneThresholdColumn");
            this.RaRuneThresholdColumn.Name = "RaRuneThresholdColumn";
            this.RaRuneThresholdColumn.Visible = false;
            // 
            // MaxSmRuneCanHitColumn
            // 
            resources.ApplyResources(this.MaxSmRuneCanHitColumn, "MaxSmRuneCanHitColumn");
            this.MaxSmRuneCanHitColumn.Name = "MaxSmRuneCanHitColumn";
            this.MaxSmRuneCanHitColumn.Visible = false;
            // 
            // MaxPaRuneCanHitColumn
            // 
            resources.ApplyResources(this.MaxPaRuneCanHitColumn, "MaxPaRuneCanHitColumn");
            this.MaxPaRuneCanHitColumn.Name = "MaxPaRuneCanHitColumn";
            this.MaxPaRuneCanHitColumn.Visible = false;
            // 
            // restoreHighSinkStatsCheckbox
            // 
            resources.ApplyResources(this.restoreHighSinkStatsCheckbox, "restoreHighSinkStatsCheckbox");
            this.restoreHighSinkStatsCheckbox.ForeColor = System.Drawing.SystemColors.Control;
            this.restoreHighSinkStatsCheckbox.Name = "restoreHighSinkStatsCheckbox";
            this.restoreHighSinkStatsCheckbox.UseVisualStyleBackColor = true;
            this.restoreHighSinkStatsCheckbox.AutoCheck = false;
            this.restoreHighSinkStatsCheckbox.CheckedChanged += new System.EventHandler(this.ConfigForm_OnRestoreHighSinkStatsCheckboxCheckedChanged);
            this.restoreHighSinkStatsCheckbox.Dock = DockStyle.Fill;
            // 
            // autoRestartBotCheckbox
            // 
            resources.ApplyResources(this.autoRestartBotCheckbox, "autoRestartBotCheckbox");
            this.autoRestartBotCheckbox.ForeColor = System.Drawing.SystemColors.Control;
            this.autoRestartBotCheckbox.Name = "restoreHighSinkStatsCheckbox";
            this.autoRestartBotCheckbox.UseVisualStyleBackColor = true;
            this.autoRestartBotCheckbox.CheckedChanged += new System.EventHandler(this.ConfigForm_OnAutoRestartBotCheckboxCheckedChanged);
            this.autoRestartBotCheckbox.Dock = DockStyle.Fill;
            // 
            // showWarningsCheckbox
            // 
            resources.ApplyResources(this.showWarningsCheckbox, "showWarningsCheckbox");
            this.showWarningsCheckbox.ForeColor = System.Drawing.SystemColors.Control;
            this.showWarningsCheckbox.Name = "showWarningsCheckbox";
            this.showWarningsCheckbox.UseVisualStyleBackColor = true;
            this.showWarningsCheckbox.CheckedChanged += new System.EventHandler(this.ConfigForm_OnShowWarningsCheckboxCheckboxCheckedChanged);
            this.showWarningsCheckbox.Dock = DockStyle.Fill;
            // 
            // publishExosCheckbox
            // 
            resources.ApplyResources(this.publishExosCheckbox, "publishExosCheckbox");
            this.publishExosCheckbox.ForeColor = System.Drawing.SystemColors.Control;
            this.publishExosCheckbox.Name = "publishExosCheckbox";
            this.publishExosCheckbox.UseVisualStyleBackColor = true;
            this.publishExosCheckbox.CheckedChanged += new System.EventHandler(this.ConfigForm_OnPublishExosCheckboxCheckedChanged);
            this.publishExosCheckbox.Dock = DockStyle.Fill;
            // 
            // autoShutdownComboBox
            // 
            resources.ApplyResources(this.autoShutdownComboBox, "autoShutdownComboBox");
            this.autoShutdownComboBox.BackColor = System.Drawing.Color.Black;
            this.autoShutdownComboBox.ForeColor = System.Drawing.SystemColors.Control;
            this.autoShutdownComboBox.FormattingEnabled = true;
            this.autoShutdownComboBox.Name = "autoShutdownComboBox";
            this.autoShutdownComboBox.SelectedIndexChanged += new System.EventHandler(this.autoShutdownComboBox_SelectedIndexChanged);
            this.autoShutdownComboBox.FlatStyle = FlatStyle.Flat;
            this.autoShutdownComboBox.DropDownStyle = ComboBoxStyle.DropDown;
            
            this.autoShutdownComboBox.DataSource = new BindingSource(new Dictionary<int, string>() {
                {-1, resources.GetString("autoShutdownComboBox.OptionDisabled")!},
                {59*1000, resources.GetString("autoShutdownComboBox.OptionMinute")!},
                {60*2*1000, resources.GetString("autoShutdownComboBox.OptionMinutes")!.Replace(":value", "2")},
                {60*3*1000, resources.GetString("autoShutdownComboBox.OptionMinutes")!.Replace(":value", "3")},
                {60*5*1000, resources.GetString("autoShutdownComboBox.OptionMinutes")!.Replace(":value", "5")},
                {60*10*1000, resources.GetString("autoShutdownComboBox.OptionMinutes")!.Replace(":value", "10")},
                {60*15*1000, resources.GetString("autoShutdownComboBox.OptionMinutes")!.Replace(":value", "15")},
            }, null);
            this.autoShutdownComboBox.ValueMember = "Key";
            this.autoShutdownComboBox.DisplayMember = "Value";
            this.autoShutdownComboBox.TabIndex = 3;

            // 
            // enableRuneCheckingCheckbox
            // 
            resources.ApplyResources(this.enableRuneCheckingCheckbox, "enableRuneCheckingCheckbox");
            this.enableRuneCheckingCheckbox.ForeColor = System.Drawing.SystemColors.Control;
            this.enableRuneCheckingCheckbox.Name = "enableRuneCheckingCheckbox";
            this.enableRuneCheckingCheckbox.UseVisualStyleBackColor = true;
            this.enableRuneCheckingCheckbox.CheckedChanged += new System.EventHandler(this.ConfigForm_OnEnableRuneCheckingCheckboxCheckedChanged);
            this.enableRuneCheckingCheckbox.Dock = DockStyle.Fill;

            // 
            // enableKamasCalculationCheckbox
            // 
            resources.ApplyResources(this.enableKamasCalculationCheckbox, "enableKamasCalculationCheckbox");
            this.enableKamasCalculationCheckbox.ForeColor = System.Drawing.SystemColors.Control;
            this.enableKamasCalculationCheckbox.Name = "enableKamasCalculationCheckbox";
            this.enableKamasCalculationCheckbox.UseVisualStyleBackColor = true;
            this.enableKamasCalculationCheckbox.CheckedChanged += new System.EventHandler(this.ConfigForm_OnEnableKamasCalculationCheckboxCheckboxCheckedChanged);
            this.enableKamasCalculationCheckbox.Dock = DockStyle.Fill;
            // 
            // enableMageQueueingCheckbox
            // 
            resources.ApplyResources(this.enableMageQueueingCheckbox, "enableMageQueueingCheckbox");
            this.enableMageQueueingCheckbox.ForeColor = System.Drawing.SystemColors.Control;
            this.enableMageQueueingCheckbox.Name = "enableMageQueueingCheckbox";
            this.enableMageQueueingCheckbox.UseVisualStyleBackColor = true;
            this.enableMageQueueingCheckbox.CheckedChanged += new System.EventHandler(this.ConfigForm_OnEnableMageQueueingCheckboxCheckboxCheckedChanged);
            this.enableMageQueueingCheckbox.Dock = DockStyle.Fill;

            // 
            // exampleScriptsLinkLabel
            // 
            this.exampleScriptsLinkLabel.ActiveLinkColor = System.Drawing.SystemColors.ControlLight;
            resources.ApplyResources(this.exampleScriptsLinkLabel, "exampleScriptsLinkLabel");
            this.exampleScriptsLinkLabel.LinkColor = System.Drawing.SystemColors.Control;
            this.exampleScriptsLinkLabel.Name = "exampleScriptsLinkLabel";
            this.exampleScriptsLinkLabel.TabStop = true;
            this.exampleScriptsLinkLabel.Click += new System.EventHandler(this.exampleScriptsLinkLabel_OnClick);
            this.exampleScriptsLinkLabel.AutoSize = true;
            this.exampleScriptsLinkLabel.Margin = new Padding(0, 5, 0, 2);
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
            // automaticShutdownPanel
            // 
            resources.ApplyResources(this.automaticShutdownPanel, "automaticShutdownPanel");
            this.automaticShutdownPanel.Controls.Add(this.automaticShutdownLabel);
            this.automaticShutdownPanel.Controls.Add(this.autoShutdownComboBox);
            this.automaticShutdownPanel.Name = "automaticShutdownPanel";
            this.automaticShutdownPanel.Dock = DockStyle.Fill;
            this.automaticShutdownPanel.AutoSize = true;
            //
            // customResizeRatioPanel
            // 
            resources.ApplyResources(this.customResizeRatioPanel, "customResizeRatioPanel");
            this.customResizeRatioPanel.Controls.Add(this.customResizeRatioNumericUpDownLabel);
            this.customResizeRatioPanel.Controls.Add(this.customResizeRatioNumericUpDown);
            this.customResizeRatioPanel.Name = "customResizeRatioPanel";
            this.customResizeRatioPanel.Dock = DockStyle.Fill;
            this.customResizeRatioPanel.AutoSize = true;
            //
            // dataGridViewSidebarPanel
            // 
            resources.ApplyResources(this.dataGridViewSidebarPanel, "dataGridViewSidebarPanel");
            this.dataGridViewSidebarPanel.Controls.Add(this.showAdvancedOptionsButton);
            this.dataGridViewSidebarPanel.Name = "dataGridViewSidebarPanel";
            this.dataGridViewSidebarPanel.Dock = DockStyle.Right;
            this.dataGridViewSidebarPanel.AutoSize = true;
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
            // bottomPanel
            // 
            resources.ApplyResources(this.bottomPanel, "bottomPanel");
            this.bottomPanel.ColumnCount = 2;
            this.bottomPanel.RowCount = 4;
            this.bottomPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            this.bottomPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            this.bottomPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            this.bottomPanel.Controls.Add(this.restoreHighSinkStatsCheckbox);
            this.bottomPanel.Controls.Add(this.publishExosCheckbox);
            this.bottomPanel.Controls.Add(this.showWarningsCheckbox);
            this.bottomPanel.Controls.Add(this.enableKamasCalculationCheckbox);
            this.bottomPanel.Controls.Add(this.autoRestartBotCheckbox);
            this.bottomPanel.Controls.Add(this.enableRuneCheckingCheckbox);
            this.bottomPanel.Controls.Add(this.enableMageQueueingCheckbox);
            this.bottomPanel.Controls.Add(new Panel() {Size = Size.Empty });
            this.bottomPanel.Controls.Add(this.automaticShutdownPanel);
            this.bottomPanel.Controls.Add(this.customResizeRatioPanel);

            this.customMagingAIPanel.Controls.Add(this.exampleScriptsLinkLabel);
            this.customMagingAIPanel.SetFlowBreak(this.exampleScriptsLinkLabel, true);
            
            this.customMagingAIPanel.Controls.Add(this.customScriptLabel);
            this.customMagingAIPanel.Controls.Add(this.scriptChangeButton);
            this.customMagingAIPanel.Controls.Add(this.scriptResetButton);
            this.customMagingAIPanel.Controls.Add(this.customScriptPathLabel);
            this.customMagingAIPanel.Controls.Add(this.scriptValidPictureBox);
            this.customMagingAIPanel.Paint += (sender, e) => {
                e.Graphics.DrawLine(borderPen, 0, 0, this.customMagingAIPanel.ClientRectangle.Width, 1);
            };
            this.bottomPanel.Name = "bottomPanel";
            this.bottomPanel.AutoSize = true;
            this.customMagingAIPanel.Name = "customMagingAIPanel";
            this.customMagingAIPanel.AutoSize = true;
            this.customMagingAIPanel.Dock = DockStyle.Bottom;
            // 
            // tooltipLabelExtra
            // 
            resources.ApplyResources(this.tooltipLabelExtra, "tooltipLabelExtra");
            this.tooltipLabelExtra.Name = "tooltipLabelExtra";
            this.tooltipLabelExtra.AutoSize = true;
            this.tooltipLabelExtra.ForeColor = System.Drawing.SystemColors.Control;
            this.tooltipLabelExtra.AutoSize = true;
            this.tooltipLabelExtra.Dock = DockStyle.Top;
            this.tooltipLabelExtra.Height = 20;
            // 
            // customScriptLabel
            // 
            resources.ApplyResources(this.customScriptLabel, "customScriptLabel");
            this.customScriptLabel.Name = "customScriptLabel";
            this.customScriptLabel.AutoSize = true;
            this.customScriptLabel.ForeColor = System.Drawing.SystemColors.Control;
            this.customScriptLabel.Margin = new Padding(0, 7, 0, 0);
            // 
            // automaticShutdownLabel
            // 
            resources.ApplyResources(this.automaticShutdownLabel, "automaticShutdownLabel");
            this.automaticShutdownLabel.Name = "automaticShutdownLabel";
            this.automaticShutdownLabel.ForeColor = System.Drawing.SystemColors.Control;
            this.automaticShutdownLabel.Margin = new Padding(0, 6, 0, 0);
            this.automaticShutdownLabel.AutoSize = true;
            // 
            // customResizeRatioNumericUpDown
            // 
            this.customResizeRatioNumericUpDown.BackColor = Color.Black;
            this.customResizeRatioNumericUpDown.ForeColor = System.Drawing.SystemColors.Control;
            this.customResizeRatioNumericUpDown.Minimum = 0;
            this.customResizeRatioNumericUpDown.Maximum = 4;
            this.customResizeRatioNumericUpDown.Increment = new decimal(0.01d);
            this.customResizeRatioNumericUpDown.DecimalPlaces = 2;
            this.customResizeRatioNumericUpDown.Value = 1;
            this.customResizeRatioNumericUpDown.Width = 50;
            this.customResizeRatioNumericUpDown.ValueChanged += new EventHandler(customResizeRatioNumericUpDown_ValueChanged);
            // 
            // customResizeRatioNumericUpDownLabel
            // 
            resources.ApplyResources(this.customResizeRatioNumericUpDownLabel, "customResizeRatioNumericUpDownLabel");
            this.customResizeRatioNumericUpDownLabel.Name = "customResizeRatioNumericUpDownLabel";
            this.customResizeRatioNumericUpDownLabel.ForeColor = System.Drawing.SystemColors.Control;
            this.customResizeRatioNumericUpDownLabel.Margin = new Padding(0, 3, 10, 0);
            this.customResizeRatioNumericUpDownLabel.AutoSize = true;
            // 
            // scriptChangeButton
            // 
            resources.ApplyResources(this.scriptChangeButton, "scriptChangeButton");
            this.scriptChangeButton.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (15)))), ((int) (((byte) (15)))), ((int) (((byte) (15)))));
            this.scriptChangeButton.ForeColor = System.Drawing.SystemColors.Control;
            this.scriptChangeButton.Name = "scriptChangeButton";
            this.scriptChangeButton.UseVisualStyleBackColor = false;
            this.scriptChangeButton.Click += new System.EventHandler(this.scriptChangeButton_Click);
            // 
            // scriptResetButton
            // 
            resources.ApplyResources(this.scriptResetButton, "scriptResetButton");
            this.scriptResetButton.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (15)))), ((int) (((byte) (15)))), ((int) (((byte) (15)))));
            this.scriptResetButton.ForeColor = System.Drawing.SystemColors.Control;
            this.scriptResetButton.Name = "scriptResetButton";
            this.scriptResetButton.UseVisualStyleBackColor = false;
            this.scriptResetButton.Visible = false;
            this.scriptResetButton.Click += new System.EventHandler(this.scriptResetButton_Click);
            // 
            // customScriptPathLabel
            // 
            resources.ApplyResources(this.customScriptPathLabel, "customScriptPathLabel");
            this.customScriptPathLabel.Name = "customScriptLabel";
            this.customScriptPathLabel.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.customScriptPathLabel.AutoSize = true;
            this.customScriptPathLabel.Margin = new Padding(0, 7, 0, 0);
            // 
            // toastIconPictureBox
            // 
            resources.ApplyResources(this.scriptValidPictureBox, "scriptValidPictureBox");
            this.scriptValidPictureBox.Name = "scriptValidPictureBox";
            this.scriptValidPictureBox.TabStop = false;
            this.scriptValidPictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            this.scriptValidPictureBox.Size = new System.Drawing.Size(20, 20);
            this.scriptValidPictureBox.Visible = false;
            
            // 
            // scriptFileDialog
            // 
            this.scriptFileDialog.DefaultExt = "cs";
            this.scriptFileDialog.Filter = "C# Script (*.cs)|*.cs";
            // 
            // ConfigForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.MinimumSize = new System.Drawing.Size(400, 400);
            this.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (30)))), ((int) (((byte) (30)))), ((int) (((byte) (30)))));
            this.Controls.Add(this.mainPanel);
            this.Controls.Add(this.tooltipLabelExtra);
            this.Controls.Add(this.bottomPanel);
            this.Controls.Add(this.customMagingAIPanel);
            this.Name = "ConfigForm";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.ConfigForm_Closing);
            this.Load += new System.EventHandler(this.ConfigForm_OnLoad);
            ((System.ComponentModel.ISupportInitialize) (this.statsDataGridView)).EndInit();
            ((System.ComponentModel.ISupportInitialize) (this.scriptValidPictureBox)).EndInit();
            this.customMagingAIPanel.ResumeLayout(false);
            this.customMagingAIPanel.PerformLayout();
            this.bottomPanel.ResumeLayout(false);
            this.bottomPanel.PerformLayout();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Button showAdvancedOptionsButton;
        private System.Windows.Forms.Panel mainPanel;
        private System.Windows.Forms.FlowLayoutPanel automaticShutdownPanel;
        private System.Windows.Forms.FlowLayoutPanel customResizeRatioPanel;
        private System.Windows.Forms.Panel dataGridViewSidebarPanel;
        private System.Windows.Forms.FlowLayoutPanel customMagingAIPanel;
        private System.Windows.Forms.TableLayoutPanel bottomPanel;
        private System.Windows.Forms.Button scriptChangeButton;
        private System.Windows.Forms.Button scriptResetButton;

        private System.Windows.Forms.CheckBox autoRestartBotCheckbox;
        private System.Windows.Forms.CheckBox showWarningsCheckbox;
        private System.Windows.Forms.CheckBox publishExosCheckbox;
        private System.Windows.Forms.CheckBox enableRuneCheckingCheckbox;
        private System.Windows.Forms.CheckBox enableMageQueueingCheckbox;
        private System.Windows.Forms.CheckBox restoreHighSinkStatsCheckbox;
        private System.Windows.Forms.CheckBox enableKamasCalculationCheckbox;
        private Inkybot.Controls.ComboBox autoShutdownComboBox;
        private System.Windows.Forms.ToolTip tooltip;

        private System.Windows.Forms.DataGridViewCheckBoxColumn UseSmRunesColumn;
        private System.Windows.Forms.DataGridViewCheckBoxColumn UsePaRunesColumn;
        private System.Windows.Forms.DataGridViewCheckBoxColumn UseRaRunesColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn MaxPaRuneCanHitColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn MaxSmRuneCanHitColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn PaRuneThresholdColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn RaRuneThresholdColumn;
        private System.Windows.Forms.Label customScriptLabel;
        private System.Windows.Forms.Label customScriptPathLabel;
        private System.Windows.Forms.Label automaticShutdownLabel;
        private System.Windows.Forms.Label tooltipLabelExtra;
        private System.Windows.Forms.LinkLabel exampleScriptsLinkLabel;
        private System.Windows.Forms.Label customResizeRatioNumericUpDownLabel;
        private System.Windows.Forms.NumericUpDown customResizeRatioNumericUpDown;

        private System.Windows.Forms.DataGridView statsDataGridView;
        private System.Windows.Forms.PictureBox scriptValidPictureBox;

        private System.Windows.Forms.DataGridViewTextBoxColumn StatColumn;
        private System.Windows.Forms.OpenFileDialog scriptFileDialog;
        private DataGridViewCellStyle modifiedStyle;

        #endregion
    }
}

