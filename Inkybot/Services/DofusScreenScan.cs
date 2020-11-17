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
using Inkybot.Exceptions;
using Inkybot.Helpers;
using Inkybot.Services;
using Tesseract;
using Debug = System.Diagnostics.Debug;
using ImageFormat = System.Drawing.Imaging.ImageFormat;

namespace Inkybot
{
    public class DofusScreenScan
    {
        public static readonly Responsive.Measurement HistoryBoundsMeasurement = new Responsive.Measurement {
            Rectangle = Rect.FromCoords(346, 117, 590, 844),
            Width = 1920,
            Height = 1017
        };

        public static readonly Responsive.Measurement ShortHistoryBoundsMeasurement = new Responsive.Measurement {
            Rectangle = Rect.FromCoords(346, 752, 590, 842),
            Width = 1920,
            Height = 1017
        };

        public static readonly Responsive.Measurement StatValuesBoundsMeasurement = new Responsive.Measurement {
            Rectangle = Rect.FromCoords(745, 307, 973, 842),
            Width = 1920,
            Height = 1017
        };

        public static readonly Responsive.Measurement StatMinBoundsMeasurement = new Responsive.Measurement {
            Rectangle = Rect.FromCoords(645, 307, 695, 842),
            Width = 1920,
            Height = 1017
        };
        
        public static readonly Responsive.Measurement StatMaxBoundsMeasurement = new Responsive.Measurement {
            Rectangle = Rect.FromCoords(695, 307, 745, 842),
            Width = 1920,
            Height = 1017
        };

        public static Responsive.Measurement[] StatMinBoundsIndividualLineMeasurements =>
            SplitStatLineMeasurementsIntoIndividualLineMeasurements(StatMinBoundsMeasurement);
        
        public static Responsive.Measurement[] StatMaxBoundsIndividualLineMeasurements =>
            SplitStatLineMeasurementsIntoIndividualLineMeasurements(StatMaxBoundsMeasurement);

        public static Responsive.Measurement[] SplitStatLineMeasurementsIntoIndividualLineMeasurements(Responsive.Measurement measurement) {
            var b = measurement.Rectangle;
            var n = 14;

            var measurements = new Responsive.Measurement[n];
            for (int i = 0; i < n; ++i) {
                var smallerRect = new Rect(b.X1, b.Y1 + (int) (1f * b.Height / n * i), b.Width, b.Height / n);
                var m = new Responsive.Measurement {
                    Rectangle = smallerRect,
                    Height = measurement.Height,
                    Width = measurement.Width
                };
                measurements[i] = m;
            }

            return measurements;
        }
        
        private static ScreenCapture screen;
        
        private static ScreenScanner historyScanner;
        private static ScreenScanner shortHistoryScanner;
        private static ScreenScanner statValuesScanner;
        private static ScreenScanner statMinsScanner;
        private static ScreenScanner statMaxesScanner;

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
                var folderPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)+@"/debug/images/";
                Directory.CreateDirectory(folderPath);
                screenshot.Save(folderPath+Path.GetRandomFileName()+".png");
            }
        }

        private void Init() {
            //if (lang != null && lang.Equals(Program.Lang)) return;
            
            lang = Program.Lang;
            screen = (ScreenCapture) Program.Services.GetService(typeof(ScreenCapture));
            
            historyScanner = new TextScreenScanner(HistoryBoundsMeasurement, SplitHistoryTextLines, new ResizeImagePreprocessor(200));
            shortHistoryScanner = new TextScreenScanner(ShortHistoryBoundsMeasurement, SplitHistoryTextLines, new ResizeImagePreprocessor(200));
            statValuesScanner = new TextScreenScanner(StatValuesBoundsMeasurement, SplitStatTextLines, new ResizeImagePreprocessor(150));
            statMinsScanner = new NumberScreenScanner(StatMinBoundsMeasurement, SplitStatTextLines, new ResizeImagePreprocessor(300));
            statMaxesScanner = new NumberScreenScanner(StatMaxBoundsMeasurement, SplitStatTextLines, new ResizeImagePreprocessor(300));

            historyScanner.PageProcessed += OnHistoryPageProcessed;
        }

        private string[] SplitHistoryTextLines(string text) {
            return Regex.Split(text, Regex.Unescape(Properties.Regex.HistorySplitPattern))
                .Where(s => s != string.Empty)
                .Select(result => result.Replace("\n", " "))
                .ToArray();
        }

        private string[] SplitStatTextLines(string text) {
            return text.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
        }

        public async Task<string[]> Stats() {
            var statValuesScanTask = statValuesScanner.ScanRegionAsync(screenshot, saveToDisk);
            var statMinScanTask = statMinsScanner.ScanRegionAsync(screenshot, saveToDisk);
            var statMaxScanTask = statMaxesScanner.ScanRegionAsync(screenshot, saveToDisk);

            await Task.WhenAll(statValuesScanTask, statMinScanTask, statMaxScanTask);
            
            var statValues =  await statValuesScanTask;
            var statMins = await statMinScanTask;
            var statMaxes = await statMaxScanTask;
            
            return statMins.Zip(statMaxes, (s1, s2) => s1 + " " + s2).Zip(statValues, (s1, s2) => s1 + " " + s2).ToArray();
        }

        public async Task<string[]> History() {
            return await historyScanner.ScanRegionAsync(screenshot, saveToDisk);
        }

        private void OnHistoryPageProcessed(object sender, TesseractPageProcessed e) {
            var page = e.Page;
            var region = page.GetSegmentedRegions(0).FirstOrDefault();
            if (region == default) return;
            
            Debug.WriteLine("Segmented history region: "+region);
        }

        public Image TakeScreenshot() {
            //times = times >= 3 ? times : ++times;
            var bitmap = screen.CaptureWindow(handle);

            return bitmap;
        }
    }
}
