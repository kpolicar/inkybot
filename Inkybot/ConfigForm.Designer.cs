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
            readonlyCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            readonlyCellStyle.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (30)))), ((int) (((byte) (30)))), ((int) (((byte) (30)))));
            readonlyCellStyle.Font = new System.Drawing.Font("Calibri", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            readonlyCellStyle.ForeColor = System.Drawing.SystemColors.ControlDark;
            readonlyCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(((int) (((byte) (60)))), ((int) (((byte) (60)))), ((int) (((byte) (60)))));
            readonlyCellStyle.SelectionForeColor = System.Drawing.SystemColors.ControlDark;
            readonlyCellStyle.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            
            
            modifiedStyle = new DataGridViewCellStyle();
            modifiedStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
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
            this.PaRuneThresholdColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.RaRuneThresholdColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.MaxSmRuneCanHitColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.customScriptLabel = new System.Windows.Forms.Label();
            this.customScriptPathLabel = new System.Windows.Forms.Label();
            this.tooltipLabelExtra = new System.Windows.Forms.Label();
            this.exampleScriptsLinkLabel = new System.Windows.Forms.LinkLabel();
            this.MaxPaRuneCanHitColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.restoreHighSinkStatsCheckbox = new System.Windows.Forms.CheckBox();
            this.autoRestartBotCheckbox = new System.Windows.Forms.CheckBox();
            this.showWarningsCheckbox = new System.Windows.Forms.CheckBox();
            this.publishExosCheckbox = new System.Windows.Forms.CheckBox();
            this.enableRuneCheckingCheckbox = new System.Windows.Forms.CheckBox();
            this.enableKamasCalculationCheckbox = new System.Windows.Forms.CheckBox();
            this.bottomPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.customMagingAIPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.scriptChangeButton = new System.Windows.Forms.Button();
            this.scriptResetButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize) (this.statsDataGridView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize) (this.scriptValidPictureBox)).BeginInit();
            this.bottomPanel.SuspendLayout();
            this.customMagingAIPanel.SuspendLayout();
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
            this.statsDataGridView.CellEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.ConfigForm_OnCellEnter);
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
            // autoRestartBotCheckbox
            // 
            resources.ApplyResources(this.autoRestartBotCheckbox, "autoRestartBotCheckbox");
            this.autoRestartBotCheckbox.ForeColor = System.Drawing.SystemColors.Control;
            this.autoRestartBotCheckbox.Name = "restoreHighSinkStatsCheckbox";
            this.autoRestartBotCheckbox.UseVisualStyleBackColor = true;
            this.autoRestartBotCheckbox.CheckedChanged += new System.EventHandler(this.ConfigForm_OnAutoRestartBotCheckboxCheckedChanged);
            // 
            // showWarningsCheckbox
            // 
            resources.ApplyResources(this.showWarningsCheckbox, "showWarningsCheckbox");
            this.showWarningsCheckbox.ForeColor = System.Drawing.SystemColors.Control;
            this.showWarningsCheckbox.Name = "showWarningsCheckbox";
            this.showWarningsCheckbox.UseVisualStyleBackColor = true;
            this.showWarningsCheckbox.CheckedChanged += new System.EventHandler(this.ConfigForm_OnShowWarningsCheckboxCheckboxCheckedChanged);
            // 
            // publishExosCheckbox
            // 
            resources.ApplyResources(this.publishExosCheckbox, "publishExosCheckbox");
            this.publishExosCheckbox.ForeColor = System.Drawing.SystemColors.Control;
            this.publishExosCheckbox.Name = "publishExosCheckbox";
            this.publishExosCheckbox.UseVisualStyleBackColor = true;
            this.publishExosCheckbox.CheckedChanged += new System.EventHandler(this.ConfigForm_OnPublishExosCheckboxCheckedChanged);
            // 
            // enableRuneCheckingCheckbox
            // 
            resources.ApplyResources(this.enableRuneCheckingCheckbox, "enableRuneCheckingCheckbox");
            this.enableRuneCheckingCheckbox.ForeColor = System.Drawing.SystemColors.Control;
            this.enableRuneCheckingCheckbox.Name = "enableRuneCheckingCheckbox";
            this.enableRuneCheckingCheckbox.UseVisualStyleBackColor = true;
            this.enableRuneCheckingCheckbox.CheckedChanged += new System.EventHandler(this.ConfigForm_OnEnableRuneCheckingCheckboxCheckedChanged);
            // 
            // enableKamasCalculationCheckbox
            // 
            resources.ApplyResources(this.enableKamasCalculationCheckbox, "enableKamasCalculationCheckbox");
            this.enableKamasCalculationCheckbox.ForeColor = System.Drawing.SystemColors.Control;
            this.enableKamasCalculationCheckbox.Name = "enableKamasCalculationCheckbox";
            this.enableKamasCalculationCheckbox.UseVisualStyleBackColor = true;
            this.enableKamasCalculationCheckbox.CheckedChanged += new System.EventHandler(this.ConfigForm_OnEnableKamasCalculationCheckboxCheckboxCheckedChanged);
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
            // bottomPanel
            // 
            resources.ApplyResources(this.bottomPanel, "bottomPanel");
            this.bottomPanel.Controls.Add(this.autoRestartBotCheckbox);
            this.bottomPanel.Controls.Add(this.restoreHighSinkStatsCheckbox);
            this.bottomPanel.Controls.Add(this.showWarningsCheckbox);
            this.bottomPanel.Controls.Add(this.enableRuneCheckingCheckbox);
            this.bottomPanel.Controls.Add(this.publishExosCheckbox);
            this.bottomPanel.Controls.Add(this.enableKamasCalculationCheckbox);
            
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
            this.MinimumSize = new System.Drawing.Size(600, 600);
            this.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (30)))), ((int) (((byte) (30)))), ((int) (((byte) (30)))));
            this.Controls.Add(this.statsDataGridView);
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

        private System.Windows.Forms.FlowLayoutPanel customMagingAIPanel;
        private System.Windows.Forms.FlowLayoutPanel bottomPanel;
        private System.Windows.Forms.Button scriptChangeButton;
        private System.Windows.Forms.Button scriptResetButton;

        private System.Windows.Forms.CheckBox autoRestartBotCheckbox;
        private System.Windows.Forms.CheckBox showWarningsCheckbox;
        private System.Windows.Forms.CheckBox publishExosCheckbox;
        private System.Windows.Forms.CheckBox enableRuneCheckingCheckbox;
        private System.Windows.Forms.CheckBox restoreHighSinkStatsCheckbox;
        private System.Windows.Forms.CheckBox enableKamasCalculationCheckbox;

        private System.Windows.Forms.DataGridViewTextBoxColumn MaxPaRuneCanHitColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn MaxSmRuneCanHitColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn PaRuneThresholdColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn RaRuneThresholdColumn;
        private System.Windows.Forms.Label customScriptLabel;
        private System.Windows.Forms.Label customScriptPathLabel;
        private System.Windows.Forms.Label tooltipLabelExtra;
        private System.Windows.Forms.LinkLabel exampleScriptsLinkLabel;

        private System.Windows.Forms.DataGridView statsDataGridView;
        private System.Windows.Forms.PictureBox scriptValidPictureBox;

        private System.Windows.Forms.DataGridViewTextBoxColumn StatColumn;
        private System.Windows.Forms.OpenFileDialog scriptFileDialog;
        private DataGridViewCellStyle modifiedStyle;

        #endregion
    }
}

