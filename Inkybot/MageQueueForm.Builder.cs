using System.ComponentModel;
using System.Drawing;
using System.Linq;
using Inkybot.Controls;
using Inkybot.Events;
using Inkybot.Resources;
using Inkybot.Services;

namespace Inkybot
{
    public partial class MageQueueForm
    {
        private ComponentResourceManager resources;

        private GroupBox BuildMageQueueGroupBox(MageQueueManager.MageQueueItem mageQueueItem)
        {
            System.Windows.Forms.Panel previewPanel;
            System.Windows.Forms.FlowLayoutPanel buttonsPanel;
            System.Windows.Forms.Button moveUpButton;
            System.Windows.Forms.Button moveDownButton;
            System.Windows.Forms.Button removeButton;
            System.Windows.Forms.Label configPresetLabel;
            System.Windows.Forms.Panel configPresetPanel;
            Inkybot.Controls.ComboBox statPresetComboBox;
            Inkybot.Controls.ComboBox configPresetComboBox;
            System.Windows.Forms.Label statPresetLabel;
            System.Windows.Forms.Panel statPresetPanel;
            System.Windows.Forms.PictureBox previewPictureBox;
            Inkybot.Controls.GroupBox queueItemGroupBox;
        
            queueItemGroupBox = new Inkybot.Controls.GroupBox();
            buttonsPanel = new System.Windows.Forms.FlowLayoutPanel();
            moveUpButton = new System.Windows.Forms.Button();
            moveDownButton = new System.Windows.Forms.Button();
            removeButton = new System.Windows.Forms.Button();
            configPresetPanel = new System.Windows.Forms.Panel();
            configPresetComboBox = new Inkybot.Controls.ComboBox();
            configPresetLabel = new System.Windows.Forms.Label();
            statPresetPanel = new System.Windows.Forms.Panel();
            statPresetComboBox = new Inkybot.Controls.ComboBox();
            statPresetLabel = new System.Windows.Forms.Label();
            previewPanel = new System.Windows.Forms.Panel();
            previewPictureBox = new System.Windows.Forms.PictureBox();
            queueItemGroupBox.SuspendLayout();
            buttonsPanel.SuspendLayout();
            configPresetPanel.SuspendLayout();
            statPresetPanel.SuspendLayout();
            previewPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize) (previewPictureBox)).BeginInit();
            // 
            // queueItem
            // 
            queueItemGroupBox.Controls.Add(buttonsPanel);
            queueItemGroupBox.Controls.Add(configPresetPanel);
            queueItemGroupBox.Controls.Add(statPresetPanel);
            queueItemGroupBox.Controls.Add(previewPanel);
            queueItemGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
            queueItemGroupBox.Location = new System.Drawing.Point(0, 0);
            queueItemGroupBox.Name = "queueItem";
            queueItemGroupBox.Size = new System.Drawing.Size(642, 120);
            queueItemGroupBox.BorderColor = System.Drawing.Color.FromArgb(((int) (((byte) (70)))), ((int) (((byte) (70)))), ((int) (((byte) (70)))));
            queueItemGroupBox.TabIndex = 0;
            queueItemGroupBox.TabStop = false;
            // 
            // buttonsPanel
            // 
            buttonsPanel.Controls.Add(moveUpButton);
            buttonsPanel.Controls.Add(moveDownButton);
            buttonsPanel.Controls.Add(removeButton);
            buttonsPanel.Dock = System.Windows.Forms.DockStyle.Right;
            buttonsPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            buttonsPanel.Location = new System.Drawing.Point(536, 14);
            buttonsPanel.Name = "buttonsPanel";
            buttonsPanel.Padding = new System.Windows.Forms.Padding(2, 0, 0, 0);
            buttonsPanel.Size = new System.Drawing.Size(103, 99);
            buttonsPanel.TabIndex = 7;
            // 
            // moveUpButton
            // 
            moveUpButton.AutoSize = true;
            moveUpButton.BackColor = System.Drawing.Color.Black;
            moveUpButton.FlatAppearance.BorderSize = 0;
            moveUpButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            moveUpButton.ForeColor = System.Drawing.SystemColors.Control;
            moveUpButton.Location = new System.Drawing.Point(0,0);
            moveUpButton.Name = "moveUpButton";
            moveUpButton.Size = new System.Drawing.Size(93, 26);
            moveUpButton.TabIndex = 5;
            moveUpButton.Text = resources.GetString("moveUpButton.Text");
            moveUpButton.UseVisualStyleBackColor = false;
            moveUpButton.Click += (sender, _) => OnMageQueueItemMoveUp(sender, new MageQueueEventArgs(mageQueueItem));
            // 
            // moveDownButton
            // 
            moveDownButton.AutoSize = true;
            moveDownButton.BackColor = System.Drawing.Color.Black;
            moveDownButton.FlatAppearance.BorderSize = 0;
            moveDownButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            moveDownButton.ForeColor = System.Drawing.SystemColors.Control;
            moveDownButton.Location = new System.Drawing.Point(0,0);
            moveDownButton.Name = "moveDownButton";
            moveDownButton.Size = new System.Drawing.Size(93, 26);
            moveDownButton.TabIndex = 6;
            moveDownButton.Text = resources.GetString("moveDownButton.Text");
            moveDownButton.UseVisualStyleBackColor = false;
            moveDownButton.Click += (sender, _) => OnMageQueueItemMoveDown(sender, new MageQueueEventArgs(mageQueueItem));
            // 
            // removeButton
            // 
            removeButton.AutoSize = true;
            removeButton.BackColor = System.Drawing.Color.Black;
            removeButton.FlatAppearance.BorderSize = 0;
            removeButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            removeButton.ForeColor = System.Drawing.SystemColors.Control;
            removeButton.Location = new System.Drawing.Point(0,0);
            removeButton.Name = "removeButton";
            removeButton.Size = new System.Drawing.Size(93, 26);
            removeButton.TabIndex = 4;
            removeButton.Text = resources.GetString("removeButton.Text");
            removeButton.UseVisualStyleBackColor = false;
            removeButton.Click += (sender, _) => OnMageQueueItemRemove(sender, new MageQueueEventArgs(mageQueueItem));
            // 
            // configPresetPanel
            // 
            configPresetPanel.Controls.Add(configPresetComboBox);
            configPresetPanel.Controls.Add(configPresetLabel);
            configPresetPanel.Dock = System.Windows.Forms.DockStyle.Left;
            configPresetPanel.Location = new System.Drawing.Point(303, 18);
            configPresetPanel.Name = "configPresetPanel";
            configPresetPanel.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            configPresetPanel.Size = new System.Drawing.Size(240, 99);
            configPresetPanel.TabIndex = 2;
            // 
            // configPresetComboBox
            // 
            configPresetComboBox.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            configPresetComboBox.BackColor = System.Drawing.Color.Black;
            configPresetComboBox.ForeColor = System.Drawing.SystemColors.Control;
            configPresetComboBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            configPresetComboBox.FormattingEnabled = true;
            configPresetComboBox.Location = new System.Drawing.Point(10, 26);
            configPresetComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            configPresetComboBox.Name = "configPresetComboBox";
            configPresetComboBox.Size = new System.Drawing.Size(180, 24);
            configPresetComboBox.TabIndex = 1;
            configPresetComboBox.SelectedIndexChanged += (sender, _) =>
                OnMageQueueConfigPresetSelectedIndexChanged(sender, new MageQueueEventArgs(mageQueueItem), configPresetComboBox.SelectedIndex);
            // 
            // configPresetLabel
            // 
            configPresetLabel.Dock = System.Windows.Forms.DockStyle.Top;
            configPresetLabel.ForeColor = System.Drawing.SystemColors.Control;
            configPresetLabel.Location = new System.Drawing.Point(10, 0);
            configPresetLabel.Name = "configPresetLabel";
            configPresetLabel.Size = new System.Drawing.Size(180, 23);
            configPresetLabel.TabIndex = 0;
            configPresetLabel.Text = resources.GetString("configPresetLabel.Text");
            configPresetLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            // 
            // statPresetPanel
            // 
            statPresetPanel.Controls.Add(statPresetComboBox);
            statPresetPanel.Controls.Add(statPresetLabel);
            statPresetPanel.Dock = System.Windows.Forms.DockStyle.Left;
            statPresetPanel.Location = new System.Drawing.Point(103, 18);
            statPresetPanel.Name = "statPresetPanel";
            statPresetPanel.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            statPresetPanel.Size = new System.Drawing.Size(240, 99);
            statPresetPanel.TabIndex = 1;
            // 
            // statPresetComboBox
            // 
            statPresetComboBox.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            statPresetComboBox.BackColor = System.Drawing.Color.Black;
            statPresetComboBox.ForeColor = System.Drawing.SystemColors.Control;
            statPresetComboBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            statPresetComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            statPresetComboBox.FormattingEnabled = true;
            statPresetComboBox.Location = new System.Drawing.Point(10, 26);
            statPresetComboBox.Name = "statPresetComboBox";
            statPresetComboBox.Size = new System.Drawing.Size(180, 24);
            statPresetComboBox.TabIndex = 1;
            statPresetComboBox.SelectedIndexChanged += (sender, _) =>
                OnMageQueueStatPresetSelectedIndexChanged(sender, new MageQueueEventArgs(mageQueueItem), statPresetComboBox.SelectedIndex);
            // 
            // statPresetLabel
            // 
            statPresetLabel.Dock = System.Windows.Forms.DockStyle.Top;
            statPresetLabel.ForeColor = System.Drawing.SystemColors.Control;
            statPresetLabel.Location = new System.Drawing.Point(10, 0);
            statPresetLabel.Name = "statPresetLabel";
            statPresetLabel.Size = new System.Drawing.Size(180, 23);
            statPresetLabel.TabIndex = 0;
            statPresetLabel.Text = resources.GetString("statPresetLabel.Text");
            statPresetLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            // 
            // previewPanel
            // 
            previewPanel.Controls.Add(previewPictureBox);
            previewPanel.Dock = System.Windows.Forms.DockStyle.Left;
            previewPanel.Location = new System.Drawing.Point(3, 18);
            previewPanel.Name = "previewPanel";
            previewPanel.Size = new System.Drawing.Size(100, 99);
            previewPanel.TabIndex = 8;
            // 
            // previewPictureBox
            // 
            previewPictureBox.Anchor = System.Windows.Forms.AnchorStyles.None;
            previewPictureBox.Location = new System.Drawing.Point(18, 1);
            previewPictureBox.Name = "previewPictureBox";
            previewPictureBox.Size = new System.Drawing.Size(64, 64);
            previewPictureBox.TabIndex = 0;
            previewPictureBox.TabStop = false;
            previewPictureBox.Image = mageQueueItem.ItemPreview;
            
