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
using Tesseract;
using DofusMagingJob = Inkybot.Domain.DofusMagingJob;

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

            var image = Image.FromFile(@"C:\Users\Klemen\Desktop\eaxc.png");
            var dataProvider = (ScreenReaderDataProvider) Program.Services.GetService(typeof(DofusDataProvider));
            dataProvider.FetchData(image);
            
            var history = new DofusMagingJob().history.Analyse(dataProvider.History());
            foreach (var sad in history.history) {
                foreach (var ex in sad.fell) {
                    label1.Text += ex.stat.DisplayName + " " + ex.value + "\n";
                }
            }

            foreach (var item in dataProvider.Item().Stats) {
                Debug.WriteLine(item.stat.DisplayName +" " + item.value);
            }
        }
        
        private void button1_Click(object sender, EventArgs e) {
            timerLabel.Text = "";
            var timer = Stopwatch.StartNew();

            try {
                tesseract();
            } catch (Exception ex) {
                Debug.WriteLine(ex.Message);
            }

            timer.Stop();
            TimeSpan timespan = timer.Elapsed;
            var timeelapsed = String.Format("{0:00}:{1:00}:{2:00}", timespan.Minutes, timespan.Seconds, timespan.Milliseconds / 10);
            timerLabel.Text += "\n"+timeelapsed;
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

