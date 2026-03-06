using System;
using static InkybotHook.NativeMethods;

namespace InkybotHook
{
    public partial class AdvancedInjectionEntryPoint
    {
        private void DrawDebugMarker()
        {
            var sp = GetFixedScreenPoint();
            int x = sp.X;
            int y = sp.Y;
            const int r = 12;
            const int arm = 20;
            IntPtr hdc = GetDC(IntPtr.Zero);
            if (hdc == IntPtr.Zero) return;
            try
            {
                SetROP2(hdc, R2_NOT);
                IntPtr pen = CreatePen(PS_SOLID, 2, 0x0000FF00);
                IntPtr oldPen = SelectObject(hdc, pen);
                IntPtr oldBrush = SelectObject(hdc, GetStockObject(5 /*NULL_BRUSH*/));

                MoveToEx(hdc, x - arm, y, IntPtr.Zero); LineTo(hdc, x + arm, y);
                MoveToEx(hdc, x, y - arm, IntPtr.Zero); LineTo(hdc, x, y + arm);

                Ellipse(hdc, x - r, y - r, x + r, y + r);

                SelectObject(hdc, oldPen);
                SelectObject(hdc, oldBrush);
                DeleteObject(pen);
            }
            finally
            {
                ReleaseDC(IntPtr.Zero, hdc);
            }
        }
    }
}
