using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Windows.Forms;
using Inkybot.Contracts;
using Inkybot.Exceptions;
using NLog;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Windows.Graphics.Capture;
using Windows.Graphics.DirectX;
using Windows.Graphics.DirectX.Direct3D11;

using Device = SharpDX.Direct3D11.Device;
using MapFlags = SharpDX.Direct3D11.MapFlags;

namespace Inkybot
{
    public class WinGraphicsCaptureScreenCapture : ScreenCapture, IDisposable
    {
        private static readonly Logger Log = LogManager.GetLogger("system");

        private IntPtr handle;
        private Form mainForm;
        private Panel dofusClientPanel;
        public event EventHandler? BeginScreenshot;
        public event EventHandler? EndScreenshot;
        private int xOffsetLeft;
        private int xOffsetRight;

        private Device d3dDevice;
        private IDirect3DDevice winrtDevice;
        private GraphicsCaptureItem captureItem;
        private Direct3D11CaptureFramePool framePool;
        private GraphicsCaptureSession session;
        private Texture2D stagingTexture;

        private readonly object frameLock = new object();
        private bool isCapturing = false;
        private volatile bool hasFirstFrame = false;
        private readonly Stopwatch frameThrottle = Stopwatch.StartNew();
        private bool _firstValidFrameLogged = false;
        private readonly Stopwatch _invalidFrameLogThrottle = Stopwatch.StartNew();

        public void BindTo(IntPtr handle, Panel dofusClientPanel, int xOffsetLeft, int xOffsetRight, Form mainForm)
        {
            this.handle = handle;
            this.dofusClientPanel = dofusClientPanel;
            this.mainForm = mainForm;
            this.xOffsetLeft = xOffsetLeft;
            this.xOffsetRight = xOffsetRight;

            if (handle == IntPtr.Zero) {
                Log.Warn("[WinGraphicsCapture] BindTo called with IntPtr.Zero — capture will not start.");
                return;
            }

            Log.Info($"[WinGraphicsCapture] Initializing for handle 0x{handle:X}...");

            d3dDevice = new Device(SharpDX.Direct3D.DriverType.Hardware, DeviceCreationFlags.BgraSupport);
            winrtDevice = CreateWinRTDevice(d3dDevice);

            captureItem = CreateCaptureItemForWindow(mainForm.Handle);
            captureItem.Closed += (s, e) => {
                Log.Warn("[WinGraphicsCapture] Capture item closed (window detached or closed).");
                isCapturing = false;
            };

            var textureDesc = new Texture2DDescription
            {
                Width = captureItem.Size.Width,
                Height = captureItem.Size.Height,
                MipLevels = 1,
                ArraySize = 1,
                Format = Format.B8G8R8A8_UNorm,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Staging,
                BindFlags = BindFlags.None,
                CpuAccessFlags = CpuAccessFlags.Read,
                OptionFlags = ResourceOptionFlags.None
            };
            stagingTexture = new Texture2D(d3dDevice, textureDesc);

            framePool = Direct3D11CaptureFramePool.CreateFreeThreaded(
                winrtDevice,
                DirectXPixelFormat.B8G8R8A8UIntNormalized,
                2,
                captureItem.Size);

            framePool.FrameArrived += OnFrameArrived;
            session = framePool.CreateCaptureSession(captureItem);
            session.IsCursorCaptureEnabled = false;
            if (Windows.Foundation.Metadata.ApiInformation.IsPropertyPresent(typeof(GraphicsCaptureSession).FullName, "IsBorderRequired"))
            {
                var borderProperty = typeof(GraphicsCaptureSession).GetProperty("IsBorderRequired");
                borderProperty?.SetValue(session, false);
            }

            session.StartCapture();
            Log.Info($"[WinGraphicsCapture] Session started. Capture item size: {captureItem.Size.Width}x{captureItem.Size.Height}.");
        }

