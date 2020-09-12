using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Globalization;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;
using Windows.Storage.Streams;
using WindowsFormsApp.Contracts;

namespace WindowsFormsApp
{
    public class ScreenReaderDataProvider : DofusDataProvider
    {
        private Win32ScreenReader scanner;

        public ScreenReaderDataProvider(IntPtr hwnd) {
            scanner = new Win32ScreenReader(hwnd);
        }

        public async Task<Item.ItemStat[]> Stats() {
            var result = Program.debug ?
                await scanner.Scan(new Rectangle(745, 305, 1050-745, 760-305)) :
                await scanner.Scan(new Rectangle(740, 305, 1044-740, 840-305));

            int i = 0;
            var stats = new Item.ItemStat[result.Lines.Count];
            foreach (var line in result.Lines) {
                var valueText = Regex.Match(line.Text, @"-?\d+").Value;
                var nameText = Regex.Replace(line.Text, @"-?\d+ ?", "");

                Debug.WriteLine(valueText+" "+nameText);
                var stat = Stat.Stats.First(statData => statData.DisplayName == nameText);
                var value = int.Parse(valueText);
                stats[i++] = new Item.ItemStat(stat, value);
            }

            return stats;
        }
    }
}