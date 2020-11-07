using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using ImageMagick;
using Inkybot.Adapters;
using Tesseract;

namespace Inkybot
{
    public partial class OcrDebugForm : Form
    {
        private TesseractEngine engine;
        private SymSpell spellCorrect;

        public OcrDebugForm() {
            InitializeComponent();
            
            engine = new TesseractEngine(
                "./Resources/Tesseract",
                "eng",
                EngineMode.TesseractOnly,
                null, new Dictionary<string, object> {
                    {"tessedit_char_whitelist", "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789%-,+() "}
                }, false);
            
            
            
            spellCorrect = new SymSpell(16, 6);
            
            Dictionary.LoadInto(spellCorrect);
        }
        
        private void tesseract() {
            
            var fileDialogResult = openFileDialog1.ShowDialog();

            if (fileDialogResult != DialogResult.OK) return;
            
            var path = openFileDialog1.FileName;

            pictureBox1.Image = null;
            using (var image = Image.FromFile(path)) {
                    
                using (var ms = new MemoryStream()) {
                    image.Save(ms, image.RawFormat);
                    ms.Position = 0;
                    
                    using (var img = new MagickImage(ms))
                    {
                        // Resize each image in the collection to a width of 200. When zero is specified for the height
                        // the height will be calculated with the aspect ratio.
                        //img.ColorSpace = ColorSpace.Gray;
                        //img.Resize(new Percentage(300));
                        //img.BlackThreshold(new Percentage(35));
                        //img.WhiteThreshold(new Percentage(35));
                        
                        img.Resize(new Percentage(300));
                        img.ColorSpace = ColorSpace.Gray;

                        img.Sharpen();
                        img.BlackThreshold(new Percentage(35));
                        img.WhiteThreshold(new Percentage(35));


                        img.Write(ms);
                        var outImage = pictureBox1.Image = Image.FromStream(ms);
                        ocr(outImage);

                        // Save the result
                    }
                }
            }
        }
        
        public void ocr(Image image) {
            Stopwatch timer = Stopwatch.StartNew();
            var result = engine.Process((Bitmap) image, PageSegMode.SingleBlock);

            ocrResultsLabel.Text = "";
            using (var iter = result.GetIterator()) {
                iter.Begin();

                var text = "" + iter.GetText(PageIteratorLevel.TextLine);
                ocrResultsLabel.Text += result.GetText();
            }
                
            timer.Stop();
            TimeSpan timespan = timer.Elapsed;

            var timeelapsed = String.Format("{0:00}:{1:00}:{2:00}", timespan.Minutes, timespan.Seconds, timespan.Milliseconds / 10);
            timerLabel.Text = timeelapsed;

            result.Dispose();
        }

        private void button1_Click(object sender, EventArgs e) {
            Stopwatch timer = Stopwatch.StartNew();
            
            tesseract();
            
            timer.Stop();
            TimeSpan timespan = timer.Elapsed;
            var timeelapsed = String.Format("{0:00}:{1:00}:{2:00}", timespan.Minutes, timespan.Seconds, timespan.Milliseconds / 10);
            timerLabel.Text += "\n"+timeelapsed;
            //ironocr();
        }

        private void textBox1_TextChanged(object sender, EventArgs e) {
            var input = textBox1.Text;
            var term = spellCorrect.Lookup(input, SymSpell.Verbosity.Closest).FirstOrDefault();

            string outp;
            if (term != null)
                outp = term.term;
            else
                outp = "null";

            label1.Text = outp;
        }
    }
}

