using System;
using System.Drawing;
using Inkybot.Dofus;

namespace Inkybot.Events
{
    public class ImageEventArgs : EventArgs
    {
        public readonly Image Image;


        public ImageEventArgs(Image image) =>
            Image = image;
    }
}
