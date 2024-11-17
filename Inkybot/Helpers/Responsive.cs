using System;
using System.Drawing;
using Tesseract;

namespace Inkybot.Helpers
{
    /**
     * Usage:
     * 
     * 1. Define a measurement:
     *  public static readonly Responsive.Measurement HistoryBounds = new Responsive.Measurement {
     *      Rectangle = Rect.FromCoords(346, 127, 628, 844),
     *      Width = 1920,
     *      Height = 1017
     *  };
     * 
     * Note for Flatybot: You will want to replace your current measurements with a measurement like this, supplying
     * width=640, height=480 - or whatever client resolution you are currently locked to.
     * If you would like to define a "point", so not a rectangle, an easy solution would be to simply
     * set x1=x2 and y1=y2 in the rectangle's arguments.
     *
     * 2. Calculate the rectangle relative to the current width & height
     *  var clientRect = Responsive.ResponsiveRectangle(Measurements.HistoryBounds, client.width, client.height);
     *
     * 3. Success.
     *  win32.click(clientRect.X, clientRect.Y)
     */
    public static class Responsive
    {
        
        private static double perfectRatio => 0.6;
        // const double perfectRatio = 0.8;

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

        /**
         * <summary>Recorded measurement of a rectangle with information about the client width & height</summary>
         */
        public struct Measurement
        {
            /**
             * <summary>The rectangle that we're capturing</summary>
             */
            public Rect Rectangle;
            
            /*
             * <summary>The width of the client during capture</summary>
             */
            public int Width;
            
            /**
             * <summary>The height of the client during capture</summary>
             */
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
