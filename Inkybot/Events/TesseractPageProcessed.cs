using System;
using System.Drawing;
using Tesseract;

namespace Inkybot.Events
{
    public class TesseractPageProcessed : EventArgs
    {
        public readonly Image Image;
        public readonly Page Page;
        public readonly string Text;


        public TesseractPageProcessed(Image image, Page page, string Text) {
            this.Image = image;
            this.Page = page;
            this.Text = Text;
        }
    }
}
