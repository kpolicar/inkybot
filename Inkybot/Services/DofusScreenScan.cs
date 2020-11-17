using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ImageMagick;
using Inkybot.Events;
using Inkybot.Contracts;
using Inkybot.Domain;
using Inkybot.Exceptions;
using Inkybot.Helpers;
using Inkybot.Services;
using Tesseract;
using Debug = System.Diagnostics.Debug;
using Enumerable = Inkybot.Helpers.Enumerable;
using ImageFormat = System.Drawing.Imaging.ImageFormat;

namespace Inkybot.Services
{
    public partial class ScreenReaderDataProvider
    {
        public class DofusScreenScan
        {
            private static ScreenCapture screen;

            private static ScreenScanner historyScanner;
            private static ScreenScanner shortHistoryScanner;
            private static ScreenScanner statValuesScanner;
            private static ScreenScanner statMinsScanner;
            private static ScreenScanner statMaxesScanner;
            private static ScreenScanner runeScanner;

            private static CultureInfo lang;
            private readonly IntPtr handle;
            private readonly Image screenshot;
            private bool saveToDisk;


            public DofusScreenScan(Image image, bool saveToDisk = false) {
                Init();
                this.screenshot = image;
            }

            public DofusScreenScan(IntPtr hwnd, bool saveToDisk = false) {
                Init();
                handle = hwnd;
                screenshot = TakeScreenshot();
                this.saveToDisk = saveToDisk;

                if (saveToDisk) {
                    var folderPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                                     @"/debug/images/";
                    Directory.CreateDirectory(folderPath);
                    screenshot.Save(folderPath + Path.GetRandomFileName() + ".png");
                }
            }

            private void Init() {
                //if (lang != null && lang.Equals(Program.Lang)) return;

                lang = Program.Lang;
                screen = (ScreenCapture) Program.Services.GetService(typeof(ScreenCapture));

                historyScanner = new TextScreenScanner(Measurements.HistoryBounds, SplitHistoryTextLines,
                    new ResizeImagePreprocessor(200));
                shortHistoryScanner = new TextScreenScanner(Measurements.ShortHistoryBounds, SplitHistoryTextLines,
                    new ResizeImagePreprocessor(200));
                statValuesScanner = new TextScreenScanner(Measurements.StatValuesBounds, SplitStatTextLines,
                    new ResizeImagePreprocessor(150));
                statMinsScanner = new NumberScreenScanner(Measurements.StatMinBounds, SplitStatTextLines,
                    new ResizeImagePreprocessor(300));
                statMaxesScanner = new NumberScreenScanner(Measurements.StatMaxBounds, SplitStatTextLines,
                    new ResizeImagePreprocessor(300));
                runeScanner =
                    new PositiveNumberScreenScanner(null, null, new RuneImagePreprocessor(), PageSegMode.SingleChar);

                historyScanner.PageProcessed += OnHistoryPageProcessed;
            }

            private string[] SplitHistoryTextLines(string text) {
                return Regex.Split(text, Regex.Unescape(Properties.Regex.HistorySplitPattern))
                    .Where(s => s != string.Empty)
                    .Select(result => result.Replace("\n", " "))
                    .ToArray();
            }

            private string[] SplitStatTextLines(string text) {
                return text.Split(new[] {"\n"}, StringSplitOptions.RemoveEmptyEntries);
            }

            public async Task<string[]> Stats() {
                var statValuesScanTask = statValuesScanner.ScanRegionAsync(screenshot, saveToDisk);
                var statMinScanTask = statMinsScanner.ScanRegionAsync(screenshot, saveToDisk);
                var statMaxScanTask = statMaxesScanner.ScanRegionAsync(screenshot, saveToDisk);

                await Task.WhenAll(statValuesScanTask, statMinScanTask, statMaxScanTask);

                var statValues = await statValuesScanTask;
                var statMins = await statMinScanTask;
                var statMaxes = await statMaxScanTask;

                var stats = statValues
                    .ZipWithDefault(statMaxes, (value, max) => (max ?? "-") + " " + value)
                    .ZipWithDefault(statMins, (valuemax, min) => (min ?? "-") + " " + valuemax)
                    .ToArray();

                return stats;

            }

            public async Task<RuneQuantityScan> RuneQuantity(int column, int row) {
                var runeBounds = Measurements.RuneBoxBounds(column, row);
                runeScanner.SetRegion(runeBounds);
                var scanned = runeScanner.ScanRegionAsync(screenshot).Result;
                var result = scanned.First();
                
                int runeQuantity;
                var hasRune = int.TryParse(result, out runeQuantity);
                runeQuantity = hasRune ? runeQuantity : 0;
                
                return new RuneQuantityScan {
                    Column = column,
                    Row = row,
                    Quantity = runeQuantity,
                };
            }

            public async Task<RuneQuantityScan[]> RunesQuantities() {
                var runeBoxes = Measurements.RuneBoundsIndividualMeasurements;

                var scanIndex = 0;
                return runeBoxes.Select(runeBox => {
                    runeScanner.SetRegion(runeBox);
                    var scanned = runeScanner.ScanRegionAsync(screenshot).Result;
                    var result = scanned.First();

                    int runeQuantity;
                    var hasRune = int.TryParse(result, out runeQuantity);
                    runeQuantity = hasRune ? runeQuantity : 0;
                    
                    var scan = new RuneQuantityScan {
                        Column = scanIndex / 14,
                        Row = scanIndex % 14,
                        Quantity = runeQuantity,
                    };
                    
                    scanIndex++;
                    return scan;
                }).ToArray();
            }

            public async Task<string[]> History() {
                return await historyScanner.ScanRegionAsync(screenshot, saveToDisk);
            }

            private void OnHistoryPageProcessed(object sender, TesseractPageProcessed e) {
                var page = e.Page;
                var region = page.GetSegmentedRegions(0).FirstOrDefault();
                if (region == default) return;

                Debug.WriteLine("Segmented history region: " + region);
            }

            public Image TakeScreenshot() {
                //times = times >= 3 ? times : ++times;
                var bitmap = screen.CaptureWindow(handle);

                return bitmap;
            }
        }
    }
}
