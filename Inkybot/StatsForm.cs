using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Security;
using System.Threading;
using System.Windows.Forms;
using Inkybot.Adapters;
using Inkybot.Api;
using Inkybot.Contracts;
using Inkybot.Dofus;
using Inkybot.Dofus.Contracts;
using Inkybot.Dofus.Repositories;
using Inkybot.Domain;
using Inkybot.Events;
using Inkybot.Exceptions;
using Inkybot.Extensions;
using Inkybot.Helpers;
using Inkybot.Resources;
using Inkybot.Services;
using DataGridView = Inkybot.Helpers.DataGridView;
using Debug = System.Diagnostics.Debug;
using DofusMagingJob = Inkybot.Contracts.DofusMagingJob;
using MageConfig = Inkybot.Dofus.MageConfig;

namespace Inkybot
{
    public partial class StatsForm : Form
    {
        public event EventHandler<ExceptionEventArgs>? Error;
        private readonly ScreenReaderDataProvider dataProvider;
        private DofusMagingJob magingJob;
        private ConfigManager configManager;
        private AuthManager auth;
        private static bool hasDisplayedWarningAboutMultipleMinimums = false;

        public StatsForm() {
            InitializeComponent();
            InitializeCustomComponents();
            magingJob = Program.Services.GetService<DofusMagingJob>();
            dataProvider = (ScreenReaderDataProvider) Program.Services.GetService<DofusDataProvider>();
            configManager = (ConfigManager) Program.Services.GetService<MageConfigManager>();
            auth = Program.Services.GetService<AuthManager>();
            actionsPanel.Hide();
            helpPanel.Hide();
        }

        private void StatsForm_Loaded(object sender, EventArgs e) {
            exoStatComboBox.DataSource =
                Stat.Stats.Values.Select(stat => stat.DisplayName).ToArray();
            exoStatComboBox.SelectedIndex = Stat.Stats.Count - 1;
            configManager.ConfigModified += OnConfigModified;
            dataProvider.FetchedItem += OnStatsFetched;
            LoadPresetsToComboBox();
        }

        private void LoadPresetsToComboBox() {
            var presets = configManager.UserSettings.Presets.Presets;
            
            presetsComboBox.DataSource =
                presets.Select(preset => preset.Name)
                    .Prepend("None")
                    .ToArray();
        }

        private void OnConfigModified(object sender, ConfigModifiedEventArgs e) {
            Invoke(new MethodInvoker(() => {
                RebuildDataGridView(e.Config);
            }));
        }

        private void OnStatsFetched(object sender, ItemEventArgs e) {
            if (!magingJob.IsMaging && Visible) {
                configManager.EnforceConfigSetForItem(e.Item);
            }
            Invoke(new MethodInvoker(() => {
                UpdateDataGridView(e.Item);
                
                if (e.Item.IsValid) {
                    actionsPanel.Show();
                    helpPanel.Show();
                } else {
                    actionsPanel.Hide();
                    helpPanel.Hide();
                }
            }));
        }

        private void UpdateDataGridView(Item item) {
            if (!DataGridViewMatchesItem(item)) {
                RebuildDataGridView(item);
                presetsComboBox.SelectedIndex = 0;
            } else {
                UpdateDataGridRowValues(item);
            }
        }

        private void UpdateDataGridRowValues(Item item) {
            var updatedStats = new List<Stat>();
            
            foreach (var itemStat in item.Stats) {
                if (!itemStat.Stat.Mageable)
                    continue;
                
                var row = statsDataGridView.Rows
                    .FindWithTag<ItemStatRow>(tag => tag.Stat == itemStat.Stat)!;
                
                row.Cells[0].ToolTipText = itemStat.Exo
                    ? "Min: -\nMax: -"
                    : $"Min: {itemStat.Min}\nMax: {itemStat.Max}";
                row.Cells[1].Value = itemStat.Value;
                updatedStats.Add(itemStat.Stat);
            }
            
            var untouchedRows = statsDataGridView.Rows.Cast<DataGridViewRow>()
                .Where(row => !updatedStats.Contains(((ItemStatRow) row.Tag).Stat));

            foreach (var untouchedRow in untouchedRows) {
                var stat = ((ItemStatRow) untouchedRow.Tag).Stat;
                if (!stat.Mageable)
                    continue;
                
                var config = configManager.Config![stat];
                // Remove it if it's not configured
                if (config!.Value.Target == null || config!.Value.Target == 0) {
                    statsDataGridView.Rows.Remove(untouchedRow);
                } else {
                    untouchedRow.Cells[1].Value = 0;
                }
            }
        }

