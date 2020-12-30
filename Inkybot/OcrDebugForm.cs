using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using ImageMagick;
using Inkybot.Adapters;
using Inkybot.Contracts;
using Inkybot.Domain;
using Inkybot.Services;
using Tesseract;

namespace Inkybot
{
    public partial class OcrDebugForm : Form
    {
        private TesseractEngine engine;
        private SymSpell spellCorrect;

        public OcrDebugForm() {
            InitializeComponent();
        }
        
        private void tesseract() {
            label1.Text = "";

            var image = Image.FromFile(@"C:\Users\Klemen\Desktop\example.png");
            var dataProvider = (ScreenReaderDataProvider) Program.Services.GetService(typeof(DofusDataProvider));
            dataProvider.FetchData(image, true);

            var item = dataProvider.Item();
            foreach (var itemStat in item.Stats) {
                Debug.WriteLine(itemStat);
            }

            dataProvider.Reset();
        }
        
        private void tesseractOn5() {
            var engine = new TesseractEngine(
                    "./Resources/Tesseract",
                    CultureInfo.CurrentUICulture.ThreeLetterISOLanguageName,
                    EngineMode.Default);
            engine.SetVariable("tessedit_char_whitelist", "-0123456789");
            var done = engine.Process(
                Pix.LoadFromFile(@"C:\Users\Klemen\Desktop\-5.bmp"),
                PageSegMode.SingleChar);
            
            Debug.WriteLine(Regex.Escape(done.GetText()));
        }
        
        private void button1_Click(object sender, EventArgs e) {
            try {
                tesseract();
            } catch (Exception ex) {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}

