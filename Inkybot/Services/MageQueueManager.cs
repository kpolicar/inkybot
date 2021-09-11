using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using ImageMagick;
using Inkybot.Contracts;
using Inkybot.Controls;
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
        
        public readonly List<MageQueueItem> Queue = new List<MageQueueItem>();

        public event EventHandler<MeasurementEventArgs>? Enqueueing;
        public event EventHandler<MageQueueEventArgs>? Enqueued;
        public event EventHandler<MageQueueEventArgs>? Dequeued;
        public event EventHandler<MageQueueEventArgs>? Removed;

        public bool Empty => Queue.Count == 0;
        public bool Full => Count < Max;
        public int Max => 9 * 5;
        public int Count => Queue.Count;
        
        
        public void BindDependencies(ServiceContainer serviceContainer) {
            configManager = (ConfigManager) serviceContainer.GetService<MageConfigManager>();
            screen = serviceContainer.GetService<ScreenCapture>();
        }

        public MageQueueItem Dequeue() {
            var mage = Queue[0];
            Queue.RemoveAt(0);
            mage.Config.ApplyToConfigManager(configManager);

            Dequeued?.Invoke(this, new MageQueueEventArgs(mage));
            return mage;
        }


        public MageQueueItem Enqueue(EnqueueRectangle control, Responsive.Measurement itemBoundingBox) {
            Enqueueing?.Invoke(this, new MeasurementEventArgs(itemBoundingBox));
            
            var config = new QueuedConfigProvider(configManager.StatConfig.Config());

            Image image = null!;
            
            control.Invoke(new MethodInvoker(() => {
                control.Visible = false;
                control.Refresh();
            }));
            
            Thread.Sleep(50);
            image = CapturePreviewImageOfItem(itemBoundingBox);
            
            control.Invoke(new MethodInvoker(() => {
                control.Visible = true;
            }));
            
            var enqueued = new MageQueueItem(config, image, itemBoundingBox, control);
            Queue.Add(enqueued);
            
            Enqueued?.Invoke(this, new MageQueueEventArgs(enqueued));
            return enqueued;
        }

        public void Remove(EnqueueRectangle control) {
            var mage = Queue.Find(item => item.Control.Equals(control));
            Queue.Remove(mage);
            Removed?.Invoke(this, new MageQueueEventArgs(mage));
        }

        public MageQueueItem Peek() =>
            Queue[0];

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
            public readonly QueuedConfigProvider Config;
            public readonly Responsive.Measurement ItemBoundingBox;
            public readonly Image? ItemPreview;
            public readonly EnqueueRectangle Control;

            public MageQueueItem(QueuedConfigProvider config, Image image, Responsive.Measurement itemBoundingBox, EnqueueRectangle control) {
                (Config, ItemPreview, ItemBoundingBox, Control) = (config, image, itemBoundingBox, control);
            }

            public void Dispose() {
                ItemPreview?.Dispose();
            }
            
        }
    }
}
