using System;
using System.ComponentModel;
using System.Windows.Forms;
using Inkybot.Dofus;
using Inkybot.Domain;
using Inkybot.Extensions;
using Inkybot.Helpers;
using Inkybot.Services;

namespace Inkybot
{
    public partial class ConfigForm : Form
    {
        private ConfigManager configManager;
        
        public ConfigForm() {
            InitializeComponent();
            InitializeCustomComponents();
            configManager = Program.Services.GetService<ConfigManager>();
            //var ini = Stat.Stats.Where(sta => sta.Identifier == "initiative").First();
        }

        public void ConfigForm_OnLoad(object sender, EventArgs eventArgs) {
            var config = configManager.DefaultStatConfig!;
            foreach (var stat in Stat.Stats.Values) {
                var statConfig = config[stat];
                
                var rowIndex = statsDataGridView.Rows.Add(
                    stat.DisplayName(),
                    ParseConfigThreshold(statConfig.ChangeToPaRuneThreshold),
                    ParseConfigThreshold(statConfig.ChangeToRaRuneThreshold),
                    ParseConfigThreshold(statConfig.MaxValueAtWhichSmRuneCanHit),
                    ParseConfigThreshold(statConfig.MaxValueAtWhichPaRuneCanHit)
                    );
                var row = statsDataGridView.Rows[rowIndex];
                row.Tag = stat;
                if (!stat.CanUsePaRunes) {
                    row.Cells[1].ReadOnly = row.Cells[4].ReadOnly = true;
                    row.Cells[1].Style = row.Cells[4].Style = readonlyCellStyle;
                }
                if (!stat.CanUseRaRunes) {
                    row.Cells[2].ReadOnly = true;
                    row.Cells[2].Style = readonlyCellStyle;
                }

                SetConfigRowTooltips(row);
            }

            restoreHighSinkStatsCheckbox.Checked = Properties.Settings.Default.restoreHighSinkStatImmediately;
            autoRestartBotCheckbox.Checked = Properties.Settings.Default.autoRestartBot;
        }

        private void SetConfigRowTooltips(DataGridViewRow row) {
            var stat = (Stat) row.Tag;
            var smRune = new Rune(stat, Rune.Type.Sm);
            var paRune = new Rune(stat, Rune.Type.Pa);
            var raRune = new Rune(stat, Rune.Type.Ra);
            var config = configManager.DefaultStatConfig[stat];
            
            if (stat.CanUsePaRunes) {
                row.Cells[1].ToolTipText = config.ChangeToPaRuneThreshold switch {
                    int.MaxValue => resources.GetString("config.neverchange")!
                        .Replace(":rune", paRune.ToString()),
                    
                    0 => resources.GetString("config.prefer")!
                        .Replace(":rune1", paRune.ToString())
                        .Replace(":rune2", smRune.ToString()),
                    
                    _ => resources.GetString("config.changeonthreshold")!
                        .Replace(":rune", paRune.ToString())
                        .Replace(":stat", stat.ToString())
                        .Replace(":threshold", config.ChangeToPaRuneThreshold.ToString())
                };
                row.Cells[4].ToolTipText = (config.MaxValueAtWhichPaRuneCanHit, config.ChangeToPaRuneThreshold) switch {
                    (_, int.MaxValue) => resources.GetString("config.neverchange_threshold")!
                        .Replace(":rune", paRune.ToString())
                        .Replace(":threshold", resources.GetString("PaRuneThresholdColumn.HeaderText")),
                    
                    (int.MaxValue, _) => resources.GetString("config.alwaysland")!
                        .Replace(":rune", paRune.ToString()),
                    
                    (0, _) => resources.GetString("config.neverchange")!
                        .Replace(":rune", paRune.ToString()),
                    
                    _ => resources.GetString("config.canland_maxvalue")!
                        .Replace(":rune", paRune.ToString())
                        .Replace(":maxvalue", config.MaxValueAtWhichPaRuneCanHit.ToString())
                };
            }
            if (stat.CanUseRaRunes) {
                row.Cells[2].ToolTipText = config.ChangeToRaRuneThreshold switch {
                    int.MaxValue => resources.GetString("config.neverchange")!
                        .Replace(":rune", raRune.ToString()),
                    
                    0 => resources.GetString("config.prefer")!
                        .Replace(":rune1", raRune.ToString())
                        .Replace(":rune2", paRune.ToString()),
                    
                    _ => resources.GetString("config.changeonthreshold")!
                        .Replace(":rune", raRune.ToString())
                        .Replace(":stat", stat.ToString())
                        .Replace(":threshold", config.ChangeToRaRuneThreshold.ToString()),
                };
            }
            row.Cells[3].ToolTipText = config.MaxValueAtWhichSmRuneCanHit switch {
                int.MaxValue => resources.GetString("config.alwaysland")!
                    .Replace(":rune", smRune.ToString()),
                
                0 => resources.GetString("config.neverchange")!
                    .Replace(":rune", smRune.ToString())
                    .Replace(":rune", smRune.ToString()),
                
                _ => resources.GetString("config.canland_maxvalue")!
                    .Replace(":rune", smRune.ToString())
                    .Replace(":maxvalue", config.MaxValueAtWhichSmRuneCanHit.ToString()),
            };
        }

        protected string ParseConfigThreshold(int? threshold) {
            return threshold?.ToString()
                   ?? "-";
        }

        private void ConfigForm_OnChangeValue(object sender, DataGridViewCellEventArgs e) {
            if (e.ColumnIndex < 1 || e.ColumnIndex > 4 || e.RowIndex < 0) return;
            var row = statsDataGridView.Rows[e.RowIndex];
            var cell = row.Cells[e.ColumnIndex];

            var stat = (Stat) row.Tag;
            var config = configManager.DefaultStatConfig[stat];
            try {

                var current = config.Deconstruct();
                var newConfig = e.ColumnIndex switch {
                    1 => new Stat.StatConfig(
                        current.changeToPaRuneThreshold = Numbers.Parse(cell.Value.ToString())
                        ),
                    2 => new Stat.StatConfig(
                        current.changeToRaRuneThreshold = Numbers.Parse(cell.Value.ToString())
                        ),
                    3 => new Stat.StatConfig(
                        current.maxValueSmRuneCanHit = Numbers.Parse(cell.Value.ToString())
                        ),
                    4 => new Stat.StatConfig(
                        current.maxValuePaRuneCanHit = Numbers.Parse(cell.Value.ToString())
                        ),
                };
                
                configManager.DefaultStatConfig[stat] = newConfig;
            } catch (FormatException) {
                
            }
            SetConfigRowTooltips(row);
        }

        private void ConfigForm_OnRestoreHighSinkStatsCheckboxCheckedChanged(object sender, EventArgs e) {
            Properties.Settings.Default.restoreHighSinkStatImmediately = restoreHighSinkStatsCheckbox.Checked;
            Properties.Settings.Default.Save();
        }

        private void ConfigForm_Closing(object sender, CancelEventArgs cancelEventArgs) {
            cancelEventArgs.Cancel = true;
            Hide();
        }

        private void ConfigForm_OnAutoRestartBotCheckboxCheckedChanged(object sender, EventArgs e) {
            Properties.Settings.Default.autoRestartBot = autoRestartBotCheckbox.Checked;
            Properties.Settings.Default.Save();
        }
    }
}

