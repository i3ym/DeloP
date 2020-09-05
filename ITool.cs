using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

namespace Painter
{
    public interface ITool
    {
        void OnStart(int x, int y, Canvas canvas);
        void OnEnd(int sx, int sy, int ex, int ey, Canvas canvas);
        void OnMove(int sx, int sy, int ex, int ey, Canvas canvas);
    }

    public abstract class DrawTool : ITool
    {
        protected GraphicsOptions Options = GraphicsOptions.Default;
        public float Thickness = 1f;

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
        public PencilTool()
        {
            Options = new GraphicsOptions(false);
        }
    }
    public class BrushTool : DrawTool
    {
        public BrushTool()
        {
            Thickness = 4f;
        }
    }
}