using System;
using System.ComponentModel.Design;
using Inkybot.Actions;
using Inkybot.Contracts;
using Inkybot.Domain;
using Inkybot.Exceptions;
using Tesseract;
using DofusMagingJob = Inkybot.Contracts.DofusMagingJob;

namespace Inkybot.Services
{
    public class FileEventLogger
    {
        private static NLog.Logger OcrLogger = NLog.LogManager.GetLogger("ocr");
        private static NLog.Logger MagingLogger = NLog.LogManager.GetLogger("mage");
        
        
        public void BindToServices() {
            BindToScreenReaderDataProvider();
            BindToMagingJob();
        }

        private void BindToScreenReaderDataProvider() {
            var dataProvider =  (ScreenReaderDataProvider) Program.Services.GetService(typeof(DofusDataProvider));
            
            dataProvider.ScannedStats += (sender, args) =>
                OcrLogger.Info("Stats scanned:\r\n" + string.Join("\r\n", args.Lines)+"\r\n");
            dataProvider.ScannedHistory += (sender, args) =>
                OcrLogger.Info("History scanned:\r\n" + string.Join("\r\n", args.Lines)+"\r\n");
        }

        private void BindToMagingJob() {
            var magingJob =  (DofusMagingJob) Program.Services.GetService(typeof(DofusMagingJob));
            var actionHandler = (ActionHandler) Program.Services.GetService(typeof(ActionHandler));
            var config =  (ConfigManager) Program.Services.GetService(typeof(ConfigManager));
            
            magingJob.Started += (sender, args) => 
                MagingLogger.Info("Maging started.");
            magingJob.Finished += (sender, args) => 
                MagingLogger.Info("Maging stopped.");
            magingJob.Error += (sender, args) => 
                MagingLogger.Error(args.exception, $"Maging error occured: {FormatException(args.exception)}");
            magingJob.Warning += (sender, args) => 
                MagingLogger.Warn(args.exception, $"Unexpected result occured during maging: {FormatException(args.exception)}");
            magingJob.SinkChanged += (sender, args) => 
                MagingLogger.Info("Sink has changed: " + Math.Round(args.Sink, 2));
            magingJob.RuneQuantityChanged += (sender, args) => 
                MagingLogger.Info($"Rune quantity changed: {args.Rune}, new: {args.Quantity}, old: {args.OldQuantity}");
            config.ConfigModified += (sender, args) =>
                MagingLogger.Info("Mage config has changed.");
            actionHandler.ActionExecuted += (sender, args) => 
                MagingLogger.Info("Action executed: " + FormatAction(args.action));
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
    }
}
