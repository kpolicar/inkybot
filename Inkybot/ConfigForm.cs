using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Inkybot.Contracts;
using Inkybot.Dofus;
using Inkybot.Dofus.Contracts;
using Inkybot.Helpers;
using Inkybot.Services;
using Debug = System.Diagnostics.Debug;

namespace Inkybot
{
    public partial class ConfigForm : Form
    {
        private FileSystemUserSettingsConfigManager userSettingsConfigManager;
        private MagingAIServiceManager magingAiManager;
        
        public ConfigForm() {
            InitializeComponent();
            InitializeCustomComponents();
            userSettingsConfigManager = (FileSystemUserSettingsConfigManager)
                Program.Services.GetService<UserSettingsConfigManager>();
            magingAiManager =
                Program.Services.GetService<MagingAIServiceManager>();
        }

        public void ConfigForm_OnLoad(object sender, EventArgs eventArgs) {
            var config = userSettingsConfigManager.Config();
            foreach (var stat in Stat.Stats.Values) {
                var statConfig = config[stat];
                
                var rowIndex = statsDataGridView.Rows.Add(
                    stat.DisplayName,
                    Numbers.ToString(statConfig.ChangeToPaRuneThreshold),
                    Numbers.ToString(statConfig.ChangeToRaRuneThreshold),
                    Numbers.ToString(statConfig.MaxValueAtWhichSmRuneCanHit),
                    Numbers.ToString(statConfig.MaxValueAtWhichPaRuneCanHit)
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

            restoreHighSinkStatsCheckbox.Checked = userSettingsConfigManager.RestoreHighSinkStats;
            autoRestartBotCheckbox.Checked = userSettingsConfigManager.AutoRestartBot;
            showWarningsCheckbox.Checked = userSettingsConfigManager.ShowUserWarnings;
            publishExosCheckbox.Checked = userSettingsConfigManager.PublishExos;
            enableRuneCheckingCheckbox.Checked = userSettingsConfigManager.EnableRuneChecking;
        }

        private void SetConfigRowTooltips(DataGridViewRow row) {
            var stat = (Stat) row.Tag;
            var smRune = new Rune(stat, Rune.RuneType.Sm);
            var paRune = new Rune(stat, Rune.RuneType.Pa);
            var raRune = new Rune(stat, Rune.RuneType.Ra);
            var config = userSettingsConfigManager.Config(stat);
            
            if (stat.CanUsePaRunes) {
                row.Cells[1].ToolTipText = config.ChangeToPaRuneThreshold switch {
                    null => resources.GetString("config.neverchange")!
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
                    (_, null) => resources.GetString("config.neverchange_threshold")!
                        .Replace(":rune", paRune.ToString())
                        .Replace(":threshold", resources.GetString("PaRuneThresholdColumn.HeaderText")),
                    
                    (null, _) => resources.GetString("config.alwaysland")!
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
                    null => resources.GetString("config.neverchange")!
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
                null => resources.GetString("config.alwaysland")!
                    .Replace(":rune", smRune.ToString()),
                
                0 => resources.GetString("config.neverchange")!
                    .Replace(":rune", smRune.ToString())
                    .Replace(":rune", smRune.ToString()),
                
                _ => resources.GetString("config.canland_maxvalue")!
                    .Replace(":rune", smRune.ToString())
                    .Replace(":maxvalue", config.MaxValueAtWhichSmRuneCanHit.ToString()),
            };
        }

        private void ConfigForm_OnChangeValue(object sender, DataGridViewCellEventArgs e) {
            if (e.ColumnIndex < 1 || e.ColumnIndex > 4 || e.RowIndex < 0) return;
            var row = statsDataGridView.Rows[e.RowIndex];
            var cell = row.Cells[e.ColumnIndex];

            var stat = (Stat) row.Tag;
            var currentConfig =
                userSettingsConfigManager.Config(stat).Deconstruct();
            
            try {
                var value = Numbers.Parse(cell.Value.ToString());
                var newConfig = new StatConfig(
                    changeToPaRuneThreshold:
                        e.ColumnIndex == 1 ? value : currentConfig.changeToPaRuneThreshold,
                    changeToRaRuneThreshold:
                        e.ColumnIndex == 2 ? value : currentConfig.changeToRaRuneThreshold,
                    maxValueSmRuneCanHit:
                        e.ColumnIndex == 3 ? value : currentConfig.maxValueSmRuneCanHit,
                    maxValuePaRuneCanHit:
                        e.ColumnIndex == 4 ? value : currentConfig.maxValuePaRuneCanHit
                );
                Debug.WriteLine(newConfig);
                
                userSettingsConfigManager.SetConfig(stat, newConfig);
            } catch (FormatException) {
                
            }
            SetConfigRowTooltips(row);
        }

        private void ConfigForm_OnRestoreHighSinkStatsCheckboxCheckedChanged(object sender, EventArgs e) =>
            userSettingsConfigManager.RestoreHighSinkStats = restoreHighSinkStatsCheckbox.Checked;
        
        private void ConfigForm_OnAutoRestartBotCheckboxCheckedChanged(object sender, EventArgs e) =>
            userSettingsConfigManager.AutoRestartBot = autoRestartBotCheckbox.Checked;
        
        private void ConfigForm_OnShowWarningsCheckboxCheckboxCheckedChanged(object sender, EventArgs e) =>
            userSettingsConfigManager.ShowUserWarnings = showWarningsCheckbox.Checked;
        
        private void ConfigForm_OnEnableRuneCheckingCheckboxCheckedChanged(object sender, EventArgs e) =>
            userSettingsConfigManager.EnableRuneChecking = enableRuneCheckingCheckbox.Checked;

        private void ConfigForm_OnPublishExosCheckboxCheckedChanged(object sender, EventArgs e) =>
            userSettingsConfigManager.PublishExos = publishExosCheckbox.Checked;

        private void ConfigForm_Closing(object sender, CancelEventArgs cancelEventArgs) {
            cancelEventArgs.Cancel = true;
            Hide();
        }

        private void scriptChangeButton_Click(object sender, EventArgs e) {
        
            var result = scriptFileDialog.ShowDialog();
            if (result == DialogResult.OK) {
                var path = scriptFileDialog.FileName;
                customScriptPathLabel.Text = Path.GetFileName(path);
                resources.ApplyResources(scriptValidPictureBox, "scriptValidPictureBoxLoading");
                scriptValidPictureBox.Show();
                
                _ = Task.Run(() => {
                    Thread.Sleep(500);
                    TrySwitchToCustomAIScript();
                });
            }
        }

        private void TrySwitchToCustomAIScript() {
            try {
                magingAiManager.UseCustomAIScript(scriptFileDialog.FileName);
                
                Invoke(new MethodInvoker(() => {
                    resources.ApplyResources(scriptValidPictureBox, "scriptValidPictureBoxValid");
                }));
            } catch (Exception) {
                Invoke(new MethodInvoker(() => {
                    resources.ApplyResources(scriptValidPictureBox, "scriptValidPictureBoxValidInvalid");
                }));
            }
            
            Invoke(new MethodInvoker(() => {
                scriptResetButton.Show();
            }));
        }

        private void scriptResetButton_Click(object sender, EventArgs e) {
            scriptResetButton.Hide();
            scriptValidPictureBox.Hide();
            customScriptPathLabel.Text = "";
            magingAiManager.UseBuiltInAIScript();
        }
    }
}

