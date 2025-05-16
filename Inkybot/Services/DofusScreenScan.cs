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
using Enumerable = System.Linq.Enumerable;
using UserSettings = Inkybot.Properties.Settings;

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
            private static MinMaxScreenScanner? statMinsScanner;
            private static MinMaxScreenScanner? statMaxesScanner;
            private static ScreenScanner? sinkScanner;
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
                    sinkScanner!.Saved += (_, e) => Saved?.Invoke(this, e);
                }

                this.saveToDisk = saveToDisk;
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
                        new ResizeImagePreprocessor(350));
                    latestHistoryScanner = new TextScreenScanner(Measurements.HistoryBounds, SplitHistoryTextLines,
                        new ResizeImagePreprocessor(350));
                    statValuesScanner = new TextScreenScanner(Measurements.StatValuesBounds, SplitStatTextLines,
                        new StatValuesImagePreprocessor(userSettings, 350), PageSegMode.SparseText);
                    statMinsScanner = new MinMaxScreenScanner(Measurements.StatMinBounds, SplitStatTextLines,
                        new MinMaxImagePreprocessor(userSettings, 350), PageSegMode.SingleLine);
                    statMaxesScanner = new MinMaxScreenScanner(Measurements.StatMaxBounds, SplitStatTextLines,
                        new MinMaxImagePreprocessor(userSettings, 350), PageSegMode.SingleLine);
                    runeScanner =
                        new PositiveNumberScreenScanner(default, null, new RuneImagePreprocessor(), PageSegMode.SingleChar);
                    averageItemPriceScanner =
                        new KamasScanner(Measurements.InventoryAverageItemValueBounds, null,
                            new ResizeImagePreprocessor(350), PageSegMode.SingleWord);
                    sinkScanner = new SinkScanner(Program.Lang.TwoLetterISOLanguageName == "fr" ? Measurements.SinkFrMeasurement : Measurements.SinkMeasurement, SplitStatTextLines,
                        new SinkScannerImagePreprocessor(userSettings, 350), PageSegMode.SingleLine);
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
                return Regex.Split(text, "[\r\n]+").Where(s => s!=String.Empty).ToArray();
            }

            public async Task<string[]> MinMaxStats() {

                var minstask = statMinsScanner!.ScanRegionAsync(screenshot, screenshotHeight, saveToDisk);
                var maxesTask = statMaxesScanner!.ScanRegionAsync(screenshot, screenshotHeight, saveToDisk);
                var valuesTask = Stats();

                Task.WaitAll(minstask, maxesTask, valuesTask);

                var mins = await minstask;
                var maxes = await maxesTask;
                var values = await valuesTask;
                
                for (int i = 0; i < Math.Min(values.Length, Math.Min(mins.Length, maxes.Length)); i++) {
                    Debug.Write(mins[i] + " ");
                    Debug.Write(maxes[i] + " ");
                    Debug.WriteLine(values[i]);
                }

                mins = ResizeArrayLeft(mins, values.Length, "-");
                maxes = ResizeArrayLeft(maxes, values.Length, "-");

                // Postprocess OCR result, fix OCR % misread
                for (int i = 0; i < Math.Min(mins.Length, maxes.Length); i++) {
                    if (mins[i].Contains('%') || maxes[i].Contains('%') || values[i].Contains('%')) {
                        var suc1 = int.TryParse(mins[i], NumberStyles.Any, CultureInfo.InvariantCulture, out var min);
                        var suc2 = int.TryParse(maxes[i], NumberStyles.Any, CultureInfo.InvariantCulture, out var max);
                        
                        if (suc1 && suc2) {
                            if (min > max || min > 20) {
                                // min must've interpreted the % as a number
                                mins[i] = mins[i][0] + "%";
                            }
                            if (max > 20) {
                                // max must've interpreted the % as a number
                                maxes[i] = maxes[i][0] + "%";
                            }
                            if (!mins[i].Contains("%")) mins[i] += "%";
                            if (!maxes[i].Contains("%")) maxes[i] += "%";
                        }
                    }
                    Debug.Write(mins[i] + " ");
                    Debug.Write(maxes[i] + " ");
                    Debug.WriteLine(values[i]);
                }
                
                var result = mins.ZipWithDefault(maxes, (min, valuemax) => (min ?? "-") + " " + valuemax)
                    .ToArray();
                
                return result;
            }
            
            
            static string[] ResizeArrayLeft(string[] originalArray, int targetLength, string defaultValue)
            {
                return Enumerable
                    .Repeat(defaultValue, Math.Max(0, targetLength - originalArray.Length)) // Padding on the left
                    .Concat(originalArray.Take(targetLength))                              // Add original elements, truncate if needed
                    .ToArray();
            }

            public async Task<string[]> Stats() {
                var statValuesScanTask = statValuesScanner!.ScanRegionAsync(screenshot, screenshotHeight, saveToDisk);
                return (await statValuesScanTask).Select(s => {
                    if (s.StartsWith("O ") || s.StartsWith("o ")) {
                        return "0" + s.TrimStart(new[] { 'O', 'o' }); // fix misreads of 0 with an O
                    }

                    return s;
                }).ToArray();
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

            public async Task<decimal?> Sink() {
                var scanned = await sinkScanner!.ScanRegionAsync(screenshot, screenshotHeight, saveToDisk);
                var result = scanned.FirstOrDefault()?.Replace(",", ".") ?? ""; // some dofus seem to have "," separator instead of dot
                var sinkResult = Regex.Match(result, @"(\d*\.?\d+)", RegexOptions.RightToLeft).Groups[1].Value;
                if (sinkResult.StartsWith("."))
                    sinkResult = Regex.Match(result, @"(\d+)", RegexOptions.RightToLeft).Groups[1].Value;
                
                var succ = decimal.TryParse(sinkResult, NumberStyles.Any, CultureInfo.InvariantCulture, out var sink);
                return succ ? sink : null;
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
