using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Inkybot.Contracts;
using Inkybot.Design;
using Inkybot.Dofus;
using Inkybot.Events;
using Inkybot.Helpers;
using Inkybot.Resources;
using Debug = System.Diagnostics.Debug;
using StatConfig = Inkybot.Dofus.StatConfig;

namespace Inkybot.Services
{
    public class FileSystemUserSettingsConfigManager : UserSettingsConfigManager
    {
        public event EventHandler? EnableMageQueueingChanged;
        public event EventHandler? PresetsChanged;
        public event EventHandler? ConfigPresetsChanged;
        public event EventHandler<ConfigPresetEventArgs>? AppliedPreset;
        public event EventHandler? ResetFinished;

        public bool RestoreHighSinkStats {
            set {
                Properties.Settings.Default.restoreHighSinkStatImmediately = value;
                Properties.Settings.Default.Save();
            }
            get => Properties.Settings.Default.restoreHighSinkStatImmediately;
        }

        public decimal CustomResizeMultiplier {
            set {
                Properties.Settings.Default.customResizeRatio = value;
                Properties.Settings.Default.Save();
            }
            get => Properties.Settings.Default.customResizeRatio;
        }

        public bool AutoRestartBot {
            set {
                Properties.Settings.Default.autoRestartBot = value;
                Properties.Settings.Default.Save();
            }
            get => Properties.Settings.Default.autoRestartBot;
        }

        public bool ShowUserWarnings {
            set {
                Properties.Settings.Default.showUserWarnings = value;
                Properties.Settings.Default.Save();
            }
            get => Properties.Settings.Default.showUserWarnings;
        }

        public bool EnableKamasCalculation {
            set {
                Properties.Settings.Default.kamasCalculation = value;
                Properties.Settings.Default.Save();
            }
            get => Properties.Settings.Default.kamasCalculation;
        }

        public bool PublishExos {
            set {
                Properties.Settings.Default.publishExos = value;
                Properties.Settings.Default.Save();
            }
            get => Properties.Settings.Default.publishExos;
        }

        public bool EnableMageQueueing {
            set {
                Properties.Settings.Default.enableMageQueueing = value;
                Properties.Settings.Default.Save();
                EnableMageQueueingChanged?.Invoke(this, EventArgs.Empty);
            }
            get => Properties.Settings.Default.enableMageQueueing;
        }

        public bool EnableRuneChecking {
            set {
                Properties.Settings.Default.enableRuneChecking = value;
                Properties.Settings.Default.Save();
            }
            get => Properties.Settings.Default.enableRuneChecking;
        }

        public ItemPresets Presets {
            set {
                Properties.Settings.Default.presets = value;
                Properties.Settings.Default.Save();
                PresetsChanged?.Invoke(this, EventArgs.Empty);
            }
            get => Properties.Settings.Default.presets;
        }

        public ConfigPresets ConfigPresets {
            set {
                Properties.Settings.Default.configPresets = value;
                Properties.Settings.Default.Save();
                ConfigPresetsChanged?.Invoke(this, EventArgs.Empty);
            }
            get => Properties.Settings.Default.configPresets;
        }
        
        public StatConfig Config(Stat stat) {
            var config = (Inkybot.Resources.StatConfig) Properties.Settings.Default[stat.Identifier];
            return new StatConfig(
                Numbers.Parse(config.MaxValueAtWhichSmRuneCanLand),
                Numbers.Parse(config.ChangeToPaRuneThreshold),
                Numbers.Parse(config.MaxValueAtWhichPaRuneCanLand),
                Numbers.Parse(config.ChangeToRaRuneThreshold), 
                config.UseSmRunes, 
                config.UsePaRunes, 
                config.UseRaRunes, 
                DefaultStatConfigProvider.Instance.Config(stat).HighSinkStat
            );
        }

        public Dictionary<Stat, StatConfig> Config() {
            return Stat.Stats
                .ToDictionary(
                    pair => pair.Value, 
                    pair => Config(pair.Value));
        }

        public void SetConfig(Stat stat, in StatConfig config) =>
            SetConfig(stat, config, true);

        public void ApplyConfigPreset(int index) {
            var preset = ConfigPresets.Presets
                .Skip(index).First();
            
            Reset(false);
            
            foreach (var statConfigPreset in preset.Configs) {
                var stat = Stat.FirstOrNew(statConfigPreset.Stat);
                SetConfig(stat, new StatConfig(
                    Numbers.Parse(statConfigPreset.MaxValueAtWhichSmRuneCanLand),
                    Numbers.Parse(statConfigPreset.ChangeToPaRuneThreshold),
                    Numbers.Parse(statConfigPreset.MaxValueAtWhichPaRuneCanLand),
                    Numbers.Parse(statConfigPreset.ChangeToRaRuneThreshold),
                    statConfigPreset.UseSmRunes,
                    statConfigPreset.UsePaRunes,
                    statConfigPreset.UseRaRunes,
                    DefaultStatConfigProvider.Instance.Config(stat).HighSinkStat
                ));
            }
            Properties.Settings.Default.Save();
            AppliedPreset?.Invoke(this, new ConfigPresetEventArgs(preset, index));
        }

        public void Reset() =>
            Reset(true);

        public void Reset(bool save) {
            foreach (var defaultStatConfig in DefaultStatConfigProvider.Instance.Config()) {
                SetConfig(defaultStatConfig.Key, defaultStatConfig.Value, false);
            }
            if (save)
                Properties.Settings.Default.Save();
            ResetFinished?.Invoke(this, EventArgs.Empty);
        }

        public void SetConfig(Stat stat, in StatConfig config, bool save) {
            Properties.Settings.Default[stat.Identifier] = new Resources.StatConfig {
                ChangeToPaRuneThreshold = Numbers.ToString(config.ChangeToPaRuneThreshold),
                ChangeToRaRuneThreshold = Numbers.ToString(config.ChangeToRaRuneThreshold),
                MaxValueAtWhichPaRuneCanLand = Numbers.ToString(config.MaxValueAtWhichPaRuneCanHit),
                MaxValueAtWhichSmRuneCanLand = Numbers.ToString(config.MaxValueAtWhichSmRuneCanHit),
                UseSmRunes = config.UseSmRunes,
                UsePaRunes = config.UsePaRunes,
                UseRaRunes = config.UseRaRunes,
            };
            if (save)
                Properties.Settings.Default.Save();
        }
    }
}
