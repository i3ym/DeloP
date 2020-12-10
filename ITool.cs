using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using DeloP.Controls;
using osu.Framework.Graphics.Primitives;
using SkiaSharp;

namespace DeloP
{
    public interface ITool
    {
        string SpriteName => GetType().Name[..^4].ToLowerInvariant();

        void OnStart(int x, int y);
        void OnEnd(int sx, int sy, int ex, int ey);
        void OnMove(int sx, int sy, int ex, int ey);

        void OnStartRight(int x, int y) { }
        void OnEndRight(int sx, int sy, int ex, int ey) { }
        void OnMoveRight(int sx, int sy, int ex, int ey) { }
    }
    public interface IThicknessTool : ITool
    {
        int Thickness { get; set; }
    }

    public abstract class Tool
    {
        protected Canvas Canvas => FullCanvas.Canvas;
        protected readonly FullCanvas FullCanvas;

        protected Tool(FullCanvas fullCanvas) => FullCanvas = fullCanvas;
    }
    public abstract class ThicknessTool : Tool
    {
        public int Thickness { get; set; } = 1;

        protected ThicknessTool(FullCanvas fullCanvas) : base(fullCanvas) { }
    }

    public class PencilTool : ThicknessTool, IThicknessTool
    {
        public PencilTool(FullCanvas fullCanvas) : base(fullCanvas) { }

        public void OnStart(int x, int y) => DrawLine(x, y, x, y, Canvas, Canvas.MainColor);
        public void OnEnd(int sx, int sy, int ex, int ey) { }
        public void OnMove(int sx, int sy, int ex, int ey) => DrawLine(sx, sy, ex, ey, Canvas, Canvas.MainColor);

        public void OnStartRight(int x, int y) => DrawLine(x, y, x, y, Canvas, Canvas.SecondaryColor);
        public void OnEndRight(int sx, int sy, int ex, int ey) { }
        public void OnMoveRight(int sx, int sy, int ex, int ey) => DrawLine(sx, sy, ex, ey, Canvas, Canvas.SecondaryColor);

        void DrawLine(int sx, int sy, int ex, int ey, Canvas Canvas, SKColor color)
        {
            ShapeTool.DrawLine(sx, sy, ex, ey, Canvas.Image.ActiveCanvas, color, Thickness);
            Canvas.Image.UpdateActiveLayer(new RectangleI(sx - Thickness, sy - Thickness, ex + Thickness, ey + Thickness));
        }
    }
    public class EraserTool : ThicknessTool, IThicknessTool
    {
        public EraserTool(FullCanvas fullCanvas) : base(fullCanvas) { }

        public void OnStart(int x, int y) => DrawLine(x, y, x, y, Canvas);
        public void OnEnd(int sx, int sy, int ex, int ey) { }
        public void OnMove(int sx, int sy, int ex, int ey) => DrawLine(sx, sy, ex, ey, Canvas);

        public void OnStartRight(int x, int y) => DrawLineReplace(x, y, x, y, Canvas);
        public void OnEndRight(int sx, int sy, int ex, int ey) { }
        public void OnMoveRight(int sx, int sy, int ex, int ey) => DrawLineReplace(sx, sy, ex, ey, Canvas);

        void DrawLine(int sx, int sy, int ex, int ey, Canvas Canvas)
        {
            ShapeTool.DrawLine(sx, sy, ex, ey, Canvas.Image.ActiveCanvas, Canvas.SecondaryColor, Thickness);
            Canvas.Image.UpdateActiveLayer(new RectangleI(sx - Thickness, sy - Thickness, ex + Thickness, ey + Thickness));
        }
        void DrawLineReplace(int sx, int sy, int ex, int ey, Canvas Canvas)
        {
            DrawLine(sx, sy, ex, ey, Canvas.Image.ActiveImage, Canvas.MainColor, Canvas.SecondaryColor, Thickness);
            Canvas.Image.UpdateActiveLayer(new RectangleI(sx - Thickness, sy - Thickness, ex + Thickness, ey + Thickness));
        }