        private bool DataGridViewMatchesItem(Item item) {
            var foundStats = new List<Stat>();
            
            foreach (var itemStat in item.Stats) {
                var row = statsDataGridView.Rows
                    .FindWithTag<ItemStatRow>(tag => tag.Stat == itemStat.Stat);
                if (row == null)
                    return false;
                
                var rowItemStat = (ItemStatRow) row!.Tag;
                foundStats.Add(rowItemStat.Stat);
            }
            
            var untouchedRows = statsDataGridView.Rows.Cast<DataGridViewRow>()
                .Where(row => !foundStats.Contains(((ItemStatRow) row.Tag).Stat));

            foreach (var untouchedRow in untouchedRows) {
                var stat = ((ItemStatRow) untouchedRow.Tag).Stat;
                if (!configManager.Config?[stat]?.Exo ?? true)
                    return false;
            }

            return true;
        }

        private bool TryRebuildDataGridViewWithExistingRows(MageConfig config) {
            if (config.StatsConfig.StandardStatsConfigs.Count > statsDataGridView.Rows.Count)
                return false;
            
            var updatedStats = new List<Stat>();
            
            foreach (var stat in config.StatsConfig.Keys.ToArray()) {
                var statConfig = config[stat]!;
                var itemStatConfig = statConfig.Value;
                
                var row = statsDataGridView.Rows
                    .FindWithTag<ItemStatRow>(tag => tag.Stat == stat);
                
                if (row == null && itemStatConfig.Exo) {
                    row = AddNewStatRow(
                        stat.DisplayName,
                        0,
                        itemStatConfig.Target,
                        itemStatConfig.Priority,
                        itemStatConfig.TargetMinimum,
                        true,
                        stat.Mageable);
                    row.Tag = new ItemStatRow(stat, true);
                }
                if (row == null)
                    return false;
                
                var rowItemStat = (ItemStatRow) row!.Tag;

                if (statConfig.Value.Exo != rowItemStat.Exo)
                    return false;

                row.Cells[2].Value = Numbers.ToString(itemStatConfig.Target);
                row.Cells[3].Value = Numbers.ToString(itemStatConfig.TargetMinimum);
                row.Cells[4].Value = itemStatConfig.Priority;
                row.Cells[0].ToolTipText = itemStatConfig.Exo
                    ? "Min: -\nMax: -"
                    : $"Min: {itemStatConfig.Minimum}\nMax: {itemStatConfig.Maximum}";

                updatedStats.Add(rowItemStat.Stat);
            }
            
            var untouchedRows = statsDataGridView.Rows.Cast<DataGridViewRow>()
                .Where(row => !updatedStats.Contains(((ItemStatRow) row.Tag).Stat)).ToArray();
            
            foreach (var dataGridViewRow in untouchedRows) {
                statsDataGridView.Rows.Remove(dataGridViewRow);
            }

            return true;
        }

        private void RebuildDataGridView(MageConfig config) {
            try {
                if (TryRebuildDataGridViewWithExistingRows(config))
                    return;
            } catch (Exception) {
            }

            statsDataGridView.Rows.Clear();
            foreach (var statConfig in config.StatsConfig) {
                var stat = statConfig.Key;
                var cfg = statConfig.Value;
                var mageStatConfig = config.StatsConfig[stat];
                
                var row = AddNewStatRow(stat.DisplayName, 0, cfg.Target, cfg.Priority, cfg.TargetMinimum, mageStatConfig.Exo, stat.Mageable);
                row.Tag = new ItemStatRow(stat, mageStatConfig.Exo);
                row.Cells[0].ToolTipText = mageStatConfig.Exo
                    ? "Min: -\nMax: -"
                    : $"Min: {statConfig.Value.Minimum}\nMax: {statConfig.Value.Maximum}";
            }
        }

        private void RebuildDataGridView(Item item) {
            statsDataGridView.Rows.Clear();

            foreach (var itemStat in item.Stats) {
                var row = AddNewStatRow(itemStat.Stat.DisplayName, itemStat.Value, itemStat.Max,  0, 0, itemStat.Exo, itemStat.Stat.Mageable);
                row.Tag = new ItemStatRow(itemStat);
                var (min, max) = itemStat.Exo
                    ? ("-", "-")
                    : (itemStat.Min.ToString(), itemStat.Max.ToString());
                row.Cells[0].ToolTipText = $"Min: {min}\nMax: {max}";
            }
        }

        private DataGridViewRow AddNewStatRow(string displayName, int value, int? target, int priority, int? targetMinimum, bool exo, bool mageable) {
            if (mageable) {
                statsDataGridView.Rows.Add(displayName, value, target?.ToString() ?? "-", targetMinimum?.ToString() ?? "-", priority);
            } else {
                statsDataGridView.Rows.Add(displayName, "-", "-", "-", priority);
            }
            var index = statsDataGridView.Rows.Count-1;
            var row = statsDataGridView.Rows[index];
            if (exo)
                row.DefaultCellStyle = exoCellStyle;
            if (!mageable) {
                row.ReadOnly = true;
                row.DefaultCellStyle = unmageableCellStyle;
            }
            return row;
        }

