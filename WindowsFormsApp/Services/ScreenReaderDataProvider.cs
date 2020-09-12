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

namespace WindowsFormsApp
{
    public class ScreenReaderDataProvider : IDofusDataProvider
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
                var data = line.Text.Split(new [] { ' ' }, 2);

                Debug.Write(data[1]);
                var stat = Stat.Stats.First(statData => statData.DisplayName == data[1]);
                var value = int.Parse(Regex.Match(data[0], @"-?\d+").Value);
                stats[i++] = new Item.ItemStat(stat, value);
            }

            return stats;
        }
    }
}