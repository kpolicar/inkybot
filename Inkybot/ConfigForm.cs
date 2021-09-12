using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Inkybot.Contracts;
using Inkybot.Dofus;
using Inkybot.Dofus.Contracts;
using Inkybot.Domain;
using Inkybot.Events;
using Inkybot.Extensions;
using Inkybot.Helpers;
using Inkybot.Services;
using DataGridView = Inkybot.Helpers.DataGridView;
using Debug = System.Diagnostics.Debug;
using StatConfigProvider = Inkybot.Services.StatConfigProvider;
using StatConfigProviderContract = Inkybot.Dofus.Contracts.StatConfigProvider;

namespace Inkybot
{
    public partial class ConfigForm : Form
    {
        private FileSystemUserSettingsConfigManager userSettingsConfigManager;
        private MagingAIServiceManager magingAiManager;
        private StatConfigProvider configProvider;
        private AuthManager auth;

        public int AutoShutdownDelay {
            private set;
            get;
        } = -1;

        public ConfigForm(StatsForm setupForm) {
            InitializeComponent();
            InitializeCustomComponents();
            SetupForm = setupForm;
            userSettingsConfigManager = (FileSystemUserSettingsConfigManager)
                Program.Services.GetService<UserSettingsConfigManager>();
            magingAiManager =
                Program.Services.GetService<MagingAIServiceManager>();
            configProvider =
                (StatConfigProvider) Program.Services.GetService<StatConfigProviderContract>();
            auth = Program.Services.GetService<AuthManager>();
        }

        public StatsForm SetupForm;

        public void ConfigForm_OnLoad(object sender, EventArgs eventArgs) {
            var config = userSettingsConfigManager.Config();
            foreach (var stat in Stat.Stats.Values) {
                var statConfig = config[stat];
                
                var rowIndex = statsDataGridView.Rows.Add(
                    stat.DisplayName,
                    statConfig.UseSmRunes,
                    statConfig.UsePaRunes,
                    statConfig.UseRaRunes,
                    Numbers.ToString(statConfig.ChangeToPaRuneThreshold),
                    Numbers.ToString(statConfig.ChangeToRaRuneThreshold),
                    Numbers.ToString(statConfig.MaxValueAtWhichSmRuneCanHit),
                    Numbers.ToString(statConfig.MaxValueAtWhichPaRuneCanHit)
                    );
                var row = statsDataGridView.Rows[rowIndex];
                row.Tag = stat;
                if (!stat.CanUsePaRunes) {
                    row.Cells[2].ReadOnly = row.Cells[4].ReadOnly = row.Cells[7].ReadOnly = true;
                    row.Cells[2].Style = row.Cells[4].Style = row.Cells[7].Style = readonlyCellStyle;
                }
                if (!stat.CanUseRaRunes) {
                    row.Cells[3].ReadOnly = row.Cells[5].ReadOnly = true;
                    row.Cells[3].Style = row.Cells[5].Style = readonlyCellStyle;
                }

                SetConfigRowTooltipsAndChangeStyles(row);
            }

            restoreHighSinkStatsCheckbox.Checked = userSettingsConfigManager.RestoreHighSinkStats;
            autoRestartBotCheckbox.Checked = userSettingsConfigManager.AutoRestartBot;
            showWarningsCheckbox.Checked = userSettingsConfigManager.ShowUserWarnings;
            publishExosCheckbox.Checked = userSettingsConfigManager.PublishExos;
            enableRuneCheckingCheckbox.Checked = userSettingsConfigManager.EnableRuneChecking;
            enableKamasCalculationCheckbox.Checked = userSettingsConfigManager.EnableKamasCalculation;
            enableMageQueueingCheckbox.Checked = userSettingsConfigManager.EnableMageQueueing;
            customResizeRatioNumericUpDown.Value = userSettingsConfigManager.CustomResizeMultiplier;
            magingAiManager.MagingAIChanged += OnMagingAIChanged;
            
            LoadPresetsToComboBox();
        }

