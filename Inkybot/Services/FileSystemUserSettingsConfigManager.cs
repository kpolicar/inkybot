using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Inkybot.Contracts;
using Inkybot.Dofus;
using Inkybot.Helpers;
using Debug = System.Diagnostics.Debug;

namespace Inkybot.Services
{
    public class FileSystemUserSettingsConfigManager : UserSettingsConfigManager
    {
        public event EventHandler? EnableMageQueueingChanged;
        
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

        public void SetConfig(Stat stat, in StatConfig config) {
            Properties.Settings.Default[stat.Identifier] = new Resources.StatConfig {
                ChangeToPaRuneThreshold = Numbers.ToString(config.ChangeToPaRuneThreshold),
                ChangeToRaRuneThreshold = Numbers.ToString(config.ChangeToRaRuneThreshold),
                MaxValueAtWhichPaRuneCanLand = Numbers.ToString(config.MaxValueAtWhichPaRuneCanHit),
                MaxValueAtWhichSmRuneCanLand = Numbers.ToString(config.MaxValueAtWhichSmRuneCanHit),
                UseSmRunes = config.UseSmRunes,
                UsePaRunes = config.UsePaRunes,
                UseRaRunes = config.UseRaRunes,
            };
            Properties.Settings.Default.Save();
        }
    }
}
