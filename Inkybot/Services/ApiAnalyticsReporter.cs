using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Inkybot.Actions;
using Inkybot.Api;
using Inkybot.Contracts;
using Inkybot.Design;
using Inkybot.Dofus;
using Inkybot.Dofus.Contracts;
using Inkybot.Dofus.Domain;
using Inkybot.Events;
using Newtonsoft.Json;
using Timer = System.Windows.Forms.Timer;

namespace Inkybot.Services
{
    public class ApiAnalyticsReporter : AnalyticsReporter, HasDependencies
    {
        private class ApiAnalyticsReporterState
        {
            public int newlySpent = 0;
            public Dictionary<(Stat stat, Rune.RuneType runeType), int> attempts = new Dictionary<(Stat, Rune.RuneType), int>();
            public Dictionary<Stat, int> exoAttempts = new Dictionary<Stat, int>();
            public Dictionary<Stat, int> exoSuccesses = new Dictionary<Stat, int>();
        }
        public event EventHandler? ExoAttempt;
        private ApiAnalyticsReporterState state = new ApiAnalyticsReporterState();
        
        private ConfigManager config = null!;
        private ApiClient api = null!;
        
        private readonly object imageChangeMutex = new object();
        private Image? previousImage;
        private IAction? previousAction;
        private Action? onMagingJobConfirmedDelegate;
        private Stopwatch timeMagingStopwatch = new Stopwatch();
        private Timer sendStatisticsTimer = new Timer() {
            Interval = 20000,
            Enabled = false
        };

        public ApiAnalyticsReporter() {
            sendStatisticsTimer.Tick += OnSendStatisticsTimerTick;
        }

        public void BindDependencies(ServiceContainer serviceContainer) {
            api = serviceContainer.GetService<ApiClient>();
            
            var actionHandler = serviceContainer.GetService<ActionHandler>();
            var magus = (ScreenReaderDofusMagingJob) serviceContainer.GetService<DofusMagingJob>();
            magus.BalanceSpent += OnBalanceSpent;
            magus.Starting += (_, _) => {
                timeMagingStopwatch.Start();
                sendStatisticsTimer.Start();
            };
            magus.Started += (_, _) => onMagingJobConfirmedDelegate = null;
            magus.SuccessfulCombineTick += (_, _) => {
                onMagingJobConfirmedDelegate?.Invoke();
                onMagingJobConfirmedDelegate = null;
            };
            magus.Finished += OnMagingFinished;

            ScreenReaderDataProvider.DofusScreenScan.Screenshot += OnMagingScreenshot;
            actionHandler.ActionExecuted += OnMagingAction;
            
            config = (ConfigManager) Program.Services.GetService<MageConfigManager>();
        }

        private void OnMagingFinished(object sender, MagingJobFinishedEventArgs e) {
            Send();
            timeMagingStopwatch.Stop();
            sendStatisticsTimer.Stop();
        }
        
        private void OnSendStatisticsTimerTick(object sender, EventArgs e) =>
            Send();

        private void OnMagingScreenshot(object sender, ImageEventArgs e) {
            lock (imageChangeMutex)
            lock (e.Image) {
                previousImage?.Dispose();
                previousImage = (Image?) e.Image.Clone();
            }
        }

        private void OnMagingAction(object sender, ActionExecutedEventArgs e) {
            if (e.action is CombineRune) {
                onMagingJobConfirmedDelegate = () => OnConfirmedMagingAction(sender, e);
            } else {
                OnConfirmedMagingAction(sender, e);
            }
        }

        private void OnConfirmedMagingAction(object sender, ActionExecutedEventArgs e) {
            if (e.action is Finish finish &&
                previousAction is CombineRune previousCombine &&
                previousCombine.Exo &&
                (finish.Item.Stats.ExoStats.Any(itemStat => itemStat.Stat == previousCombine.Rune.Stat)
                 || finish.LastHistoryRecord?.Landed?.stat == previousCombine.Rune.Stat)
                )
            {
                
                var stat = previousCombine.Rune.Stat;
                lock (state) {
                    if (state.exoSuccesses.ContainsKey(stat))
                        state.exoSuccesses[stat] += 1;
                    else
                        state.exoSuccesses[stat] = 1;
                }
                
                if (stat.Config.HighSinkStat)
                    Publish();
            }

            if (e.action is CombineRune combine) {
                var stat = combine.Rune.Stat;
                var statRuneType = (stat, combine.Rune.Type);

                lock (state) {
                    if (!combine.Exo) {
                        if (state.attempts.ContainsKey(statRuneType))
                            state.attempts[statRuneType] += 1;
                        else
                            state.attempts[statRuneType] = 1;
                    } else {
                        if (state.exoAttempts.ContainsKey(stat))
                            state.exoAttempts[stat] += 1;
                        else
                            state.exoAttempts[stat] = 1;
                        if (stat.Config.HighSinkStat)
                            ExoAttempt?.Invoke(this, EventArgs.Empty);
                    }
                }
            }
            
            previousAction = e.action;
        }

        private void OnBalanceSpent(object sender, BalanceChangedEventArgs e) {
            lock (state) {
                state.newlySpent += e.Balance - e.OldBalance;
            }
        }
        
        private void Publish() {
            if (previousImage == null)
                return;
            lock (imageChangeMutex) {
                api.Publish(previousImage, config.UserSettings.PublishExos).Wait();
            }
        }

        private void Send() {
            Dictionary<string, string> data;
            
            lock (state) {
                var attemptsByIdentifier =
                    state.attempts
                        .GroupBy(pair => pair.Key.stat.Identifier)
                        .ToDictionary(
                            pairs => pairs.Key, 
                            pairs => pairs.ToDictionary(
                                pair => pair.Key.runeType.ToString().ToLower(), pair => pair.Value));
                var exoAttemptsByIdentifier =
                    state.exoAttempts.Select(pair => new KeyValuePair<string, int>(pair.Key.Identifier, pair.Value))
                        .ToDictionary(x => x.Key, x => x.Value);
                var exoSuccessesByIdentifier =
                    state.exoSuccesses.Select(pair => new KeyValuePair<string, int>(pair.Key.Identifier, pair.Value))
                        .ToDictionary(x => x.Key, x => x.Value);

                if (state.newlySpent == 0
                    && attemptsByIdentifier.Count == 0
                    && exoAttemptsByIdentifier.Count == 0
                    && exoSuccessesByIdentifier.Count == 0)
                    return;
            
                data = new Dictionary<string, string> {
                    {"expend", state.newlySpent.ToString() },
                    {"time_maging", timeMagingStopwatch.ElapsedMilliseconds.ToString() },
                    {"expended_enabled", config.UserSettings.EnableKamasCalculation.ToString() },
                    {"attempts", JsonConvert.SerializeObject(attemptsByIdentifier) },
                    {"attempts_exo", JsonConvert.SerializeObject(exoAttemptsByIdentifier) },
                    {"successes_exo", JsonConvert.SerializeObject(exoSuccessesByIdentifier) },
                };
                state = new ApiAnalyticsReporterState();
            }
            
            timeMagingStopwatch.Restart();
            _ = api.SendStatistics(data); 
        }
    }
}
