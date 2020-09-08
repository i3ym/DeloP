using System.Collections.Concurrent;
using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Generic;

namespace Painter
{
    public interface ITool
    {
        float Thickness { get; set; }

        void OnStart(int x, int y, Canvas canvas);
        void OnEnd(int sx, int sy, int ex, int ey, Canvas canvas);
        void OnMove(int sx, int sy, int ex, int ey, Canvas canvas);
    }

    public abstract class DrawTool : ITool
    {
        public static readonly GraphicsOptions OptionsWithoutAA = new GraphicsOptions(false);

        protected GraphicsOptions Options = OptionsWithoutAA;
        public float Thickness { get; set; } = 1f;

        public void OnStart(int x, int y, Canvas canvas) => DrawLine(x, y, x, y, canvas);
        public void OnEnd(int sx, int sy, int ex, int ey, Canvas canvas) { }
        public void OnMove(int sx, int sy, int ex, int ey, Canvas canvas) => DrawLine(sx, sy, ex, ey, canvas);

        protected void DrawLine(int sx, int sy, int ex, int ey, Canvas canvas)
        {
            ShapeTool.DrawLine(sx, sy, ex, ey, canvas.Image, canvas.CurrentColor, Thickness);
            canvas.UpdateImage();
        }
    }
    public class PencilTool : DrawTool { }

    public abstract class ShapeTool : ITool
    {
        protected GraphicsOptions Options = DrawTool.OptionsWithoutAA;
        public float Thickness { get; set; } = 1f;
        int StartX, StartY;
        int OldEndX, OldEndY;

        public void OnStart(int x, int y, Canvas canvas) => (StartX, StartY) = (x, y);
        public void OnEnd(int sx, int sy, int ex, int ey, Canvas canvas)
        {
            Draw(StartX, StartY, OldEndX, OldEndY, canvas.OverlayImage, Color.Transparent);
            Draw(StartX, StartY, ex, ey, canvas.Image, canvas.CurrentColor);
            canvas.UpdateOverlay();
            canvas.UpdateImage();
        }

        public void OnMove(int sx, int sy, int ex, int ey, Canvas canvas)
        {
            Draw(StartX, StartY, OldEndX, OldEndY, canvas.OverlayImage, Color.Transparent);
            Draw(StartX, StartY, ex, ey, canvas.OverlayImage, canvas.CurrentColor);

            (OldEndX, OldEndY) = (ex, ey);
            canvas.UpdateOverlay();
        }

        protected virtual void Draw(int startX, int startY, int endX, int endY, Image<Rgba32> image, Rgba32 color) =>
            DrawOverride(startX, startY, endX, endY, image, color);
        protected abstract void DrawOverride(int startX, int startY, int endX, int endY, Image<Rgba32> image, Rgba32 color);


