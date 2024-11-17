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
using Inkybot.Extensions;
using Inkybot.Helpers;
using Tesseract;
using Debug = System.Diagnostics.Debug;
using ImageFormat = System.Drawing.Imaging.ImageFormat;

namespace Inkybot.Services
{
    public class MageQueueManager : HasDependencies
    {
        private ConfigManager configManager = null!;
        private ScreenCapture screen = null!;
        
        public readonly List<MageQueueItem> Queue = new List<MageQueueItem>();
        private ActionFactory actionFactory;
        private ActionHandler actions;

        public event EventHandler<MeasurementEventArgs>? Enqueueing;
        public event EventHandler<MageQueueMovedEventArgs>? Enqueued;
        public event EventHandler<MageQueueEventArgs>? Head;
        public event EventHandler<MageQueueMovedEventArgs>? Dequeued;
        public event EventHandler<MageQueueMovedEventArgs>? Moved;
        public event EventHandler<MageQueueMovedEventArgs>? Removed;

        public bool Empty => Queue.Count == 0;
        public bool Full => Count < Max;
        public int Max => 9 * 5;
        public int Count => Queue.Count;
        
        
        public void BindDependencies(ServiceContainer serviceContainer) {
            configManager = (ConfigManager) serviceContainer.GetService<MageConfigManager>();
            screen = serviceContainer.GetService<ScreenCapture>();
            actionFactory = serviceContainer.GetService<ActionFactory>();
            actions = serviceContainer.GetService<ActionHandler>();
        }

        public MageQueueItem ApplyHead() {
            var mage = Queue[0];
            mage.Config.ApplyToConfigManager(configManager);
            
            Head?.Invoke(this, new MageQueueEventArgs(mage));
            return mage;
        }

        public MageQueueItem Dequeue() {
            var mage = Queue[0];
            Queue.RemoveAt(0);
            
            Dequeued?.Invoke(this, new MageQueueMovedEventArgs(mage, 0));
            return mage;
        }

        public MageQueueItem Move(MageQueueItem mage, int newIndex) {
            var index = Queue.IndexOf(mage);
            if (index == newIndex)
                return mage;
            Queue.Move(index, newIndex);
            
            Moved?.Invoke(this, new MageQueueMovedEventArgs(mage, newIndex));
            return mage;
        }

        public MageQueueItem MoveForward(MageQueueItem mage) {
            var index = Queue.IndexOf(mage);
            return Move(mage, Math.Max(0, index-1));
        }

        public MageQueueItem MoveBack(MageQueueItem mage) {
            var index = Queue.IndexOf(mage);
            return Move(mage, Math.Min(Count-1, index+1));
        }


        public MageQueueItem Enqueue(EnqueueRectangle control, Responsive.Measurement itemBoundingBox) {
            Enqueueing?.Invoke(this, new MeasurementEventArgs(itemBoundingBox));
            
            var config = new QueuedConfigProvider(configManager.StatConfig.Config());

            Image image = null!;
            
            control.Invoke(new MethodInvoker(() => {
                control.Visible = false;
                control.Refresh();
            }));

            if (configManager.UserSettings.EnableSafeMageQueueing) {
                actions.Execute(actionFactory.InventorySelectAllAction(), true);
                Thread.Sleep(300);
                actions.Execute(actionFactory.InventorySelectEquipmentAction(), true);
                Thread.Sleep(300);
            } else {
                Thread.Sleep(50);
            }
            
            image = CapturePreviewImageOfItem(itemBoundingBox);
            
            control.Invoke(new MethodInvoker(() => {
                control.Visible = true;
            }));
            
            var enqueued = new MageQueueItem(config, image, itemBoundingBox, control);
            Queue.Add(enqueued);
            
            Enqueued?.Invoke(this, new MageQueueMovedEventArgs(enqueued, Queue.Count-1));
            return enqueued;
        }

        public void Remove(EnqueueRectangle control) => Remove(
            Queue.Find(item => item.Control.Equals(control)));

        public void Remove(MageQueueItem mage) {
            var index = Queue.IndexOf(mage);
            Queue.Remove(mage);
            Removed?.Invoke(this, new MageQueueMovedEventArgs(mage, index));
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
            newImage.Crop(new MagickGeometry(target.X, target.Y, (uint)target.Width, (uint)target.Height));
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
