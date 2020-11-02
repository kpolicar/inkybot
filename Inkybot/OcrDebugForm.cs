using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Tesseract;

namespace Inkybot
{
    public partial class OcrDebugForm : Form
    {
        private TesseractEngine engine;

        public OcrDebugForm() {
            InitializeComponent();
            
            engine = new TesseractEngine(
                "./tessdata",
                "eng",
                EngineMode.TesseractOnly,
                new []{ "./tessdata/config" }, new Dictionary<string, object> {
                    {"tessedit_char_whitelist", "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789%-,+() "},
                    {"tessedit_enable_dict_correction", true}
                }, false);
        }

        private void button1_Click(object sender, EventArgs e) {
            var fileDialogResult = openFileDialog1.ShowDialog();

            if (fileDialogResult != DialogResult.OK) return;
            
            var path = openFileDialog1.FileName;

            label1.Text = "";
            using (var image = Image.FromFile(path)) {
                var result = engine.Process((Bitmap) image, PageSegMode.SingleBlock);

                using (var iter = result.GetIterator()) {
                    iter.Begin();

                    var text = "" + iter.GetText(PageIteratorLevel.TextLine);
                    label1.Text += result.GetText();
                }

                result.Dispose();
            }
        }
    }
}

