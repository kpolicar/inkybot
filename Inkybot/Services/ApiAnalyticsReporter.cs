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

namespace Inkybot.Services
{
    public class ApiAnalyticsReporter : AnalyticsReporter, HasDependencies
    {
        private ConfigManager config = null!;
        private ApiClient api = null!;
        private int changesCount = 0;
        const int MinChangesToSendCount = 10;
        private const int MaxReasonableBalanceDifference = 300000;
        
        private int balanceDifference = 0;
        private Dictionary<(Stat stat, Rune.RuneType runeType), int> attempts = new Dictionary<(Stat, Rune.RuneType), int>();
        private Dictionary<Stat, int> exoAttempts = new Dictionary<Stat, int>();
        private Dictionary<Stat, int> exoSuccesses = new Dictionary<Stat, int>();
        private readonly object imageChangeMutex = new object();
        private Image? previousImage;
        private IAction? previousAction;
        private Action? onMagingJobConfirmedDelegate;
        private Stopwatch timeMagingStopwatch = new Stopwatch();

        public void BindDependencies(ServiceContainer serviceContainer) {
            api = serviceContainer.GetService<ApiClient>();
            
            var actionHandler = serviceContainer.GetService<ActionHandler>();
            var magus = (ScreenReaderDofusMagingJob) serviceContainer.GetService<DofusMagingJob>();
            magus.BalanceChanged += OnBalanceChanged;
            magus.Starting += (_, _) => timeMagingStopwatch.Start();
            magus.Started += (_, _) => onMagingJobConfirmedDelegate = null;;
            magus.SuccessfulCombineTick += (_, _) => {
                onMagingJobConfirmedDelegate?.Invoke();
                onMagingJobConfirmedDelegate = null;
            };
            magus.Stopped += (_, _) => {
                Send();
                timeMagingStopwatch.Stop();
            };

            ScreenReaderDataProvider.DofusScreenScan.Screenshot += OnMagingScreenshot;
            actionHandler.ActionExecuted += OnMagingAction;
            
            config = (ConfigManager) Program.Services.GetService<MageConfigManager>();
        }

        private void OnMagingScreenshot(object sender, ImageEventArgs e) {
            lock (imageChangeMutex)
            lock (e.Image) {
                previousImage?.Dispose();
                var ms = new MemoryStream();
                e.Image.Save(ms, ImageFormat.Bmp);
                previousImage = Image.FromStream(ms);
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
                if (exoSuccesses.ContainsKey(stat))
                    exoSuccesses[stat] += 1;
                else
                    exoSuccesses[stat] = 1;
                
                if (stat.Config.HighSinkStat)
                    Publish();
            }

            if (e.action is CombineRune combine) {
                var stat = combine.Rune.Stat;
                var statRuneType = (stat, combine.Rune.Type);
                
                if (!combine.Exo) {
                    if (attempts.ContainsKey(statRuneType))
                        attempts[statRuneType] += 1;
                    else
                        attempts[statRuneType] = 1;
                } else {
                    if (exoAttempts.ContainsKey(stat))
                        exoAttempts[stat] += 1;
                    else
                        exoAttempts[stat] = 1;
                }
            }
            
            previousAction = e.action;
        }

        private void OnBalanceChanged(object sender, BalanceChangedEventArgs e) {
            changesCount++;
            balanceDifference += e.OldBalance - e.Balance;
            balanceDifference = Math.Max(balanceDifference, 0);
            if (balanceDifference > MaxReasonableBalanceDifference)
                balanceDifference = 0;

            if (changesCount >= MinChangesToSendCount)
                Send();
        }
        
        private void Publish() {
            if (previousImage == null)
                return;
            lock (imageChangeMutex) {
                api.Publish(previousImage, config.UserSettings.PublishExos).Wait();
            }
        }

        private void Send() {
            var attemptsByIdentifier =
                attempts
                    .GroupBy(pair => pair.Key.stat.Identifier)
                    .ToDictionary(
                        pairs => pairs.Key, 
                        pairs => pairs.ToDictionary(
                            pair => pair.Key.runeType.ToString().ToLower(), pair => pair.Value));
            var exoAttemptsByIdentifier =
                exoAttempts.Select(pair => new KeyValuePair<string, int>(pair.Key.Identifier, pair.Value))
                    .ToDictionary(x => x.Key, x => x.Value);
            var exoSuccessesByIdentifier =
                exoSuccesses.Select(pair => new KeyValuePair<string, int>(pair.Key.Identifier, pair.Value))
                    .ToDictionary(x => x.Key, x => x.Value);

            if (balanceDifference == 0
                && attemptsByIdentifier.Count == 0
                && exoAttemptsByIdentifier.Count == 0
                && exoSuccessesByIdentifier.Count == 0)
                return;
            
            var data = new Dictionary<string, string> {
                {"expend", balanceDifference.ToString() },
                {"time_maging", timeMagingStopwatch.Elapsed.Seconds.ToString() },
                {"expended_enabled", config.UserSettings.EnableKamasCalculation.ToString() },
                {"attempts", JsonConvert.SerializeObject(attemptsByIdentifier) },
                {"attempts_exo", JsonConvert.SerializeObject(exoAttemptsByIdentifier) },
                {"successes_exo", JsonConvert.SerializeObject(exoSuccessesByIdentifier) },
            };
            timeMagingStopwatch.Restart();
            changesCount = 0;
            balanceDifference = 0;
            exoSuccesses = new Dictionary<Stat, int>();
            exoAttempts = new Dictionary<Stat, int>();
            attempts = new Dictionary<(Stat,Rune.RuneType), int>();
            _ = api.SendStatistics(data);
        }
    }
}
