using System;
using System.Windows.Forms;
using Inkybot.Controls;
using Inkybot.Helpers;
using Inkybot.Services;
using Tesseract;
using MageQueueItem = Inkybot.Services.MageQueueManager.MageQueueItem;
using Rectangle = System.Drawing.Rectangle;

namespace Inkybot.Actions
{
    public class SelectEnqueuedItem : InputAction
    {
        private readonly Responsive.Measurement itemBoundingBox;

        public SelectEnqueuedItem(MageQueueItem mageQueueItem, Control targetControl) : base(targetControl) {
            itemBoundingBox = mageQueueItem.ItemBoundingBox;
        }
        
        public override void Execute() {
            var r = itemBoundingBox.Rectangle;
            var (x, y) = (
                r.X1 + r.Width / 2,
                r.Y1 + r.Height / 2);
            var boundingBoxCenter = new Responsive.Measurement {
                Height = itemBoundingBox.Height,
                Width = itemBoundingBox.Width,
                Rectangle = Rect.FromCoords(x, y, x, y)
            };
            var itemInventoryPosition = GetCursorTarget(boundingBoxCenter);
            Input.DoubleClick(itemInventoryPosition.X, itemInventoryPosition.Y);
        }

    }
}
