using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Inkybot.Events;
using Inkybot.Contracts;
using Inkybot.Design;
using Inkybot.Helpers;
using Tesseract;
using Debug = System.Diagnostics.Debug;

namespace Inkybot.Services
{
    public partial class ScreenReaderDataProvider
    {
        public class DofusScreenScan : IDisposable
        {
            private Rectangle latestHistoryLastTextLineBounds;
            private double latestHistoryLastTextLineBoundsRatio;
            public Responsive.Measurement LatestHistoryBounds;
            
            private ScreenCapture screen;

            private static ScreenScanner? historyScanner;
            private static ScreenScanner? latestHistoryScanner;
            private static ScreenScanner? statValuesScanner;
            private static ScreenScanner? statMinsScanner;
            private static ScreenScanner? statMaxesScanner;
            private static ScreenScanner? runeScanner;
            private static ScreenScanner? averageItemPriceScanner;

            private static CultureInfo? lang;

            private int screenshotHeight;
            public Image _screenshot = null!;
            public Image screenshot {
                private set {
                    _screenshot = value;
                    screenshotHeight = value.Height;
                }
                get => _screenshot;
            }
            private bool saveToDisk;
            private UserSettingsConfigManager userSettings;
            public static event EventHandler<ImageEventArgs>? Screenshot;
            public event EventHandler<FileSystemEventArgs>? Saved;


            private DofusScreenScan(
                ServiceContainer serviceContainer,
                Responsive.Measurement? latestHistoryBounds,
                bool saveToDisk = false) {
                
                screen = serviceContainer.GetService<ScreenCapture>();
                userSettings = serviceContainer.GetService<UserSettingsConfigManager>();
                Init();
                LatestHistoryBounds = latestHistoryBounds ?? Measurements.HistoryBounds;

                if (saveToDisk) {
                    historyScanner!.Saved += (_, e) => Saved?.Invoke(this, e);
                    latestHistoryScanner!.Saved += (_, e) => Saved?.Invoke(this, e);
                    statValuesScanner!.Saved += (_, e) => Saved?.Invoke(this, e);
                    statMinsScanner!.Saved += (_, e) => Saved?.Invoke(this, e);
                    statMaxesScanner!.Saved += (_, e) => Saved?.Invoke(this, e);
                    runeScanner!.Saved += (_, e) => Saved?.Invoke(this, e);
                    averageItemPriceScanner!.Saved += (_, e) => Saved?.Invoke(this, e);
                }

                this.saveToDisk = saveToDisk;
                this.saveToDisk = true; // todo temp
            }

            public DofusScreenScan(
                Image image,
                ServiceContainer serviceContainer,
                Responsive.Measurement latestHistoryBounds,
                bool saveToDisk = false) : this(serviceContainer, latestHistoryBounds, saveToDisk) {
                
                screenshot = image;
                if (saveToDisk)
                    Save();
            }

            public DofusScreenScan(
                ServiceContainer serviceContainer,
                Responsive.Measurement latestHistoryBounds,
                bool saveToDisk,
                bool deferredScreenshot) : this(serviceContainer, latestHistoryBounds, saveToDisk) {
                
                if (deferredScreenshot)
                    return;
                CaptureScreenshot();
            }

            public void CaptureScreenshot() {
                if (screenshot != null)
                    throw new ApplicationException("Screenshot has already been taken!");
                screenshot = TakeScreenshot();

                foreach (var minmax in Stats().Result) {
                    Debug.WriteLine(minmax);
                }

                if (saveToDisk)
                    Save();
            }

            public void Save() {
                var folderPath = Path.Combine(AppContext.BaseDirectory, @"debug\images");
                Directory.CreateDirectory(folderPath);
                var fileName = $"{DateTime.Now.Ticks}.png";
                screenshot.Save(folderPath + $@"/{fileName}");
                Saved?.Invoke(this, new FileSystemEventArgs(
                    WatcherChangeTypes.Created, folderPath, fileName));
            }

            private void Init() {
                if (lang == null || !lang.Equals(CultureInfo.CurrentUICulture)) {
                    lang = CultureInfo.CurrentUICulture;

                    historyScanner = new TextScreenScanner(Measurements.HistoryBounds, SplitHistoryTextLines,
                        new ResizeImagePreprocessor(200));
                    latestHistoryScanner = new TextScreenScanner(Measurements.HistoryBounds, SplitHistoryTextLines,
                        new ResizeImagePreprocessor(200));
                    statValuesScanner = new TextScreenScanner(Measurements.StatValuesBounds, SplitStatTextLines,
                        new StatValuesImagePreprocessor(userSettings, 150));
                    statMinsScanner = new NumberScreenScanner(Measurements.StatMinBounds, SplitStatTextLines,
                        new ResizeAndBinarizationImagePreprocessor(userSettings, 300));
                    statMaxesScanner = new NumberScreenScanner(Measurements.StatMaxBounds, SplitStatTextLines,
                        new ResizeAndBinarizationImagePreprocessor(userSettings, 300));
                    runeScanner =
                        new PositiveNumberScreenScanner(default, null, new RuneImagePreprocessor(), PageSegMode.SingleChar);
                    averageItemPriceScanner =
                        new KamasScanner(Measurements.InventoryAverageItemValueBounds, null,
                            new ResizeImagePreprocessor(300), PageSegMode.SingleWord);
                }
                
                latestHistoryScanner!.PageProcessed += OnLatestHistoryPageProcessed;
            }

