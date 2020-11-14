using System.ComponentModel.Design;
using Inkybot.Contracts;

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
            var config =  (ConfigManager) Program.Services.GetService(typeof(ConfigManager));
            
            magingJob.Started += (sender, args) => 
                MagingLogger.Info("Maging started");
            magingJob.Stopped += (sender, args) => 
                MagingLogger.Info("Maging stopped");
            magingJob.Finished += (sender, args) => 
                MagingLogger.Info("Maging finished");
            magingJob.Error += (sender, args) => 
                MagingLogger.Error(args.exception, "Maging error occured!");
            magingJob.SinkChanged += (sender, args) => 
                MagingLogger.Info("Sink has changed: " + args.sink);
            config.ConfigModified += (sender, args) =>
                MagingLogger.Info("Config has changed: " + args.Config);
        }
    }
}
