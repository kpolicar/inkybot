using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using ImageMagick;
using Inkybot.Contracts;
using Inkybot.Design;
using Inkybot.Dofus.Contracts;
using Inkybot.Events;
using Inkybot.Helpers;
using Tesseract;
using ImageFormat = System.Drawing.Imaging.ImageFormat;

namespace Inkybot.Services
{
    public class MageQueueManager : HasDependencies
    {
        private ConfigManager configManager = null!;
        private ScreenCapture screen = null!;
        
        private readonly Queue<MageQueueItem> Queue = new Queue<MageQueueItem>();

        public event EventHandler<MeasurementEventArgs>? Enqueueing;
        public event EventHandler<MageQueueEventArgs>? Enqueued;
        public event EventHandler<MageQueueEventArgs>? Dequeued;

        public bool Empty => Queue.Count == 0;
        public bool Full => Count < Max;
        public int Max => 9 * 5;
        public int Count => Queue.Count;
        
        
        public void BindDependencies(ServiceContainer serviceContainer) {
            configManager = (ConfigManager) serviceContainer.GetService<MageConfigManager>();
            screen = serviceContainer.GetService<ScreenCapture>();
        }
        
        public MageQueueItem Dequeue() {
            var mage = Queue.Dequeue();
            mage.Config.ApplyToConfigManager(configManager);
            
            Dequeued?.Invoke(this, new MageQueueEventArgs(mage));
            return mage;
        }
        
        public MageQueueItem Enqueue(Responsive.Measurement itemBoundingBox) {
            Enqueueing?.Invoke(this, new MeasurementEventArgs(itemBoundingBox));
            
            var config = new QueuedConfigProvider(configManager.StatConfig.Config());

            var image = CapturePreviewImageOfItem(itemBoundingBox);
            
            var enqueued = new MageQueueItem(config, image, itemBoundingBox);
            Queue.Enqueue(enqueued);
            
            Enqueued?.Invoke(this, new MageQueueEventArgs(enqueued));
            return enqueued;
        }

        protected Image CapturePreviewImageOfItem(Responsive.Measurement itemBoundingBox) {
            var r = itemBoundingBox.Rectangle;
            var shrunkenItemBoundingBox = new Responsive.Measurement {
                Width = itemBoundingBox.Width,
                Height = itemBoundingBox.Height,
                Rectangle = Rect.FromCoords(r.X1+3, r.Y1+3, r.X2-3, r.Y2-3)
            };
            using var image = screen.CaptureWindow();
            using var ms = new MemoryStream();
            
            lock (image) {
                image.Save(ms, ImageFormat.Bmp);
            }
            ms.Position = 0;

            using var newImage = new MagickImage(ms);
            // Resize each image in the collection to a width of 200. When zero is specified for the height
            // the height will be calculated with the aspect ratio.
            var target = Responsive.ResponsiveRectangle(shrunkenItemBoundingBox, image.Width, image.Height);
            newImage.Crop(new MagickGeometry(target.X, target.Y, target.Width, target.Height));
            newImage.Write(ms);

            return Image.FromStream(ms);
        }
        
        public class MageQueueItem : IDisposable
        {
            public QueuedConfigProvider Config { get; private set; }
            public Responsive.Measurement ItemBoundingBox { get; private set; }
            public Image? ItemPreview;

            public MageQueueItem(QueuedConfigProvider config, Image image, Responsive.Measurement itemBoundingBox) {
                (Config, ItemPreview, ItemBoundingBox) = (config, image, itemBoundingBox);
            }

            public void Dispose() {
                ItemPreview?.Dispose();
            }
            
        }
    }
}