        static void DrawLine(int startX, int startY, int endX, int endY, SKBitmap image, SKColor colorFrom, SKColor color, float thickness)
        {
            if (startY == endY) DrawStraightHorizontalLine(startX, endX, startY, image, colorFrom, color, thickness);
            else if (startX == endX) DrawStraightVerticalLine(startX, startY, endY, image, colorFrom, color, thickness);
            else DrawAngledLine(startX, startY, endX, endY, image, colorFrom, color, thickness);
        }
        static void DrawStraightVerticalLine(int x, int startY, int endY, SKBitmap image, SKColor colorFrom, SKColor color, float thickness)
        {
            (startY, endY) = (Math.Min(startY, endY), Math.Max(startY, endY));
            var ct = new Constrained(image);

            for (int y = startY - (int) thickness / 2; y <= endY + (int) thickness / 2; y++)
                for (int i = 0; i < thickness; i++)
                {
                    var xx = x + i - (int) thickness / 2;
                    if (ct[xx, y] == colorFrom)
                        ct[xx, y] = color;
                }
        }
        static void DrawStraightHorizontalLine(int startX, int endX, int y, SKBitmap image, SKColor colorFrom, SKColor color, float thickness)
        {
            var ct = new Constrained(image);

            (startX, endX) = (Math.Min(startX, endX), Math.Max(startX, endX));
            for (int i = 0; i < thickness; i++)
            {
                var pos = y + i - (int) thickness / 2;
                if (pos < 0 || pos >= image.Height) continue;

                for (int x = startX; x < endX; x++)
                    if (ct[x, pos] == colorFrom)
                        ct[x, pos] = color;
            }
        }
        static void DrawAngledLine(int startX, int startY, int endX, int endY, SKBitmap image, SKColor colorFrom, SKColor color, float thickness)
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
                            if (image.GetPixel(x0 + x, y0 + y) == colorFrom)
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

    public abstract class ShapeTool : Tool, IThicknessTool
    {
        public int Thickness { get; set; } = 1;
        int StartX, StartY;
        int OldEndX, OldEndY;

        public ShapeTool(FullCanvas fullCanvas) : base(fullCanvas) { }

        public void OnStart(int x, int y) => (StartX, StartY) = (x, y);
        public void OnEnd(int sx, int sy, int ex, int ey)
        {
            Canvas.OverlayCanvas.Clear();
            Draw(StartX, StartY, ex, ey, Canvas.Image.ActiveCanvas, Canvas.MainColor);

            Canvas.Image.UpdateOverlay();
            Canvas.Image.UpdateActiveLayer();
        }

        public void OnMove(int sx, int sy, int ex, int ey)
        {
            Canvas.OverlayCanvas.Clear();
            Draw(StartX, StartY, ex, ey, Canvas.OverlayCanvas, Canvas.MainColor);

            (OldEndX, OldEndY) = (ex, ey);
            Canvas.Image.UpdateOverlay();
        }

        protected virtual void Draw(int startX, int startY, int endX, int endY, SKCanvas image, SKColor color) =>
            DrawOverride(startX, startY, endX, endY, image, color);
        protected abstract void DrawOverride(int startX, int startY, int endX, int endY, SKCanvas image, SKColor color);


        public static void DrawLine(int startX, int startY, int endX, int endY, SKCanvas image, SKColor color, float thickness)
        {
            image.DrawLine(startX, startY, endX, endY, new SKPaint() { Color = color, IsAntialias = false, StrokeWidth = thickness });
            image.Flush();
        }
    }
    public abstract class PolygonShapeTool : ShapeTool
    {
        public PolygonShapeTool(FullCanvas fullCanvas) : base(fullCanvas) { }

        protected override void Draw(int startX, int startY, int endX, int endY, SKCanvas image, SKColor color)
        {
            // (startX, endX) = (Math.Max(Math.Min(startX, endX), 0), Math.Min(Math.Max(startX, endX), image.Width));
            // (startY, endY) = (Math.Max(Math.Min(startY, endY), 0), Math.Min(Math.Max(startY, endY), image.Height));

            DrawOverride(startX, startY, endX, endY, image, color);
        }
    }
    public class RectangleTool : PolygonShapeTool
    {
        public RectangleTool(FullCanvas fullCanvas) : base(fullCanvas) { }