            private void OnLatestHistoryPageProcessed(object sender, TesseractPageProcessed e) {
                if (e.Page.GetText() == string.Empty)
                    return;
                latestHistoryLastTextLineBounds = e.Page.GetSegmentedRegions(PageIteratorLevel.TextLine).LastOrDefault();
                latestHistoryLastTextLineBoundsRatio = (latestHistoryScanner!.preprocessor as ResizeImagePreprocessor)!.resizePercentage / 100f;
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

                var minstask = statMinsScanner!.ScanRegionAsync(screenshot, screenshotHeight, saveToDisk);
                var maxesTask = statMaxesScanner!.ScanRegionAsync(screenshot, screenshotHeight, saveToDisk);

                Task.WaitAll(minstask, maxesTask);

                var mins = await minstask;
                var maxes = await maxesTask;

                return mins.ZipWithDefault(maxes, (min, valuemax) => (min ?? "-") + " " + valuemax)
                    .ToArray();
            }

            public async Task<string[]> Stats() {
                var statValuesScanTask = statValuesScanner!.ScanRegionAsync(screenshot, screenshotHeight, saveToDisk);
                return await statValuesScanTask;
            }

            public async Task<RuneQuantityScan> RuneQuantity(int column, int row) {
                var runeBounds = Measurements.RuneBoxBounds(column, row);
                runeScanner!.SetRegion(runeBounds);
                var scanned = await runeScanner.ScanRegionAsync(screenshot, screenshotHeight);
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

            public RuneQuantityScan[] RunesQuantities() {
                var runeBoxes = Measurements.RuneBoundsIndividualMeasurements;

                var scanIndex = 0;
                return runeBoxes.Select(runeBox => {
                    runeScanner!.SetRegion(runeBox);
                    var scanned = runeScanner.ScanRegionAsync(screenshot, screenshotHeight).Result;
                    var result = scanned.FirstOrDefault() ?? "";

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
                return await historyScanner!.ScanRegionAsync(screenshot, screenshotHeight, saveToDisk);
            }

            public async Task<int?> AverageItemBalance() {
                var scanned = await averageItemPriceScanner!.ScanRegionAsync(screenshot, screenshotHeight);
                var result = scanned.First();

                var success = int.TryParse(result
                    .Replace(",", "")
                    .Replace("k", ""), out var balance);

                return success ? balance : (int?) null;
            }

            public async Task<string[]> LatestHistory() {
                latestHistoryScanner!.SetRegion(LatestHistoryBounds);
                return await latestHistoryScanner.ScanRegionAsync(screenshot, screenshotHeight, saveToDisk);
            }

            public Responsive.Measurement CalculateNextHistoryBounds() {
                var region = latestHistoryLastTextLineBounds;
                if (region == default) {
                    return LatestHistoryBounds;
                }

                var bounds =
                    Responsive.ResponsiveRectangle(Measurements.ShortHistoryBounds, screenshot.Width, screenshot.Height);
                var historyBounds =
                    Responsive.ResponsiveRectangle(Measurements.HistoryBounds, screenshot.Width, screenshot.Height);
                var latestHistoryBounds =
                    Responsive.ResponsiveRectangle(LatestHistoryBounds, screenshot.Width, screenshot.Height);
                
                var maxY = (historyBounds.Y+historyBounds.Height) - bounds.Height;
                var lastY = latestHistoryBounds.Y + (int)(region.Bottom / latestHistoryLastTextLineBoundsRatio);

                var y1 = lastY < maxY ? lastY : maxY;
                
                return new Responsive.Measurement {
                    Rectangle = new Rect(bounds.X, y1, bounds.Width, bounds.Height),
                    Width = screenshot.Width,
                    Height = screenshot.Height,
                };
            }

            private Image TakeScreenshot() {
                //times = times >= 3 ? times : ++times;
                var bitmap = screen.CaptureWindow();
                Screenshot?.Invoke(this, new ImageEventArgs(bitmap));

                return bitmap;
            }

            public void Dispose() {
                if (screenshot != null) {
                    lock (screenshot) {
                        screenshot.Dispose();
                    }
                }

                if (saveToDisk) {
                    historyScanner!.Saved -= Saved;
                    latestHistoryScanner!.Saved -= Saved;
                    statValuesScanner!.Saved -= Saved;
                    statMinsScanner!.Saved -= Saved;
                    statMaxesScanner!.Saved -= Saved;
                    runeScanner!.Saved -= Saved;
                    averageItemPriceScanner!.Saved -= Saved;
                }
                latestHistoryScanner!.PageProcessed -= OnLatestHistoryPageProcessed;
            }
        }
    }
}
