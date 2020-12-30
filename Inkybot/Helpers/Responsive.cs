using System.Drawing;
using Tesseract;

namespace Inkybot.Helpers
{
    public static class Responsive
    {
        
        const double perfectRatio = 0.8;

        public struct RatioRect
        {
            public double x;
            public double y;
            public double width;
            public double height;
            
            public RatioRect(double x, double y, double width, double height) {
                this.x = x;
                this.y = y;
                this.width = width;
                this.height = height;
            }
        }

        public struct Measurement
        {
            public Rect Rectangle;
            public int Width;
            public int Height;
        }

        public static Rectangle ResponsiveRectangle(Measurement measurement, int width, int height) {
            var r = measurement.Rectangle;
            var statsRect = PointsToRatioRect(r.X1, r.Y1, r.X2, r.Y2, measurement.Width, measurement.Height);
            
            return RatioRectToScreenRect(statsRect, width, height);
        }

        private static RatioRect PointsToRatioRect(int p1x, int p1y, int p2x, int p2y, int w, int h) {
            
            var ratio = 1f*h/w;
            
            double gameW = w;
            var xOffset = 0d;
            if (ratio < perfectRatio) {
                gameW = h / perfectRatio;
                xOffset = (w - gameW) / 2;
                
                p1x -= (int) xOffset;
                p2x -= (int) xOffset;
            }
            
            double gameH = h;
            var yOffset = 0d;
            if (ratio > perfectRatio) {
                gameH = w * perfectRatio;
                yOffset = (h - gameH) / 2;
                
                p1y -= (int) yOffset;
                p2y -= (int) yOffset;
            }
            
            var p1x_ratio = 1d * p1x / gameW;
            var p1y_ratio = 1d * p1y / gameH;
            var rw = p2x - p1x;
            var rh = p2y - p1y;
            
            var rw_ratio = 1d * rw / gameW;
            var rh_ratio = 1d * rh / gameH;
            
            return new RatioRect(
                p1x_ratio,
                p1y_ratio,
                rw_ratio,
                rh_ratio
            );
        }

        // Convert a ratio rect to the rectangle that will be drawn on screen (responsive)
        private static Rectangle RatioRectToScreenRect(RatioRect bounds, int width, int height) {
            var w = 1d * width;
            var h = 1d * height;
            var ratio = h/w;
            
            var gameH = h;
            var yOffset = 0d;
            if (ratio > perfectRatio) {
                gameH = perfectRatio * w;
                yOffset = (h - gameH) / 2;
            }
            
            var gameW = w;
            var xOffset = 0d;
            if (ratio < perfectRatio) {
                gameW = h / perfectRatio;
                xOffset = (w - gameW) / 2;
            }


            return new Rectangle(
                (int) (bounds.x*gameW + xOffset), 
                (int) (bounds.y*gameH + yOffset),
                (int) (bounds.width*gameW),
                (int) (bounds.height*gameH));
        }
    }
}
