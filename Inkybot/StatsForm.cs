using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
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
using Inkybot.Extensions;
using Inkybot.Helpers;
using Inkybot.Resources;
using Inkybot.Services;
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
            var presets = Properties.Settings.Default.presets?.Presets ?? new ItemPreset[] {};
            
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
            var configuredCount = statsDataGridView.Rows.Count;
            // All the current stats must always be configured
            if (item.Stats.Length > configuredCount)
                return false;

            foreach (var itemStat in item.Stats) {
                var row = statsDataGridView.Rows
                    .FindWithTag<ItemStatRow>(tag => tag.Stat == itemStat.Stat);
                if (row == null)
                    return false;
            }
            

            return true;
        }

        private bool TryRebuildDataGridViewWithExistingRows(MageConfig config) {
            if (config.StatsConfig.StandardStatsConfigs.Count > statsDataGridView.Rows.Count)
                return false;
            
            var updatedStats = new List<Stat>();
            
            foreach (var statConfig in config.StatsConfig) {
                var stat = statConfig.Key;
                var itemStatConfig = statConfig.Value;
                if (!stat.Mageable)
                    continue;
                
                var row = statsDataGridView.Rows
                    .FindWithTag<ItemStatRow>(tag => tag.Stat == stat);
                
                if (row == null && itemStatConfig.Exo) {
                    row = AddNewStatRow(
                        stat.DisplayName,
                        0,
                        itemStatConfig.Target,
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
                
                var row = AddNewStatRow(stat.DisplayName, 0, cfg.Target, cfg.TargetMinimum, mageStatConfig.Exo, stat.Mageable);
                row.Tag = new ItemStatRow(stat, mageStatConfig.Exo);
            }
        }

        private void RebuildDataGridView(Item item) {
            statsDataGridView.Rows.Clear();

            foreach (var itemStat in item.Stats) {
                var row = AddNewStatRow(itemStat.Stat.DisplayName, itemStat.Value, itemStat.Max,  null, itemStat.Exo, itemStat.Stat.Mageable);
                row.Tag = new ItemStatRow(itemStat);
            }
        }

        private DataGridViewRow AddNewStatRow(string displayName, int value, int? target, int? targetMinimum, bool exo, bool mageable) {
            if (mageable) {
                statsDataGridView.Rows.Add(displayName, value, target?.ToString() ?? "-", targetMinimum?.ToString() ?? "-");
            } else {
                statsDataGridView.Rows.Add(displayName, "-", "-", "-");
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
            if (!Visible || magingJob.IsMaging) return;

            RefreshStats();
        }
        
        private void RefreshStats() {
            var fetchStats = new ThreadStart(delegate {
                try {
                    Invoke(new MethodInvoker(() => {
                        selectPresetPanel.Enabled = false;
                    }));
                    
                    dataProvider.Reset();
                    dataProvider.FetchData();
                    var item = dataProvider.Item();
                    configManager.EnforceConfigSetForItem(item);
                    
                } catch (Exception exception) {
                    Error?.Invoke(this, new ExceptionEventArgs(exception));
                    Debug.WriteLine(exception.Message);
                    Debug.WriteLine(exception.StackTrace);
                }
                
                Invoke(new MethodInvoker(() => {
                    selectPresetPanel.Enabled = true;
                }));
            });
            
            new Thread(fetchStats).Start();
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

        private void addExoButton_Click(object sender, EventArgs e) {
            if (auth.User != null && auth.User.is_free_trial) {
                MessageBox.Show(
                    resources.GetString("popup.error_notavailable_freetrial"),
                    resources.GetString("popup.error_restricted"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }
            var stat = Stat.Stats.Values.First(stat => stat.DisplayName == exoStatComboBox.Text);
            var exoConfig = MageConfig.ItemStatMageConfig.MakeExo(
                stat, 
                stat.StrongestRune.IncreaseInValue, 
                stat.StrongestRune.IncreaseInValue);
            
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
                    new StatConfigAdapter(statConfig.Key, statConfig.Value).ToSerializable())
                .ToArray();
            var preset = new ItemPreset {
                Name = presetsComboBox.Text,
                Stats = config
            };
            
            var existingPresets = Properties.Settings.Default.presets?.Presets ?? new ItemPreset[] {};

            if (index <= 0) {
                Properties.Settings.Default.presets = new ItemPresets {
                    Presets = existingPresets.Append(preset).ToArray()
                };
                index = Properties.Settings.Default.presets.Presets.Length;
            } else {
                existingPresets[index - 1] = preset;
            }
            Properties.Settings.Default.Save();
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
            configManager.ResetConfig(item);
            
            foreach (var statPreset in preset.Stats) {
                var stat = Stat.FirstOrNew(statPreset.Stat);
                if (!stat.Mageable)
                    continue;
                configManager.ChangeStatConfigTarget(stat, statPreset.Target);
                configManager.ChangeStatConfigTargetMinimum(stat, statPreset.TargetMinimum);
            }
        }

        private void deletePresetButton_Click(object sender, EventArgs e) {
            var index = presetsComboBox.SelectedIndex;
            if (index <= 0) return;
            
            var existingPresets = (Properties.Settings.Default.presets?.Presets ?? new ItemPreset[] {}).ToList();
            existingPresets.RemoveAt(index-1);
            Properties.Settings.Default.presets = new ItemPresets {
                Presets = existingPresets.ToArray()
            };
            Properties.Settings.Default.Save();
            LoadPresetsToComboBox();
        }

        private void linkLabel1_LinkClicked_1(object sender, EventArgs e) {
            Process.Start(Server.ConfigsUrl);
        }
    }
}