        private void OnFrameArrived(Direct3D11CaptureFramePool sender, object args)
        {
            using (var frame = sender.TryGetNextFrame())
            {
                if (frame == null || frame.ContentSize.Width <= 0 || frame.ContentSize.Height <= 0) return;

                if (hasFirstFrame && frameThrottle.ElapsedMilliseconds < 50) return;
                frameThrottle.Restart();

                if (frame.ContentSize.Width != stagingTexture.Description.Width ||
                    frame.ContentSize.Height != stagingTexture.Description.Height)
                {
                    Log.Info($"[WinGraphicsCapture] Frame size changed to {frame.ContentSize.Width}x{frame.ContentSize.Height} — recreating staging texture.");
                    lock (frameLock)
                    {
                        hasFirstFrame = false;
                        _firstValidFrameLogged = false;
                        stagingTexture?.Dispose();
                        stagingTexture = new Texture2D(d3dDevice, new Texture2DDescription
                        {
                            Width = frame.ContentSize.Width,
                            Height = frame.ContentSize.Height,
                            MipLevels = 1, ArraySize = 1,
                            Format = Format.B8G8R8A8_UNorm,
                            SampleDescription = new SampleDescription(1, 0),
                            Usage = ResourceUsage.Staging,
                            BindFlags = BindFlags.None,
                            CpuAccessFlags = CpuAccessFlags.Read,
                        });
                        framePool.Recreate(winrtDevice, DirectXPixelFormat.B8G8R8A8UIntNormalized, 2, frame.ContentSize);
                    }
                    return;
                }

                var surfaceInterop = (IDirect3DDxgiInterfaceAccess)frame.Surface;
                var textureGuid = new Guid("6f15aaf2-d208-4e89-9ab4-489535d34f9c");
                IntPtr d3dPointer = surfaceInterop.GetInterface(ref textureGuid);

                using (var gpuTexture = new Texture2D(d3dPointer))
                {
                    lock (frameLock)
                    {
                        d3dDevice.ImmediateContext.CopyResource(gpuTexture, stagingTexture);
                        hasFirstFrame = true;
                        isCapturing = true;
                    }
                }
            }
        }

        public Image CaptureWindow()
        {
            if (!isCapturing) throw new DofusProcessDetachedException("Capture session not active.");

            BeginScreenshot?.Invoke(this, EventArgs.Empty);
            var totalSw = Stopwatch.StartNew();

            while (!hasFirstFrame && totalSw.ElapsedMilliseconds < 2000) Thread.Sleep(10);

            if (!hasFirstFrame)
                Log.Warn("[WinGraphicsCapture] Timed out waiting for first frame after 2s — capture may not be delivering frames.");

            Bitmap result;
            lock (frameLock)
            {
                var copySw = Stopwatch.StartNew();
                var dataBox = d3dDevice.ImmediateContext.MapSubresource(stagingTexture, 0, MapMode.Read, MapFlags.None);

                try
                {
                    using (var rawBmp = new Bitmap(
                        stagingTexture.Description.Width,
                        stagingTexture.Description.Height,
                        dataBox.RowPitch,
                        PixelFormat.Format32bppRgb,
                        dataBox.DataPointer))
                    {
                        Profiler.Record("Capture", "gpu_to_cpu_map", copySw.ElapsedMilliseconds);

                        if (!_firstValidFrameLogged)
                            LogPixelStats(rawBmp);

                        var cropSw = Stopwatch.StartNew();
                        int yOff = yOffset();

                        int cropX = xOffsetLeft;
                        int cropY = yOff;
                        int cropW = rawBmp.Width - xOffsetLeft - xOffsetRight;
                        int cropH = rawBmp.Height - yOff;

                        var cropRect = new Rectangle(cropX, cropY, cropW, cropH);
                        cropRect.Intersect(new Rectangle(0, 0, rawBmp.Width, rawBmp.Height));

                        result = rawBmp.Clone(cropRect, rawBmp.PixelFormat);
                        Profiler.Record("Capture", "crop", cropSw.ElapsedMilliseconds);
                    }
                }
                finally { d3dDevice.ImmediateContext.UnmapSubresource(stagingTexture, 0); }
            }

            Profiler.Record("Capture", "total", totalSw.ElapsedMilliseconds);
            EndScreenshot?.Invoke(this, EventArgs.Empty);
            return result;
        }

