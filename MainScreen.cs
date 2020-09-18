using System;
using System.Threading.Tasks;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osuTK.Input;

namespace DeloP
{
    public class MainScreen : osu.Framework.Game
    {
        readonly ToolPanel ToolPanel;
        readonly Canvas Canvas;
        readonly ResizableContainer CanvasContainer;

        public MainScreen()
        {
            Canvas = new Canvas();
            CanvasContainer = new ResizableContainer();

            Canvas.OnImageReplace += img =>
            {
                Canvas.Width = img.Width;
                Canvas.Height = img.Height;

                CanvasContainer.Width = Canvas.Width * Canvas.Scale.X;
                CanvasContainer.Height = Canvas.Height * Canvas.Scale.Y;
            };
            CanvasContainer.OnResize += e =>
            {
                Canvas.ChangeSize((int) -((e.EndPos.X - e.StartPos.X) / Canvas.Scale.X), (int) -((e.EndPos.Y - e.StartPos.Y) / Canvas.Scale.Y),
                    (int) (e.EndSize.X / Canvas.Scale.X), (int) (e.EndSize.Y / Canvas.Scale.Y));
                Canvas.Position = e.EndPos;
            };
            Canvas.OnMove += () => CanvasContainer.Position = Canvas.DrawPosition;

            ToolPanel = new ToolPanel(Canvas) { RelativeSizeAxes = Axes.Y, Width = 68 };
            Canvas.Y = 2;
            Canvas.X = ToolPanel.Width + Canvas.Y;
        }
        protected override void LoadComplete()
        {
            base.LoadComplete();

            Window.Title = "DeloP";
            Window.WindowStateChanged += (obj, e) => Task.Run(async () => { await Task.Delay(5); Invalidate(); });

            Children = new Drawable[]
            {
                new Box() { Colour = Colors.DarkBackground, RelativeSizeAxes = Axes.Both },
                new Container() { Children = new Drawable[] { Canvas, CanvasContainer }, RelativeSizeAxes = Axes.Both },
                ToolPanel,
            };
        }


        #region move listener

        enum DrawingType : byte { None, Left, Right }
        DrawingType DrawType = DrawingType.None;
        int LastMouseX, LastMouseY;

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            if (base.OnMouseDown(e)) return true;
            if (!Canvas.BoundingBox.Contains(e.MousePosition)) return false;

            if (e.Button == MouseButton.Left) DrawType = DrawingType.Left;
            else if (e.Button == MouseButton.Right) DrawType = DrawingType.Right;
            else return false;


            (LastMouseX, LastMouseY) = ((int) e.MouseDownPosition.X, (int) e.MouseDownPosition.Y);

            var (x, y) = Canvas.ToImagePosition((int) e.MouseDownPosition.X, (int) e.MouseDownPosition.Y);
            if (e.Button == MouseButton.Left) Canvas.CurrentTool.OnStart(x, y, Canvas);
            else if (e.Button == MouseButton.Right) Canvas.CurrentTool.OnStartRight(x, y, Canvas);

            return false;
        }
        protected override void OnMouseUp(MouseUpEvent e)
        {
            base.OnMouseUp(e);
            if (DrawType == DrawingType.None) return;

            DrawType = DrawingType.None;

            var (sx, sy) = Canvas.ToImagePosition((int) e.MouseDownPosition.X, (int) e.MouseDownPosition.Y);
            var (ex, ey) = Canvas.ToImagePosition((int) e.MousePosition.X, (int) e.MousePosition.Y);

            if (e.Button == MouseButton.Left) Canvas.CurrentTool.OnEnd(sx, sy, ex, ey, Canvas);
            else if (e.Button == MouseButton.Right) Canvas.CurrentTool.OnEndRight(sx, sy, ex, ey, Canvas);
        }
        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            if (base.OnMouseMove(e)) return true;
            if (DrawType == DrawingType.None) return false;

            var (x, y) = Canvas.ToImagePosition((int) LastMouseX, (int) LastMouseY);
            var (tx, ty) = Canvas.ToImagePosition((int) e.MousePosition.X, (int) e.MousePosition.Y);

            (LastMouseX, LastMouseY) = ((int) e.MousePosition.X, (int) e.MousePosition.Y);

            if (DrawType == DrawingType.Left) Canvas.CurrentTool.OnMove(x, y, tx, ty, Canvas);
            else if (DrawType == DrawingType.Right) Canvas.CurrentTool.OnMoveRight(x, y, tx, ty, Canvas);

            return false;
        }

        #endregion
    }
}