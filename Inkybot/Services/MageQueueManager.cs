using System;
using System.Collections.Generic;
using System.Drawing;
using Inkybot.Controls;
using Inkybot.Design;
using Inkybot.Dofus.Contracts;
using Inkybot.Helpers;

namespace Inkybot.Services
{
    public class MageQueueManager : HasDependencies
    {
        private ConfigManager configManager = null!;
        
        private readonly Queue<MageQueueItem> Queue = new Queue<MageQueueItem>();
        
        public event EventHandler? Enqueueing;
        public event EventHandler? Enqueued;
        public event EventHandler? Dequeued;

        public bool Empty => Queue.Count == 0;
        public bool Full => Count < Max;
        public int Max => 9 * 5;
        public int Count => Queue.Count;
        
        
        public void BindDependencies(ServiceContainer serviceContainer) {
            configManager = (ConfigManager) serviceContainer.GetService<MageConfigManager>();
        }
        
        public MageQueueItem Dequeue() {
            var mage = Queue.Dequeue();
            mage.Config.ApplyToConfigManager(configManager);
            
            Dequeued?.Invoke(this, EventArgs.Empty);
            return mage;
        }
        
        public MageQueueItem Enqueue(Responsive.Measurement itemBoundingBox) {
            Enqueueing?.Invoke(this, EventArgs.Empty);
            
            var config = new QueuedConfigProvider(configManager.StatConfig.Config());
            
            var enqueued = new MageQueueItem(config, itemBoundingBox);
            Queue.Enqueue(enqueued);
            
            Enqueued?.Invoke(this, EventArgs.Empty);
            return enqueued;
        }
        
        public class MageQueueItem : IDisposable
        {
            public QueuedConfigProvider Config { get; private set; }
            public Responsive.Measurement ItemBoundingBox { get; private set; }
            public Image? ItemPreview;

            public MageQueueItem(QueuedConfigProvider config, Responsive.Measurement itemBoundingBox) {
                (Config, ItemBoundingBox) = (config, itemBoundingBox);
            }

            public void Dispose() {
                ItemPreview?.Dispose();
            }
            
        }
    }
}