        public static void DrawLine(int startX, int startY, int endX, int endY, Image<Rgba32> image, Rgba32 color, float thickness)
        {
            if (startY == endY) DrawStraightHorizontalLine(startX, endX, startY, image, color, thickness);
            else if (startX == endX) DrawStraightVerticalLine(startX, startY, endY, image, color, thickness);
            else DrawAngledLine(startX, startY, endX, endY, image, color, thickness);
        }
        static void DrawStraightVerticalLine(int x, int startY, int endY, Image<Rgba32> image, Rgba32 color, float thickness)
        {
            (startY, endY) = (Math.Min(startY, endY), Math.Max(startY, endY));
            var ct = new Constrained(image);

            for (int y = startY - (int) thickness / 2; y <= endY + (int) thickness / 2; y++)
                for (int i = 0; i < thickness; i++)
                    ct[x + i - (int) thickness / 2, y] = color;
        }
        static void DrawStraightHorizontalLine(int startX, int endX, int y, Image<Rgba32> image, Rgba32 color, float thickness)
        {
            (startX, endX) = (Math.Min(startX, endX), Math.Max(startX, endX));
            for (int i = 0; i < thickness; i++)
            {
                var pos = y + i - (int) thickness / 2;
                if (pos >= 0 && pos < image.Height)
                    image.GetPixelRowSpan(pos).Slice(startX, endX - startX).Fill(color);
            }
        }
        static void DrawAngledLine(int startX, int startY, int endX, int endY, Image<Rgba32> image, Rgba32 color, float thickness)
        {
            var ctr = new Constrained(image);

            int x1 = endX, x0 = startX;
            int y1 = endY, y0 = startY;

            var dx = Math.Abs(x1 - x0);
            var sx = x0 < x1 ? 1 : -1;
            var dy = -Math.Abs(y1 - y0);
            var sy = y0 < y1 ? 1 : -1;
            var err = dx + dy;

            while (true)
            {
                if (thickness == 1) ctr[x0, y0] = color;
                else
                    for (int x = -(int) (thickness / 2); x < thickness / 2; x++)
                        for (int y = -(int) (thickness / 2); y < thickness / 2; y++)
                            ctr[x0 + x, y0 + y] = color;


                if (x0 == x1 && y0 == y1) break;

                var e2 = 2 * err;

                if (e2 >= dy)
                {
                    err += dy;
                    x0 += sx;
                }
                if (e2 <= dx)
                {
                    err += dx;
                    y0 += sy;
                }
            }
        }
    }
    public abstract class PolygonShapeTool : ShapeTool
    {
        protected override void Draw(int startX, int startY, int endX, int endY, Image<Rgba32> image, Rgba32 color)
        {
            (startX, endX) = (Math.Max(Math.Min(startX, endX), 0), Math.Min(Math.Max(startX, endX), image.Width));
            (startY, endY) = (Math.Max(Math.Min(startY, endY), 0), Math.Min(Math.Max(startY, endY), image.Height));

            DrawOverride(startX, startY, endX, endY, image, color);
        }
    }
    public class RectangeTool : PolygonShapeTool
    {
        protected override void DrawOverride(int startX, int startY, int endX, int endY, Image<Rgba32> image, Rgba32 color)
        {
            var ct = new Constrained(image);

            DrawLine(startX, startY, endX, startY, image, color, Thickness);
            DrawLine(startX, endY, endX, endY, image, color, Thickness);

            DrawLine(startX, startY, startX, endY, image, color, Thickness);
            DrawLine(endX, startY, endX, endY, image, color, Thickness);
        }
    }
    public class TriangleTool : PolygonShapeTool
    {
        protected override void DrawOverride(int startX, int startY, int endX, int endY, Image<Rgba32> image, Rgba32 color)
        {
            DrawLine(startX, endY, (endX - startX) / 2 + startX, startY, image, color, Thickness);
            DrawLine((endX - startX) / 2 + startX, startY, endX, endY, image, color, Thickness);
            DrawLine(startX, endY, endX, endY, image, color, Thickness);
        }
    }
    public class LineTool : ShapeTool
    {
        protected override void DrawOverride(int startX, int startY, int endX, int endY, Image<Rgba32> image, Rgba32 color) =>
            DrawLine(startX, startY, endX, endY, image, color, Thickness);
    }

    public class FillTool : ITool
    {
        public float Thickness { get; set; }

        public void OnStart(int x, int y, Canvas canvas)
        {
            Fill(x, y, canvas.Image, canvas.CurrentColor);
            canvas.UpdateImage();
        }
        public void OnEnd(int sx, int sy, int ex, int ey, Canvas canvas) { }
        public void OnMove(int sx, int sy, int ex, int ey, Canvas canvas)
        {
            Fill(ex, ey, canvas.Image, canvas.CurrentColor);
            canvas.UpdateImage();
        }


        static readonly Queue<FromPoint> CachedQueue = new Queue<FromPoint>();
        public static void Fill(int x, int y, Image<Rgba32> image, Rgba32 color)
        {
            if (image[x, y] == color) return;
            var bg = image[x, y];

            CachedQueue.Clear();
            CachedQueue.Enqueue(new FromPoint(x, y, 0, 0));

            var ct = new Constrained(image);
            while (CachedQueue.Count != 0)
            {
                var point = CachedQueue.Dequeue();
                if (ct[point.X, point.Y] != bg) continue;

                ct[point.X, point.Y] = color;
                point.AddPoints(CachedQueue);
            }
        }
        readonly struct FromPoint
        {
            public readonly int X, Y, FromX, FromY;

            public FromPoint(int x, int y, int fromX, int fromY)
            {
                X = x;
                Y = y;
                FromX = fromX;
                FromY = fromY;
            }

            public void AddPoints(Queue<FromPoint> points)
            {
                if (FromX != 1) points.Enqueue(new FromPoint(X + 1, Y, -FromX, FromY));
                if (FromX != -1) points.Enqueue(new FromPoint(X - 1, Y, -FromX, FromY));
                if (FromY != 1) points.Enqueue(new FromPoint(X, Y + 1, -FromX, FromY));
                if (FromY != -1) points.Enqueue(new FromPoint(X, Y - 1, -FromX, FromY));
            }
            public void AddPoints(ConcurrentQueue<FromPoint> points)
            {
                if (FromX != 1) points.Enqueue(new FromPoint(X + 1, Y, -FromX, FromY));
                if (FromX != -1) points.Enqueue(new FromPoint(X - 1, Y, -FromX, FromY));
                if (FromY != 1) points.Enqueue(new FromPoint(X, Y + 1, -FromX, FromY));
                if (FromY != -1) points.Enqueue(new FromPoint(X, Y - 1, -FromX, FromY));
            }
        }
    }
}