        protected override void DrawOverride(int startX, int startY, int endX, int endY, SKCanvas image, SKColor color)
        {
            DrawLine(startX, startY, endX, startY, image, color, Thickness);
            DrawLine(startX, endY, endX, endY, image, color, Thickness);

            DrawLine(startX, startY, startX, endY, image, color, Thickness);
            DrawLine(endX, startY, endX, endY, image, color, Thickness);
        }
    }
    public class TriangleTool : PolygonShapeTool
    {
        public TriangleTool(FullCanvas fullCanvas) : base(fullCanvas) { }

        protected override void DrawOverride(int startX, int startY, int endX, int endY, SKCanvas image, SKColor color)
        {
            DrawLine(startX, endY, (endX - startX) / 2 + startX, startY, image, color, Thickness);
            DrawLine((endX - startX) / 2 + startX, startY, endX, endY, image, color, Thickness);
            DrawLine(startX, endY, endX, endY, image, color, Thickness);
        }
    }
    public class LineTool : ShapeTool
    {
        public LineTool(FullCanvas fullCanvas) : base(fullCanvas) { }

        protected override void DrawOverride(int startX, int startY, int endX, int endY, SKCanvas image, SKColor color) =>
            DrawLine(startX, startY, endX, endY, image, color, Thickness);
    }

    public class FillTool : Tool, ITool
    {
        public FillTool(FullCanvas fullCanvas) : base(fullCanvas) { }

        public void OnStart(int x, int y)
        {
            Fill(x, y, Canvas.Image.ActiveImage, Canvas.MainColor);
            Canvas.Image.UpdateActiveLayer();
        }
        public void OnEnd(int sx, int sy, int ex, int ey) { }
        public void OnMove(int sx, int sy, int ex, int ey)
        {
            Fill(ex, ey, Canvas.Image.ActiveImage, Canvas.MainColor);
            Canvas.Image.UpdateActiveLayer();
        }


        static readonly Queue<FromPoint> CachedQueue = new Queue<FromPoint>();
        public static void Fill(int x, int y, SKBitmap image, SKColor color)
        {
            if (image.GetPixel(x, y) == color) return;
            var bg = image.GetPixel(x, y);

            CachedQueue.Clear();
            CachedQueue.Enqueue(new FromPoint(x, y, 0, 0));

            var ct = new Constrained(image);
            while (CachedQueue.Count != 0)
            {
                var point = CachedQueue.Dequeue();
                if (ct[point.X, point.Y] != bg) continue;
                if (!ct.CheckConstraints(point.X, point.Y)) continue;

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
    public class PipetteTool : Tool, ITool
    {
        public PipetteTool(FullCanvas fullCanvas) : base(fullCanvas) { }

        public void OnStart(int x, int y) => Canvas.MainColor = Canvas.Image[x, y];
        public void OnEnd(int sx, int sy, int ex, int ey) { }
        public void OnMove(int sx, int sy, int ex, int ey) { }

        public void OnStartRight(int x, int y) => Canvas.SecondaryColor = Canvas.Image[x, y];
        public void OnEndRight(int sx, int sy, int ex, int ey) { }
        public void OnMoveRight(int sx, int sy, int ex, int ey) { }
    }
    public class MoveTool : Tool, ITool
    {
        public MoveTool(FullCanvas fullCanvas) : base(fullCanvas) { }

        public void OnStart(int x, int y) { }
        public void OnEnd(int sx, int sy, int ex, int ey) { }
        public void OnMove(int sx, int sy, int ex, int ey)
        {
            Canvas.X += (ex - sx) * FullCanvas.Zoom.Value;
            Canvas.Y += (ey - sy) * FullCanvas.Zoom.Value;
        }
    }
}