        private void LogPixelStats(Bitmap bmp) {
            const int gridSize = 5; // 5x5 = 25 sample points
            var lumas = new int[gridSize * gridSize];
            long sum = 0;

            for (int row = 0; row < gridSize; row++) {
                for (int col = 0; col < gridSize; col++) {
                    int px = (int)((col + 0.5f) / gridSize * bmp.Width);
                    int py = (int)((row + 0.5f) / gridSize * bmp.Height);
                    var c = bmp.GetPixel(px, py);
                    int luma = (c.R + c.G + c.B) / 3;
                    lumas[row * gridSize + col] = luma;
                    sum += luma;
                }
            }

            int total = lumas.Length;
            double mean = (double)sum / total;
            double variance = 0;
            foreach (var l in lumas) variance += (l - mean) * (l - mean);
            double stddev = Math.Sqrt(variance / total);

            const double validThreshold = 10.0;
            bool isValid = stddev >= validThreshold;

            if (isValid) {
                _firstValidFrameLogged = true;
                Log.Info($"[WinGraphicsCapture] First valid frame ({bmp.Width}x{bmp.Height}): " +
                         $"sum={sum}, mean={mean:F1}, stddev={stddev:F1} — looks like real content.");
            } else if (_invalidFrameLogThrottle.ElapsedMilliseconds >= 1000) {
                _invalidFrameLogThrottle.Restart();
                Log.Warn($"[WinGraphicsCapture] Frame content looks invalid ({bmp.Width}x{bmp.Height}): " +
                         $"sum={sum}, mean={mean:F1}, stddev={stddev:F1} (threshold={validThreshold}).");
            }
        }

        public int yOffset()
        {
            int borderHeight = 0;
            mainForm.Invoke((MethodInvoker)delegate {
                int titleBarHeight = SystemInformation.CaptionHeight;
                borderHeight = titleBarHeight + dofusClientPanel.Top;
            });
            return borderHeight;
        }

        public void Dispose()
        {
            Log.Info("[WinGraphicsCapture] Disposing.");
            isCapturing = false;
            session?.Dispose();        session = null!;
            framePool?.Dispose();      framePool = null!;
            stagingTexture?.Dispose(); stagingTexture = null!;
            d3dDevice?.Dispose();      d3dDevice = null!;
            winrtDevice?.Dispose();    winrtDevice = null!;
        }

        #region Boilerplate
        [ComImport, Guid("3628E81B-3CAC-4C60-B7F4-23CE0E0C3356"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IGraphicsCaptureItemInterop { IntPtr CreateForWindow([In] IntPtr window, [In] ref Guid iid); }

        [ComImport, Guid("A9B3D012-3DF2-4EE3-B8D1-8695F457D3C1"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IDirect3DDxgiInterfaceAccess { IntPtr GetInterface([In] ref Guid iid); }

        [DllImport("d3d11.dll")]
        private static extern uint CreateDirect3D11DeviceFromDXGIDevice(IntPtr dxgiDevice, out IntPtr graphicsDevice);

        private static GraphicsCaptureItem CreateCaptureItemForWindow(IntPtr hwnd)
        {
            var factory = WindowsRuntimeMarshal.GetActivationFactory(typeof(GraphicsCaptureItem));
            var interop = (IGraphicsCaptureItemInterop)factory;
            var iid = new Guid("79C3F95B-31F7-4EC2-A464-632EF5D30760");
            var pointer = interop.CreateForWindow(hwnd, ref iid);
            var item = (GraphicsCaptureItem)Marshal.GetObjectForIUnknown(pointer);
            Marshal.Release(pointer);
            return item;
        }

        private static IDirect3DDevice CreateWinRTDevice(Device d3dDevice)
        {
            var dxgiDevice = d3dDevice.QueryInterface<SharpDX.DXGI.Device>();
            CreateDirect3D11DeviceFromDXGIDevice(dxgiDevice.NativePointer, out IntPtr pUnknown);
            var winrtDevice = (IDirect3DDevice)Marshal.GetObjectForIUnknown(pUnknown);
            Marshal.Release(pUnknown);
            dxgiDevice.Dispose();
            return winrtDevice;
        }
        #endregion
    }
}
