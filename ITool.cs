using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

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

        protected GraphicsOptions Options = GraphicsOptions.Default;
        public float Thickness { get; set; } = 1f;

        public void OnStart(int x, int y, Canvas canvas) => DrawLine(x, y, x, y, canvas);
        public void OnEnd(int sx, int sy, int ex, int ey, Canvas canvas) { }
        public void OnMove(int sx, int sy, int ex, int ey, Canvas canvas) => DrawLine(sx, sy, ex, ey, canvas);

        protected void DrawLine(int sx, int sy, int ex, int ey, Canvas canvas)
        {
            var points = new[] { new PointF(sx, sy), new PointF(ex, ey) };
            canvas.Image.Mutate(ctx => ctx.DrawLines(Options, canvas.CurrentColor, Thickness, points));
            canvas.UpdateImage();
        }
    }
    public class PencilTool : DrawTool
    {
        public PencilTool() => Options = OptionsWithoutAA;
    }
    public class BrushTool : DrawTool { }

    public abstract class ShapeTool : ITool
    {
        public float Thickness { get; set; } = 1f;
        int StartX, StartY;

        public void OnStart(int x, int y, Canvas canvas) => (StartX, StartY) = (x, y);
        public void OnEnd(int sx, int sy, int ex, int ey, Canvas canvas)
        {
            ClearOverlayImage(canvas);

            canvas.Image.Mutate(ctx => ctx.DrawPolygon(DrawTool.OptionsWithoutAA, canvas.CurrentColor, Thickness, GetPath(StartX, StartY, ex, ey)));
            canvas.UpdateOverlay();
            canvas.UpdateImage();
        }

        public void OnMove(int sx, int sy, int ex, int ey, Canvas canvas)
        {
            ClearOverlayImage(canvas);

            canvas.OverlayImage.Mutate(ctx => ctx.DrawPolygon(DrawTool.OptionsWithoutAA, canvas.CurrentColor, Thickness, GetPath(StartX, StartY, ex, ey)));
            canvas.UpdateOverlay();
        }

        void ClearOverlayImage(Canvas canvas)
        {
            var transparent = (Rgba32) Color.Transparent;
            for (int y = 0; y < canvas.OverlayImage.Height; y++)
                for (int x = 0; x < canvas.OverlayImage.Width; x++)
                    canvas.OverlayImage[x, y] = transparent;
        }

        protected abstract PointF[] GetPath(int startX, int startY, int endX, int endY);
    }
    public class RectangeTool : ShapeTool
    {
        protected override PointF[] GetPath(int startX, int startY, int endX, int endY) =>
            new[]
            {
                new PointF(startX, startY),
                new PointF(endX, startY),
                new PointF(endX, endY),
                new PointF(startX, endY)
            };
    }
    public class TriangleTool : ShapeTool
    {
        protected override PointF[] GetPath(int startX, int startY, int endX, int endY) =>
            new[]
            {
                new PointF(startX, endY),
                new PointF(startX + (endX - startX) / 2, startY),
                new PointF(endX, endY)
            };
    }
    public class LineTool : ShapeTool
    {
        protected override PointF[] GetPath(int startX, int startY, int endX, int endY) =>
            new[]
            {
                new PointF(startX, startY),
                new PointF(endX, endY)
            };
    }
}