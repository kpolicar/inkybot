using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Inkybot.Adapters;
using Inkybot.Contracts;
using Inkybot.Domain;
using Inkybot.Domain.Repositories;
using Inkybot.Events;
using Inkybot.Resources;
using Inkybot.Services;
using DofusMagingJob = Inkybot.Contracts.DofusMagingJob;
using StatConfig = Inkybot.Domain.StatConfig;

namespace Inkybot
{
    public partial class StatsForm : Form
    {
        public event EventHandler<ExceptionEventArgs> Error;
        private readonly DofusDataProvider dataProvider;
        private DofusMagingJob magingJob;
        private ConfigManager configManager;

        public StatsForm() {
            InitializeComponent();
            InitializeCustomComponents();
            magingJob = (DofusMagingJob) Program.Services.GetService(typeof(DofusMagingJob));
            dataProvider = (DofusDataProvider) Program.Services.GetService(typeof(DofusDataProvider));
            configManager = (ConfigManager) Program.Services.GetService(typeof(ConfigManager));
            actionsPanel.Hide();
        }

        private void StatsForm_Loaded(object sender, EventArgs e) {
            exoStatComboBox.DataSource =
                Stat.Stats.Select(stat => stat.DisplayName).ToArray();
            exoStatComboBox.SelectedIndex = Stat.Stats.Length - 1;
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
                } else {
                    actionsPanel.Hide();
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
            for (var i = 0; i < statsDataGridView.Rows.Count; i++) {
                var row = statsDataGridView.Rows[i];
                var updatingFallenExos = i >= item.Stats.Length;
                var stat = updatingFallenExos ? ((ItemStatRow) row.Tag).Stat : item.Stats[i].stat;
                if (!stat.Mageable)
                    continue;
                    
                if (updatingFallenExos) {
                    if (configManager.Config.For(stat).Target == 0) {
                        statsDataGridView.Rows.RemoveAt(i);
                    } else
                        statsDataGridView[1, i].Value = 0;
                    
                    continue;
                }
                
                row.Cells[1].Value = item.Stats[i].value;
            }
        }

        private bool DataGridViewMatchesItem(Item item) {
            var configuredCount = statsDataGridView.Rows.Count;
            // All the current stats must always be configured
            if (item.Stats.Length > configuredCount)
                return false;

            for (var i = 0; i < item.Stats.Length; i++) {
                var row = statsDataGridView.Rows[i];
                var onRow = (ItemStatRow) row.Tag;

                // If there are more configured stats, they must be exos
                    
                if (onRow.Stat != item.Stats[i].stat) {
                    return false;
                }
            }
            

            return true;
        }

        private void RebuildDataGridView(Config config) {
            statsDataGridView.Rows.Clear();

            foreach (var statConfig in config.StatsConfig) {
                var stat = statConfig.Key;
                var cfg = statConfig.Value;
                var itemStat = config.Item.Stats.FirstOrDefault(itemStat => itemStat.stat == stat);
                
                var row = AddNewStatRow(stat.DisplayName, itemStat.value, cfg.Target, itemStat.Exo || itemStat == default, stat.Mageable);
                row.Tag = new ItemStatRow(stat);
            }
        }

        private void RebuildDataGridView(Item item) {
            statsDataGridView.Rows.Clear();

            foreach (var itemStat in item.Stats) {
                var row = AddNewStatRow(itemStat.stat.DisplayName, itemStat.value, itemStat.max, itemStat.Exo, itemStat.stat.Mageable);
                row.Tag = new ItemStatRow(itemStat);
            }
        }

        private DataGridViewRow AddNewStatRow(string displayName, int value, int max, bool exo, bool mageable) {
            if (mageable) {
                statsDataGridView.Rows.Add(displayName, value, max);
            } else {
                statsDataGridView.Rows.Add(displayName, "-", "-");
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
                    
                    dataProvider.FetchData();
                    var item = dataProvider.Item();
                    configManager.EnforceConfigSetForItem(item);
                    configManager.RemoveFallenUnconfiguredStats(item);
                    
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
            if (e.ColumnIndex != 2) return;
            var row = statsDataGridView.Rows[e.RowIndex];
            var cell = row.Cells[e.ColumnIndex];

            var statRow = (ItemStatRow) row.Tag;
            var stat = statRow.Stat;
            if (!stat.Mageable)
                return;
            
            int max;
            var newTargetIsValidNumber = int.TryParse(cell.Value.ToString(), out max);
            if (!newTargetIsValidNumber) {
                cell.Value = configManager.Config.For(stat).Target;
                return;
            }

            configManager.ChangeStatConfigTarget(stat, max);
        }

        private void exoStatComboBox_SelectedIndexChanged(object sender, EventArgs e) {
            if (Stat.Stats.Any(stat => stat.DisplayName == exoStatComboBox.Text))
                addExoButton.Enabled = true;
        }

        private void addExoButton_Click(object sender, EventArgs e) {
            var stat = Stat.Stats.First(stat => stat.DisplayName == exoStatComboBox.Text);
            var exoConfig = new StatConfig(stat, 0, 0, 0);
            
            configManager.ChangeStatConfig(stat, exoConfig);
        }

        private class ItemStatRow
        {
            public readonly Stat Stat;
            public readonly bool Exo;

            public ItemStatRow(Stat stat) {
                Stat = stat;
                Exo = true;
            }

            public ItemStatRow(ItemStat itemStat) {
                Stat = itemStat.stat;
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
            
            var config = configManager.Config.StatsConfig
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
    }
}