        private void StatsForm_VisibleChanged(object sender, EventArgs e) {
            if (!Visible) return;
            RefreshStats();
        }
        
        public void RefreshStats() {
            if (magingJob.IsMaging)
                return;
            new Thread(RefreshStatsTask).Start();
        }

        [HandleProcessCorruptedStateExceptions, SecurityCritical]
        private void RefreshStatsTask() {
            try {
                Invoke(new MethodInvoker(() => {
                    selectPresetPanel.Enabled = false;
                    refreshButton.Enabled = false;
                }));
                    
                dataProvider.Reset();
                dataProvider.FetchData();
                var item = dataProvider.Item();
                configManager.EnforceConfigSetForItem(item);
                    
            } catch (Exception exception) {
                Error?.Invoke(this, new ExceptionEventArgs(exception));
                dataProvider.Reset();
                Debug.WriteLine(exception.Message);
                Debug.WriteLine(exception.StackTrace);
            }
                
            Invoke(new MethodInvoker(() => {
                selectPresetPanel.Enabled = true;
                refreshButton.Enabled = true;
            }));
        }

        private void StatsForm_Closing(object sender, CancelEventArgs cancelEventArgs) {
            cancelEventArgs.Cancel = true;
            Hide();
        }

        private void StatsForm_OnChangeValue(object sender, DataGridViewCellEventArgs e) {
            var row = statsDataGridView.Rows[e.RowIndex];
            var cell = row.Cells[e.ColumnIndex];

            var statRow = (ItemStatRow) row.Tag;
            var stat = statRow.Stat;
            if (!stat.Mageable)
                return;
            var newValue = Numbers.Parse(cell.Value.ToString());
            
            if (e.ColumnIndex == 2)
                configManager.ChangeStatConfigTarget(stat, newValue ?? 0);
            else if (e.ColumnIndex == 3) {
                var target = configManager.Config![stat]!.Value.Target;
                newValue = newValue > target
                    ? target
                    : newValue;
                cell.Value = Numbers.ToString(newValue);
                configManager.ChangeStatConfigTargetMinimum(stat, newValue);
            } else if (e.ColumnIndex == 4) {
                configManager.ChangeStatConfigPriority(stat, newValue ?? 0);
            }
            
            if (!hasDisplayedWarningAboutMultipleMinimums
                && configManager.UserSettings.ShowUserWarnings
                && configManager.Config!.StatsConfig.Values.Count(statConfig => !statConfig.Exo && statConfig.TargetMinimum != null) >= 2)
            {
                MessageBox.Show(
                    resources.GetString("popup.multiple_minimums_info"),
                    resources.GetString("popup.multiple_minimums_info_title"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                hasDisplayedWarningAboutMultipleMinimums = true;
            }
        }

        private void exoStatComboBox_SelectedIndexChanged(object sender, EventArgs e) {
            if (Stat.Stats.Values.Any(stat => stat.DisplayName == exoStatComboBox.Text))
                addExoButton.Enabled = true;
        }

        private void EnforceUserHasPermissionToMageExo() {
            if (auth.User == null)
                return;
            
            var text = (auth.User.canMageExos, auth.User.is_free_trial, auth.User.numberOfExoMagesLeftInPlan) switch {
                (_, true, _) => resources.GetString("popup.error_notavailable_freetrial"),
                (_, false, < 1) => resources.GetString("popup.error_notavailable_current_plan_run_out"),
                _ => resources.GetString("popup.error_notavailable_current_plan"),
            };

            (auth as ApiAuthManager)?.EnforceUserHasPermissionToMageExo(
                text,
                resources.GetString("popup.error_restricted")
            );
        }

        private void addExoButton_Click(object sender, EventArgs e) {
            try {
                EnforceUserHasPermissionToMageExo();
            } catch (UserForbiddenException) {
                return;
            }
            
            var stat = Stat.Stats.Values.First(stat => stat.DisplayName == exoStatComboBox.Text);
            var exoConfig = MageConfig.ItemStatMageConfig.MakeExo(
                stat, 
                stat.StrongestRune.IncreaseInValue, 
                stat.StrongestRune.IncreaseInValue,
                0);
            
            configManager.ChangeStatConfig(stat, exoConfig);
        }

        private class ItemStatRow
        {
            public readonly Stat Stat;
            public readonly bool Exo;

            public ItemStatRow(Stat stat, bool exo) {
                Stat = stat;
                Exo = exo;
            }

            public ItemStatRow(ItemStat itemStat) {
                Stat = itemStat.Stat;
                Exo = itemStat.Exo;
            }

            public static bool operator ==(ItemStatRow operand1, ItemStatRow operand2) {
                return operand1.Stat == operand2.Stat && operand1.Exo == operand2.Exo;
            }

            public static bool operator !=(ItemStatRow operand1, ItemStatRow operand2) {
                return !(operand1 == operand2);
            }
        }

        private void clearExosButton_Click(object sender, EventArgs e) {
            configManager.RemoveExos();
        }

        private void addPresetButton_Click(object sender, EventArgs e) {
            var index = presetsComboBox.SelectedIndex;
            if (index == 0) return;
            
            var config = configManager.Config!.StatsConfig
                .Select(statConfig =>
                    new ItemStatConfigAdapter(statConfig.Key, statConfig.Value).ToSerializable())
                .ToArray();
            var preset = new ItemPreset {
                Name = presetsComboBox.Text,
                Stats = config
            };
            
            var existingPresets = configManager.UserSettings.Presets.Presets;

            if (index <= 0) {
                existingPresets = existingPresets.Append(preset).ToArray();
                index = existingPresets.Length;
            } else {
                existingPresets[index - 1] = preset;
            }
            
            configManager.UserSettings.Presets = new ItemPresets {
                Presets = existingPresets
            };
            
            LoadPresetsToComboBox();
            presetsComboBox.SelectedIndex = index;
        }

        private void presetsComboBox_SelectedIndexChanged(object sender, EventArgs e) {
            var index = presetsComboBox.SelectedIndex;
            if (index == 0) return;
            
            var preset = Properties.Settings.Default.presets.Presets.Skip(index-1).First();
            var itemStats = preset.Stats.Select(statPreset => {
                var stat = Stat.FirstOrNew(statPreset.Stat);

                return new ItemStat(stat, 0, statPreset.Minimum, statPreset.Maximum);
            }).ToArray();
            
            var item = new Item(new ItemStatRepository(itemStats));
            if (item.HasExo) {
                try {
                    EnforceUserHasPermissionToMageExo();
                } catch (UserForbiddenException) {
                    presetsComboBox.SelectedIndex = 0;
                    return;
                }
            }
            if (!configManager.ConfigIsSetForItem(item)) {
                configManager.ResetConfig(item);
            }
            
            foreach (var statPreset in preset.Stats) {
                var stat = Stat.FirstOrNew(statPreset.Stat);
                if (!stat.Mageable)
                    continue;
                configManager.ChangeStatConfigTarget(stat, statPreset.Target);
                configManager.ChangeStatConfigTargetMinimum(stat, statPreset.TargetMinimum);
                configManager.ChangeStatConfigPriority(stat, statPreset.Priority);
            }
        }

        private void deletePresetButton_Click(object sender, EventArgs e) {
            var index = presetsComboBox.SelectedIndex;
            if (index <= 0) return;
            
            var existingPresets = configManager.UserSettings.Presets.Presets.ToList();
            existingPresets.RemoveAt(index-1);
            configManager.UserSettings.Presets = new ItemPresets {
                Presets = existingPresets.ToArray()
            };
            LoadPresetsToComboBox();
        }

        private void linkLabel1_LinkClicked_1(object sender, EventArgs e) {
            Process.Start(Server.ConfigsUrl);
        }

        private void showAdvancedOptionsButton_Click(object sender, EventArgs e) {
            if (auth.User?.is_free_trial ?? false) {
                MessageBox.Show(
                    resources.GetString("popup.error_notavailable_freetrial"),
                    resources.GetString("popup.error_restricted"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }
            
            if (showAdvancedOptionsButton.Text == "+") {
                showAdvancedOptionsButton.Text = "-";
                tooltip.SetToolTip(
                    showAdvancedOptionsButton, 
                    resources.GetString("showAdvancedOptionsButton.ToolTipTextHide"));
            } else {
                showAdvancedOptionsButton.Text = "+";
                tooltip.SetToolTip(
                    showAdvancedOptionsButton, 
                    resources.GetString("showAdvancedOptionsButton.ToolTipText"));
            }
            
            statsDataGridView.Columns[3].Visible =
                statsDataGridView.Columns[4].Visible = !statsDataGridView.Columns[4].Visible;
        }

        private void refreshButton_Click(object sender, EventArgs e) {
            RefreshStats();
        }

        private void OnRefreshButtonPaint(object sender, PaintEventArgs e) {
            base.OnPaint(e);
            var format = new StringFormat();
            format.Alignment = StringAlignment.Center;
            format.LineAlignment = StringAlignment.Center;

            e.Graphics.DrawString(
                "⟲",
                refreshButton.Font,
                new SolidBrush(refreshButton.ForeColor),
                refreshButton.ClientRectangle,
                format);
        }

        private void StatsForm_OnStatsDataGridViewValidating(object sender, DataGridViewCellValidatingEventArgs e) {
            if (e.ColumnIndex == 0) return;
            DataGridView.OnValidatingDataGridViewCellNumeric(sender, e);
        }
    }
}