        private void SetConfigRowTooltipsAndChangeStyles(DataGridViewRow row) {
            var stat = (Stat) row.Tag;
            var smRune = new Rune(stat, Rune.RuneType.Sm);
            var paRune = new Rune(stat, Rune.RuneType.Pa);
            var raRune = new Rune(stat, Rune.RuneType.Ra);
            var config = userSettingsConfigManager.Config(stat);
            var defaultConfig = configProvider.Default.Config(stat).Deconstruct();

            
            row.Cells[1].Style = !config.UseSmRunes ? modifiedStyle : row.DefaultCellStyle;
            row.Cells[2].Style = config.UsePaRunes != stat.CanUsePaRunes ? modifiedStyle : row.DefaultCellStyle;
            row.Cells[3].Style = config.UseRaRunes != stat.CanUseRaRunes ? modifiedStyle : row.DefaultCellStyle;
            
            row.Cells[1].ToolTipText = config.UseSmRunes
                ? resources.GetString("config.useRune")!.Replace(":rune", smRune.DisplayName)
                : resources.GetString("config.dontUseRune")!.Replace(":rune", smRune.DisplayName);

            if (stat.CanUsePaRunes) {
                row.Cells[2].ToolTipText = config.UsePaRunes
                    ? resources.GetString("config.useRune")!.Replace(":rune", paRune.DisplayName)
                    : resources.GetString("config.dontUseRune")!.Replace(":rune", paRune.DisplayName);
                
                row.Cells[4].ToolTipText = (config.ChangeToPaRuneThreshold switch {
                    null => resources.GetString("config.neverchange")!
                        .Replace(":rune", paRune.ToString()),
                    
                    0 => resources.GetString("config.prefer")!
                        .Replace(":rune1", paRune.ToString())
                        .Replace(":rune2", smRune.ToString()),
                    
                    _ => resources.GetString("config.changeonthreshold")!
                        .Replace(":rune", paRune.ToString())
                        .Replace(":stat", stat.ToString())
                        .Replace(":threshold", config.ChangeToPaRuneThreshold.ToString())
                }).FirstCharToUpper();
                row.Cells[4].Style = config.ChangeToPaRuneThreshold != defaultConfig.changeToPaRuneThreshold
                    ? modifiedStyle
                    : row.DefaultCellStyle;
                
                if (config.ChangeToPaRuneThreshold != defaultConfig.changeToPaRuneThreshold)
                    row.Cells[4].ToolTipText += "\n" + resources.GetString("config.default")!
                        .Replace(":value", defaultConfig.changeToPaRuneThreshold?.ToString() ?? "\"-\"");
                
                row.Cells[7].ToolTipText = ((config.MaxValueAtWhichPaRuneCanHit, config.ChangeToPaRuneThreshold) switch {
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
                }).FirstCharToUpper();

                row.Cells[7].Style = config.MaxValueAtWhichPaRuneCanHit != defaultConfig.maxValuePaRuneCanHit
                    ? modifiedStyle
                    : row.DefaultCellStyle;
                
                if (config.MaxValueAtWhichPaRuneCanHit != defaultConfig.maxValuePaRuneCanHit)
                    row.Cells[7].ToolTipText += "\n" + resources.GetString("config.default")!
                        .Replace(":value", defaultConfig.maxValuePaRuneCanHit?.ToString() ?? "\"-\"");
            }
            if (stat.CanUseRaRunes) {
                row.Cells[3].ToolTipText = config.UseRaRunes
                    ? resources.GetString("config.useRune")!.Replace(":rune", raRune.DisplayName)
                    : resources.GetString("config.dontUseRune")!.Replace(":rune", raRune.DisplayName);
                row.Cells[5].ToolTipText = (config.ChangeToRaRuneThreshold switch {
                    null => resources.GetString("config.neverchange")!
                        .Replace(":rune", raRune.ToString()),
                    
                    0 => resources.GetString("config.prefer")!
                        .Replace(":rune1", raRune.ToString())
                        .Replace(":rune2", paRune.ToString()),
                    
                    _ => resources.GetString("config.changeonthreshold")!
                        .Replace(":rune", raRune.ToString())
                        .Replace(":stat", stat.ToString())
                        .Replace(":threshold", config.ChangeToRaRuneThreshold.ToString()),
                }).FirstCharToUpper();
                
                row.Cells[5].Style = config.ChangeToRaRuneThreshold != defaultConfig.changeToRaRuneThreshold
                    ? modifiedStyle
                    : row.DefaultCellStyle;
                    
                if (config.ChangeToRaRuneThreshold != defaultConfig.changeToRaRuneThreshold)
                    row.Cells[5].ToolTipText += "\n" + resources.GetString("config.default")!
                        .Replace(":value", defaultConfig.changeToRaRuneThreshold?.ToString() ?? "\"-\"");
            }
            row.Cells[6].ToolTipText = (config.MaxValueAtWhichSmRuneCanHit switch {
                null => resources.GetString("config.alwaysland")!
                    .Replace(":rune", smRune.ToString()),
                
                0 => resources.GetString("config.neverchange")!
                    .Replace(":rune", smRune.ToString())
                    .Replace(":rune", smRune.ToString()),
                
                _ => resources.GetString("config.canland_maxvalue")!
                    .Replace(":rune", smRune.ToString())
                    .Replace(":maxvalue", config.MaxValueAtWhichSmRuneCanHit.ToString()),
            }).FirstCharToUpper();
            
            row.Cells[6].Style = config.MaxValueAtWhichSmRuneCanHit != defaultConfig.maxValueSmRuneCanHit
                ? modifiedStyle
                : row.DefaultCellStyle;
            if (config.MaxValueAtWhichSmRuneCanHit != defaultConfig.maxValueSmRuneCanHit)
                row.Cells[6].ToolTipText += "\n" + resources.GetString("config.default")!
                    .Replace(":value", defaultConfig.maxValueSmRuneCanHit?.ToString() ?? "\"-\"");
        }

