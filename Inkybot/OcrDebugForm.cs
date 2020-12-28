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
            dataProvider.FetchData(image, false);

            var item = dataProvider.Item();
            foreach (var itemStat in item.Stats) {
                Debug.WriteLine(itemStat);
            }
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

