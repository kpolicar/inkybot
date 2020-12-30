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
using Inkybot.Design;
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
        public class DofusScreenScan : IDisposable
        {
            private Rectangle latestHistoryLastTextLineBounds;
            public Responsive.Measurement LatestHistoryBounds;
            
            private ScreenCapture screen;

            private static ScreenScanner historyScanner;
            private static ScreenScanner latestHistoryScanner;
            private static ScreenScanner statValuesScanner;
            private static ScreenScanner statMinsScanner;
            private static ScreenScanner statMaxesScanner;
            private static ScreenScanner runeScanner;
            private static ScreenScanner averageItemPriceScanner;

            private static CultureInfo lang;
            private readonly IntPtr handle;
            private readonly Image screenshot;
            private bool saveToDisk;


            private DofusScreenScan(
                ServiceContainer serviceContainer,
                Responsive.Measurement latestHistoryBounds = null,
                bool saveToDisk = false) {
                
                Init();
                screen = serviceContainer.GetService<ScreenCapture>();
                LatestHistoryBounds = latestHistoryBounds ?? Measurements.HistoryBounds;
                
                this.saveToDisk = saveToDisk;
                if (saveToDisk)
                    Save();
            }

            public DofusScreenScan(
                Image image,
                ServiceContainer serviceContainer,
                Responsive.Measurement latestHistoryBounds = null,
                bool saveToDisk = false) : this(serviceContainer, latestHistoryBounds, saveToDisk) {
                
                screenshot = image;
            }

            public DofusScreenScan(
                IntPtr hwnd,
                ServiceContainer serviceContainer,
                Responsive.Measurement latestHistoryBounds = null,
                bool saveToDisk = false) : this(serviceContainer, latestHistoryBounds, saveToDisk) {
                
                handle = hwnd;
                screenshot = TakeScreenshot();
            }

            public void Save() {
                var folderPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                                 @"/debug/images/";
                Directory.CreateDirectory(folderPath);
                screenshot.Save(folderPath + $@"/{DateTime.Now.Ticks}.png");
            }

            private void Init() {
                if (lang != null && lang.Equals(Program.Lang))
                    return;

                lang = CultureInfo.CurrentUICulture;

                historyScanner = new TextScreenScanner(Measurements.HistoryBounds, SplitHistoryTextLines,
                    new ResizeImagePreprocessor(200));
                latestHistoryScanner = new TextScreenScanner(Measurements.HistoryBounds, SplitHistoryTextLines,
                    new ResizeImagePreprocessor(200));
                statValuesScanner = new TextScreenScanner(Measurements.StatValuesBounds, SplitStatTextLines,
                    new ResizeImagePreprocessor(150));
                statMinsScanner = new NumberScreenScanner(Measurements.StatMinBounds, SplitStatTextLines,
                    new ResizeAndBinarizationImagePreprocessor(300));
                statMaxesScanner = new NumberScreenScanner(Measurements.StatMaxBounds, SplitStatTextLines,
                    new ResizeAndBinarizationImagePreprocessor(300));
                runeScanner =
                    new PositiveNumberScreenScanner(null, null, new RuneImagePreprocessor(), PageSegMode.SingleChar);
                averageItemPriceScanner =
                    new KamasScanner(Measurements.InventoryAverageItemValueBounds, null,
                        new ResizeImagePreprocessor(300), PageSegMode.SingleWord);

                latestHistoryScanner.PageProcessed += OnLatestHistoryPageProcessed;
            }

            private void OnLatestHistoryPageProcessed(object sender, TesseractPageProcessed e) {
                if (e.Page.GetText() == string.Empty)
                    return;
                latestHistoryLastTextLineBounds = e.Page.GetSegmentedRegions(PageIteratorLevel.TextLine).LastOrDefault();
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

            public async Task<string[]> MinMaxStats() {

                var minstask = statMinsScanner.ScanRegionAsync(screenshot, saveToDisk);
                var maxesTask = statMaxesScanner.ScanRegionAsync(screenshot, saveToDisk);

                Task.WaitAll(minstask, maxesTask);

                var mins = await minstask;
                var maxes = await maxesTask;

                return mins.ZipWithDefault(maxes, (min, valuemax) => (min ?? "-") + " " + valuemax)
                    .ToArray();
            }

            public async Task<string[]> Stats() {
                var statValuesScanTask = statValuesScanner.ScanRegionAsync(screenshot, saveToDisk);
                statValuesScanTask.Wait();
                return await statValuesScanTask;
            }

            public async Task<RuneQuantityScan> RuneQuantity(int column, int row) {
                var runeBounds = Measurements.RuneBoxBounds(column, row);
                runeScanner.SetRegion(runeBounds);
                var scanned = await runeScanner.ScanRegionAsync(screenshot);
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
                        Column = scanIndex / 13,
                        Row = scanIndex % 13,
                        Quantity = runeQuantity,
                    };
                    
                    scanIndex++;
                    return scan;
                }).ToArray();
            }

            public async Task<string[]> History() {
                return await historyScanner.ScanRegionAsync(screenshot, saveToDisk);
            }

            public async Task<int?> AverageItemBalance() {
                var scanned = await averageItemPriceScanner.ScanRegionAsync(screenshot);
                var result = scanned.First();

                var success = int.TryParse(result
                    .Replace(",", "")
                    .Replace("k", ""), out var balance);

                return success ? balance : (int?) null;
            }

            public async Task<string[]> LatestHistory() {
                latestHistoryScanner.SetRegion(LatestHistoryBounds);
                return await latestHistoryScanner.ScanRegionAsync(screenshot, saveToDisk);
            }

            public Responsive.Measurement CalculateNextHistoryBounds() {
                var region = latestHistoryLastTextLineBounds;
                if (region == default)
                    return LatestHistoryBounds;
                
                var bounds =
                    Responsive.ResponsiveRectangle(Measurements.ShortHistoryBounds, screenshot.Width, screenshot.Height);
                var historyBounds =
                    Responsive.ResponsiveRectangle(Measurements.HistoryBounds, screenshot.Width, screenshot.Height);
                var latestHistoryBounds =
                    Responsive.ResponsiveRectangle(LatestHistoryBounds, screenshot.Width, screenshot.Height);
                
                var maxY = (historyBounds.Y+historyBounds.Height) - bounds.Height;
                var lastY = latestHistoryBounds.Y + (region.Bottom / 2);

                var y1 = lastY < maxY ? lastY : maxY;
                
                return new Responsive.Measurement {
                    Rectangle = new Rect(bounds.X, y1, bounds.Width, bounds.Height),
                    Width = screenshot.Width,
                    Height = screenshot.Height,
                };
            }

            private Image TakeScreenshot() {
                //times = times >= 3 ? times : ++times;
                var bitmap = screen.CaptureWindow(handle);

                return bitmap;
            }

            public void Dispose() {
                screenshot?.Dispose();
            }
        }
    }
}