            queueItemGroupBox.ResumeLayout(false);
            buttonsPanel.ResumeLayout(false);
            buttonsPanel.PerformLayout();
            configPresetPanel.ResumeLayout(false);
            statPresetPanel.ResumeLayout(false);
            previewPanel.ResumeLayout(false);
            
            ((System.ComponentModel.ISupportInitialize) (previewPictureBox)).EndInit();
            
            
            configManager.UserSettings.PresetsChanged += (_, _) => UpdateStatPresets(statPresetComboBox);
            UpdateStatPresets(statPresetComboBox);
            configManager.UserSettings.ConfigPresetsChanged += (_, _) => UpdateConfigPresets(configPresetComboBox);
            UpdateConfigPresets(configPresetComboBox);

            return queueItemGroupBox;
        }

        private void UpdateStatPresets(ComboBox comboBox) {
            var selectedIndex = comboBox.SelectedItem;
            comboBox.DataSource =
                configManager.UserSettings.Presets.Presets.Select(preset => preset.Name)
                    .Prepend("Default")
                    .ToArray();
            comboBox.SelectedItem = selectedIndex;
        }

        private void UpdateConfigPresets(ComboBox comboBox) {
            var selectedIndex = comboBox.SelectedItem;
            comboBox.DataSource =
                configManager.UserSettings.ConfigPresets.Presets.Select(preset => preset.Name)
                    .Prepend("Default")
                    .ToArray();
            comboBox.SelectedItem = selectedIndex;
        }
    }
}
