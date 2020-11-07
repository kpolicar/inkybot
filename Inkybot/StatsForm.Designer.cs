using System.ComponentModel;
using System.Windows.Forms;

namespace Inkybot
{
    partial class StatsForm
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

        private void InitializeCustomComponents() {
            exoCellStyle = new DataGridViewCellStyle();
            exoCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            exoCellStyle.BackColor = System.Drawing.Color.FromArgb(20, 20, 20);
            exoCellStyle.Font = new System.Drawing.Font("Calibri", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            exoCellStyle.ForeColor = System.Drawing.SystemColors.Control;
            exoCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(60, 60, 60);
            exoCellStyle.SelectionForeColor = System.Drawing.Color.FromArgb(20, 20, 20);
            exoCellStyle.WrapMode = DataGridViewTriState.True;
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StatsForm));
            this.TargetColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ValueColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.StatColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.addExoButton = new System.Windows.Forms.Button();
            this.exoStatComboBox = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize) (this.dataGridView1)).BeginInit();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // TargetColumn
            // 
            this.TargetColumn.HeaderText = "Target";
            this.TargetColumn.Name = "TargetColumn";
            this.TargetColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // ValueColumn
            // 
            this.ValueColumn.HeaderText = "Value";
            this.ValueColumn.Name = "ValueColumn";
            this.ValueColumn.ReadOnly = true;
            this.ValueColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // StatColumn
            // 
            this.StatColumn.HeaderText = "Stat";
            this.StatColumn.Name = "StatColumn";
            this.StatColumn.ReadOnly = true;
            this.StatColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToResizeColumns = false;
            this.dataGridView1.AllowUserToResizeRows = false;
            this.dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridView1.BackgroundColor = System.Drawing.Color.FromArgb(((int) (((byte) (30)))), ((int) (((byte) (30)))), ((int) (((byte) (30)))));
            this.dataGridView1.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (15)))), ((int) (((byte) (15)))), ((int) (((byte) (15)))));
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Calibri", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.Color.FromArgb(((int) (((byte) (15)))), ((int) (((byte) (15)))), ((int) (((byte) (15)))));
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridView1.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {this.StatColumn, this.ValueColumn, this.TargetColumn});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (30)))), ((int) (((byte) (30)))), ((int) (((byte) (30)))));
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Calibri", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(((int) (((byte) (60)))), ((int) (((byte) (60)))), ((int) (((byte) (60)))));
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridView1.DefaultCellStyle = dataGridViewCellStyle2;
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView1.EnableHeadersVisualStyles = false;
            this.dataGridView1.GridColor = System.Drawing.Color.FromArgb(((int) (((byte) (70)))), ((int) (((byte) (70)))), ((int) (((byte) (70)))));
            this.dataGridView1.Location = new System.Drawing.Point(0, 0);
            this.dataGridView1.MultiSelect = false;
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (64)))), ((int) (((byte) (64)))), ((int) (((byte) (64)))));
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Segoe UI", 9F);
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.FromArgb(((int) (((byte) (15)))), ((int) (((byte) (15)))), ((int) (((byte) (15)))));
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.Color.FromArgb(((int) (((byte) (64)))), ((int) (((byte) (64)))), ((int) (((byte) (64)))));
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridView1.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.dataGridView1.RowHeadersVisible = false;
            this.dataGridView1.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.dataGridView1.Size = new System.Drawing.Size(535, 316);
            this.dataGridView1.TabIndex = 0;
            this.dataGridView1.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.StatsForm_OnChangeValue);
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.addExoButton);
            this.flowLayoutPanel1.Controls.Add(this.exoStatComboBox);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 276);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(535, 40);
            this.flowLayoutPanel1.TabIndex = 2;
            // 
            // addExoButton
            // 
            this.addExoButton.AutoSize = true;
            this.addExoButton.BackColor = System.Drawing.Color.Black;
            this.addExoButton.Enabled = false;
            this.addExoButton.FlatAppearance.BorderSize = 0;
            this.addExoButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.addExoButton.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            this.addExoButton.ForeColor = System.Drawing.SystemColors.Control;
            this.addExoButton.Location = new System.Drawing.Point(0, 0);
            this.addExoButton.Margin = new System.Windows.Forms.Padding(0, 0, 0, 3);
            this.addExoButton.Name = "addExoButton";
            this.addExoButton.Size = new System.Drawing.Size(113, 40);
            this.addExoButton.TabIndex = 2;
            this.addExoButton.Text = "ADD EXO";
            this.addExoButton.UseVisualStyleBackColor = false;
            this.addExoButton.Click += new System.EventHandler(this.addExoButton_Click);
            // 
            // exoStatComboBox
            // 
            this.exoStatComboBox.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (30)))), ((int) (((byte) (30)))), ((int) (((byte) (30)))));
            this.exoStatComboBox.ForeColor = System.Drawing.SystemColors.Control;
            this.exoStatComboBox.FormattingEnabled = true;
            this.exoStatComboBox.Location = new System.Drawing.Point(116, 3);
            this.exoStatComboBox.Name = "exoStatComboBox";
            this.exoStatComboBox.Size = new System.Drawing.Size(159, 21);
            this.exoStatComboBox.TabIndex = 3;
            this.exoStatComboBox.SelectedIndexChanged += new System.EventHandler(this.exoStatComboBox_SelectedIndexChanged);
            // 
            // StatsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (30)))), ((int) (((byte) (30)))), ((int) (((byte) (30)))));
            this.ClientSize = new System.Drawing.Size(535, 316);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.dataGridView1);
            this.Icon = ((System.Drawing.Icon) (resources.GetObject("$this.Icon")));
            this.Name = "StatsForm";
            this.Text = "Stats - Inkybot";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.StatsForm_Closing);
            this.Load += new System.EventHandler(this.StatsForm_Loaded);
            this.VisibleChanged += new System.EventHandler(this.StatsForm_VisibleChanged);
            ((System.ComponentModel.ISupportInitialize) (this.dataGridView1)).EndInit();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.ComboBox exoStatComboBox;

        private System.Windows.Forms.Button addExoButton;

        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;

        #endregion
        
        private DataGridViewCellStyle exoCellStyle;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.DataGridViewTextBoxColumn TargetColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn ValueColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn StatColumn;
    }
}