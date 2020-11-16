using System;
using System.Drawing;
using Tesseract;

namespace Inkybot.Events
{
    public class TesseractPageProcessed : EventArgs
    {
        public readonly Image Image;
        public readonly Page Page;


        public TesseractPageProcessed(Image image, Page page) {
            this.Image = image;
            this.Page = page;
        }
    }
}