        private void ConfigForm_OnChangeValue(object sender, DataGridViewCellEventArgs e) {
            if (e.ColumnIndex < 1 || e.ColumnIndex > 7 || e.RowIndex < 0) return;
            var row = statsDataGridView.Rows[e.RowIndex];
            var cell = row.Cells[e.ColumnIndex];

            var stat = (Stat) row.Tag;
            var currentConfig =
                userSettingsConfigManager.Config(stat).Deconstruct();
            
            try {
                var intValue = new Func<int?>(() => Numbers.Parse(cell.Value.ToString()));
                var boolValue = new Func<bool>(() => (bool)cell.Value);
                
                var newConfig = new StatConfig(
                    changeToPaRuneThreshold:
                        e.ColumnIndex == 4 ? intValue() : currentConfig.changeToPaRuneThreshold,
                    changeToRaRuneThreshold:
                        e.ColumnIndex == 5 ? intValue() : currentConfig.changeToRaRuneThreshold,
                    maxValueSmRuneCanHit:
                        e.ColumnIndex == 6 ? intValue() : currentConfig.maxValueSmRuneCanHit,
                    maxValuePaRuneCanHit:
                        e.ColumnIndex == 7 ? intValue() : currentConfig.maxValuePaRuneCanHit,
                    useSmRunes:
                        e.ColumnIndex == 1 ? boolValue() : currentConfig.useSmRunes,
                    usePaRunes:
                        e.ColumnIndex == 2 ? boolValue() : currentConfig.usePaRunes,
                    useRaRunes:
                        e.ColumnIndex == 3 ? boolValue() : currentConfig.useRaRunes
                );
                
                userSettingsConfigManager.SetConfig(stat, newConfig);
            } catch (FormatException) {
                
            }
            SetConfigRowTooltipsAndChangeStyles(row);
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
        
        private void ConfigForm_OnEnableKamasCalculationCheckboxCheckboxCheckedChanged(object sender, EventArgs e) =>
            userSettingsConfigManager.EnableKamasCalculation = enableKamasCalculationCheckbox.Checked;
        
        private void ConfigForm_OnEnableMageQueueingCheckboxCheckboxCheckedChanged(object sender, EventArgs e) =>
            userSettingsConfigManager.EnableMageQueueing = enableMageQueueingCheckbox.Checked;

        private void ConfigForm_Closing(object sender, CancelEventArgs cancelEventArgs) {
            cancelEventArgs.Cancel = true;
            Hide();
        }

        private void scriptChangeButton_Click(object sender, EventArgs e) {
            if (auth.User != null && !auth.User.canUseCustomMagingAI) {
                var text = !auth.User.onUnlimitedPlan && !auth.User.onStandardPlan
                    ? resources.GetString("popup.error_notavailable_unlimitedstandard_plan")
                    : resources.GetString("popup.error_notavailable_current_plan");
                MessageBox.Show(
                    text,
                    resources.GetString("popup.error_restricted"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }
        
            var result = scriptFileDialog.ShowDialog();
            if (result == DialogResult.OK) {
                var path = scriptFileDialog.FileName;
                customScriptPathLabel.Text = Path.GetFileName(path);
                resources.ApplyResources(scriptValidPictureBox, "scriptValidPictureBoxLoading");
                scriptValidPictureBox.Show();
                
                _ = Task.Run(() => {
                    Thread.Sleep(300);
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
        }

        private void scriptResetButton_Click(object sender, EventArgs e) {
            magingAiManager.UseBuiltInAIScript();
        }

        private void ConfigForm_OnCellEnter(object sender, DataGridViewCellEventArgs e) {
            if (e.ColumnIndex < 1 || e.ColumnIndex > 7 || e.RowIndex < 0) return;
            var row = statsDataGridView.Rows[e.RowIndex];
            var cell = row.Cells[e.ColumnIndex];

            tooltipLabelExtra.Text = cell.ToolTipText
                .Replace("\n", "; ");
        }
        
        private void OnMagingAIChanged(object sender, MagingAIChangedEventArgs e) {
            Invoke(new MethodInvoker(() => {
                if (!(e.AI is CustomDofusMagingAI)) {
                    scriptResetButton.Hide();
                    scriptValidPictureBox.Hide();
                    customScriptPathLabel.Text = "";
                } else if (e.AI is CustomDofusMagingAI) {
                    scriptResetButton.Show();
                }
            }));
        }

        private void exampleScriptsLinkLabel_OnClick(object sender, EventArgs e) =>
            Process.Start(Server.CustomScriptsUrl);
        

        private void ConfigForm_OnStatsDataGridViewValidating(object sender, DataGridViewCellValidatingEventArgs e) {
            if (e.ColumnIndex < 4 || e.ColumnIndex > 7) return;
            DataGridView.OnValidatingDataGridViewCellNumeric(sender, e);
        }

        private void showAdvancedOptionsButton_Click(object sender, EventArgs e) {
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
            
            statsDataGridView.Columns[4].Visible =
                statsDataGridView.Columns[5].Visible = 
                statsDataGridView.Columns[6].Visible = 
                statsDataGridView.Columns[7].Visible = !statsDataGridView.Columns[7].Visible;
        }

        private void autoShutdownComboBox_SelectedIndexChanged(object sender, EventArgs e) =>
            AutoShutdownDelay = (int) autoShutdownComboBox.SelectedValue;

        private int changes;
        private void customResizeRatioNumericUpDown_ValueChanged(object sender, EventArgs e) {
            userSettingsConfigManager.CustomResizeMultiplier = customResizeRatioNumericUpDown.Value;
            changes++;
            if (SetupForm.Visible) {
                Task.Run(async () => {
                    await Task.Delay(600);
                    if (--changes <= 0)
                        SetupForm.RefreshStats();
                });
            }
        }
        
        private void LoadPresetsToComboBox() {
            var presets = userSettingsConfigManager.Presets.Presets;
            
            presetsComboBox.DataSource =
                presets.Select(preset => preset.Name)
                    .Prepend("None")
                    .ToArray();
        }

        private void presetsComboBox_SelectedIndexChanged(object sender, EventArgs e) {
            throw new NotImplementedException();
        }

        private void addPresetButton_Click(object sender, EventArgs e) {
            throw new NotImplementedException();
        }

        private void deletePresetButton_Click(object sender, EventArgs e) {
            throw new NotImplementedException();
        }
    }
}

