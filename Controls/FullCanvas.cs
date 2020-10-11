using DeloP.Containers;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osuTK.Input;

namespace DeloP.Controls
{
    public class FullCanvas : CompositeDrawable
    {
        public readonly Canvas Canvas;
        readonly ResizableContainer CanvasContainer;

        public FullCanvas()
        {
            AddInternal(Canvas = new Canvas());
            AddInternal(CanvasContainer = new ResizableContainer());


            Canvas.OnImageReplace += img =>
            {
                Canvas.Width = img.Width;
                Canvas.Height = img.Height;

                CanvasContainer.Width = Canvas.Width * Canvas.Zoom;
                CanvasContainer.Height = Canvas.Height * Canvas.Zoom;
            };
            CanvasContainer.OnResize += e =>
            {
                Canvas.ChangeSize((int) -((e.EndPos.X - e.StartPos.X) / Canvas.Zoom), (int) -((e.EndPos.Y - e.StartPos.Y) / Canvas.Zoom),
                    (int) (e.EndSize.X / Canvas.Zoom), (int) (e.EndSize.Y / Canvas.Zoom));
                Canvas.Position = e.EndPos;
            };
            Canvas.LayoutInvalidateAction += () =>
            {
                CanvasContainer.Position = Canvas.DrawPosition;
                CanvasContainer.Size = Canvas.DrawSize;
            };
        }


        enum DrawingType : byte { None, Left, Right }
        DrawingType DrawType = DrawingType.None;
        int LastMouseX, LastMouseY;

        public bool MouseDown(MouseDownEvent e)
        {
            if (base.OnMouseDown(e)) return true;
            if (!Canvas.BoundingBox.Contains(e.MousePosition)) return false;

            if (e.Button == MouseButton.Left) DrawType = DrawingType.Left;
            else if (e.Button == MouseButton.Right) DrawType = DrawingType.Right;
            else return false;


            (LastMouseX, LastMouseY) = ((int) e.ScreenSpaceMouseDownPosition.X, (int) e.ScreenSpaceMouseDownPosition.Y);

            var (x, y) = Canvas.ToImageFromScreen((int) e.ScreenSpaceMouseDownPosition.X, (int) e.ScreenSpaceMouseDownPosition.Y);
            if (e.Button == MouseButton.Left) Canvas.CurrentTool.OnStart(x, y, Canvas);
            else if (e.Button == MouseButton.Right) Canvas.CurrentTool.OnStartRight(x, y, Canvas);

            return false;
        }
        public void MouseUp(MouseUpEvent e)
        {
            base.OnMouseUp(e);
            if (DrawType == DrawingType.None) return;

            DrawType = DrawingType.None;

            var (sx, sy) = Canvas.ToImageFromScreen((int) e.ScreenSpaceMouseDownPosition.X, (int) e.ScreenSpaceMouseDownPosition.Y);
            var (ex, ey) = Canvas.ToImageFromScreen((int) e.ScreenSpaceMousePosition.X, (int) e.ScreenSpaceMousePosition.Y);

            if (e.Button == MouseButton.Left) Canvas.CurrentTool.OnEnd(sx, sy, ex, ey, Canvas);
            else if (e.Button == MouseButton.Right) Canvas.CurrentTool.OnEndRight(sx, sy, ex, ey, Canvas);
        }
        public bool MouseMove(MouseMoveEvent e)
        {
            if (base.OnMouseMove(e)) return true;
            if (DrawType == DrawingType.None) return false;

            var (x, y) = Canvas.ToImageFromScreen((int) LastMouseX, (int) LastMouseY);
            var (tx, ty) = Canvas.ToImageFromScreen((int) e.ScreenSpaceMousePosition.X, (int) e.ScreenSpaceMousePosition.Y);

            (LastMouseX, LastMouseY) = ((int) e.ScreenSpaceMousePosition.X, (int) e.ScreenSpaceMousePosition.Y);

            if (DrawType == DrawingType.Left) Canvas.CurrentTool.OnMove(x, y, tx, ty, Canvas);
            else if (DrawType == DrawingType.Right) Canvas.CurrentTool.OnMoveRight(x, y, tx, ty, Canvas);

            return false;
        }
    }
}