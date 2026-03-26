using System;
using Inkybot.Actions;
using Inkybot.Contracts;
using Inkybot.Dofus.Contracts;
using Inkybot.Dofus.Domain;
using Inkybot.Domain;
using Inkybot.Exceptions;
using DofusMagingJob = Inkybot.Contracts.DofusMagingJob;

namespace Inkybot.Services
{
    public class FileEventLogger
    {
        private static NLog.Logger OcrLogger = NLog.LogManager.GetLogger("ocr");
        private static NLog.Logger MagingLogger = NLog.LogManager.GetLogger("mage");
        public static NLog.Logger CustomLogger = NLog.LogManager.GetLogger("custom");
        public static NLog.Logger SystemLogger = NLog.LogManager.GetLogger("system");
        
        
        public void BindToServices() {
            BindToScreenReaderDataProvider();
            BindToMagingJob();
            BindToMagingAIServiceManager();
        }

        private void BindToMagingAIServiceManager() {
            var aiServiceManager =  Program.Services.GetService<MagingAIServiceManager>();
            if (aiServiceManager == null)
                return;
            
            aiServiceManager.MagingAIChanged += (sender, args) =>
                MagingLogger.Info("Maging AI updated: "+FormatMagingAI(args.AI));
        }

        private void BindToScreenReaderDataProvider() {
            var dataProvider =  (ScreenReaderDataProvider) Program.Services.GetService(typeof(DofusDataProvider));
            if (dataProvider == null)
                return;
            
            dataProvider.ScannedStats += (sender, args) =>
                OcrLogger.Info("Stats scanned:\r\n" + string.Join("\r\n", args.Lines)+"\r\n");
            dataProvider.ScannedHistory += (sender, args) =>
                OcrLogger.Info("History scanned:\r\n" + string.Join("\r\n", args.Lines)+"\r\n");
        }

        private void BindToMagingJob() {
            var magingJob =  (DofusMagingJob) Program.Services.GetService(typeof(DofusMagingJob));
            var screenReaderMagingJob = (ScreenReaderDofusMagingJob) magingJob;
            var actionHandler = (ActionHandler) Program.Services.GetService(typeof(ActionHandler));
            var config =  (ConfigManager) Program.Services.GetService(typeof(MageConfigManager));

            magingJob.Started += (sender, args) => {
                MagingLogger.Info("Maging started.");
                MetricsLogger.Track("run_started", new {
                    run_id = screenReaderMagingJob.Session.RunId
                });
            };
            magingJob.Finished += (sender, args) => {
                var session = screenReaderMagingJob.Session;
                MagingLogger.Info("Maging stopped.");
                MetricsLogger.Track("run_finished", new {
                    run_id = session.RunId,
                    reason = ResolveRunFinishedReason(session, args.AutoShutdown),
                    total_combines = session.Ticks,
                    duration_ms = session.RunDuration.ElapsedMilliseconds,
                    had_any_combine = session.Ticks > 0
                });
            };
            magingJob.Error += (sender, args) => {
                screenReaderMagingJob.Session.LastError = args.exception;
                MagingLogger.Error(args.exception, $"Maging error occured: {FormatException(args.exception)}");
                MetricsLogger.Track("run_error", new {
                    run_id = screenReaderMagingJob.Session.RunId,
                    error_type = args.exception.GetType().Name,
                    error_message = args.exception.Message
                });
            };
            magingJob.Warning += (sender, args) => {
                MagingLogger.Warn(args.exception, $"Unexpected result occured during maging: {FormatException(args.exception)}");
                MetricsLogger.Track("run_warning", new {
                    run_id = screenReaderMagingJob.Session.RunId,
                    warning_type = args.exception.GetType().Name,
                    warning_message = args.exception.Message
                });
            };
            magingJob.SinkChanged += (sender, args) =>
                MagingLogger.Info("Sink has changed: " + Math.Round(args.Sink, 2));
            magingJob.RuneQuantityChanged += (sender, args) =>
                MagingLogger.Info($"Rune quantity changed: {args.Rune}, new: {args.Quantity}, old: {args.OldQuantity}");
            config.ConfigModified += (sender, args) =>
                MagingLogger.Info("Mage config has changed.");
            config.ConfigReset += (sender, args) => {
                MagingLogger.Info("Mage config has been reset.");
                MagingLogger.Debug("New config:\n"+args.Config);
                MagingLogger.Debug("Previous config:\n"+(args.PreviousConfig?.ToString() ?? "-"));
                MagingLogger.Debug("Item:\n"+args.Item);
            };
            actionHandler.ActionExecuted += (sender, args) => {
                MagingLogger.Info("Action executed: " + FormatAction(args.action));
                if (args.action is CombineRune combine) {
                    MetricsLogger.Track("combine_executed", new {
                        run_id = screenReaderMagingJob.Session.RunId,
                        stat = combine.Rune.Stat.Identifier,
                        rune_type = combine.Rune.Type.ToString().ToLower(),
                        is_exo = combine.Exo
                    });
                } else if (args.action is Finish) {
                    MetricsLogger.Track("finish_action", new {
                        run_id = screenReaderMagingJob.Session.RunId
                    });
                }
            };
        }

        private static string ResolveRunFinishedReason(MageSession session, bool autoShutdown) {
            if (session.ManuallyStopped) return "manual_stop";
            if (session.LastError != null) return ErrorToReason(session.LastError);
            if (autoShutdown) return "auto_shutdown";
            return "finished";
        }

        private static string ErrorToReason(Exception error) {
            return error switch {
                OutOfRunesException => "out_of_runes",
                NoItemToMageFoundException => "no_item_found",
                ItemDoesNotMatchPresetException => "item_mismatch",
                ItemHasChangedException => "item_changed",
                ItemHasNotChangedException => "item_not_changed",
                OperationCanceledException => "cancelled",
                UserForbiddenException => "user_forbidden",
                _ => "error"
            };
        }

        private string FormatAction(IAction action) {
            return action switch {
                Finish a => "Finished maging",
                CombineRune a => $"Combined rune \"{a.Rune}\"",
                InventoryClearSelectionAction a => $"Cleared inventory selection",
                InventorySelectResourcesAction a => $"Selected resource category in inventory",
                _ => "Unknown action",
            };
        }

        private string FormatException(Exception exception) {
            if (exception is HistoryHasntChangedException hexception) {
                return $"{exception.Message}\nPrevious history: {hexception.PreviousItemHistory}\nCurrent history: {hexception.ItemHistory}";
            }

            return exception.Message;
        }
        
        private string FormatMagingAI(Dofus.Contracts.DofusMagingAI magingAI) {
            return magingAI switch {
                DofusMagingAI => "Maging AI",
                DofusStandardStatsMagingAI => "Free Trial Maging AI",
                CustomDofusMagingAI => "Custom Maging AI",
                _ => "Unknown Maging AI"
            };
        }
    }
}
