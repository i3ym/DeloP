using System;
using DeloP.Containers;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osuTK;
using osuTK.Input;

namespace DeloP.Controls
{
    public class FullCanvas : CompositeDrawable
    {
        public readonly IBindable<float> Zoom = new BindableFloat(1);

        public readonly Canvas Canvas;
        readonly ResizableContainer CanvasResizer;

        public FullCanvas()
        {
            AddInternal(Canvas = new Canvas());
            AddInternal(CanvasResizer = new ResizableContainer() { Anchor = Canvas.Anchor, Origin = Canvas.Origin });


            Canvas.ScrollEvent += e =>
            {
                if (!e.ControlPressed && !e.AltPressed) return;
                SetZoom(Zoom.Value + e.ScrollDelta.Y / 100f * 2, e.MousePosition);
            };
            CanvasResizer.OnResize += e =>
            {
                Canvas.Position = e.EndPos;
                Canvas.ChangeSize((int) -((e.EndPos.X - e.StartPos.X) / Zoom.Value), (int) -((e.EndPos.Y - e.StartPos.Y) / Zoom.Value),
                    (int) (e.EndSize.X / Zoom.Value), (int) (e.EndSize.Y / Zoom.Value));
            };
            Canvas.LayoutInvalidateEvent += () =>
            {
                const float margin = 150;

                var br = Canvas.BoundingBox.BottomRight;
                if (br.X < margin) Canvas.X += margin - br.X;
                if (br.Y < margin) Canvas.Y += margin - br.Y;

                if (Parent is { })
                {
                    var tl = Canvas.BoundingBox.TopLeft;
                    if (tl.X > Parent.DrawWidth - margin) Canvas.X = Parent.DrawWidth - margin;
                    if (tl.Y > Parent.DrawHeight - margin) Canvas.Y = Parent.DrawHeight - margin;
                }


                CanvasResizer.Position = Canvas.DrawPosition;
                CanvasResizer.Size = Canvas.Size * Canvas.Scale;
            };
        }

        public void SetZoom(float value) => SetZoom(value, Canvas.DrawSize / 2);
        public void SetZoom(float value, Vector2 origin)
        {
            value = Math.Max(value, .1f);
            origin = new Vector2(origin.X / Canvas.Width * Canvas.Image.Width, origin.Y / Canvas.Height * Canvas.Image.Height);

            var offset = (value - Zoom.Value) * origin;
            ((Bindable<float>) Zoom).Value = value;

            Canvas.FinishTransforms();
            Canvas.ScaleTo(value, 100, Easing.OutQuad);
            Canvas.MoveTo(Canvas.Position - offset, 100, Easing.OutQuad);
        }


        enum DrawingType : byte { None, Left, Right }
        DrawingType DrawType = DrawingType.None;
        int LastMouseX, LastMouseY;

        public bool MouseDown(MouseDownEvent e)
        {
            if (base.OnMouseDown(e)) return true;

            var mousepos = Canvas.ToImageFromScreen((int) e.ScreenSpaceMousePosition.X, (int) e.ScreenSpaceMousePosition.Y);
            if (mousepos.x < 0 || mousepos.y < 0) return false;
            if (mousepos.x > Canvas.Image.Width || mousepos.y > Canvas.Image.Height) return false;

            if (e.Button == MouseButton.Left) DrawType = DrawingType.Left;
            else if (e.Button == MouseButton.Right) DrawType = DrawingType.Right;
            else return false;


            (LastMouseX, LastMouseY) = ((int) e.ScreenSpaceMouseDownPosition.X, (int) e.ScreenSpaceMouseDownPosition.Y);

            var (x, y) = Canvas.ToImageFromScreen((int) e.ScreenSpaceMouseDownPosition.X, (int) e.ScreenSpaceMouseDownPosition.Y);
            if (e.Button == MouseButton.Left) Canvas.CurrentTool.OnStart(x, y);
            else if (e.Button == MouseButton.Right) Canvas.CurrentTool.OnStartRight(x, y);

            return false;
        }
        public void MouseUp(MouseUpEvent e)
        {
            base.OnMouseUp(e);
            if (DrawType == DrawingType.None) return;

            DrawType = DrawingType.None;

            var (sx, sy) = Canvas.ToImageFromScreen((int) e.ScreenSpaceMouseDownPosition.X, (int) e.ScreenSpaceMouseDownPosition.Y);
            var (ex, ey) = Canvas.ToImageFromScreen((int) e.ScreenSpaceMousePosition.X, (int) e.ScreenSpaceMousePosition.Y);

            if (e.Button == MouseButton.Left) Canvas.CurrentTool.OnEnd(sx, sy, ex, ey);
            else if (e.Button == MouseButton.Right) Canvas.CurrentTool.OnEndRight(sx, sy, ex, ey);
        }
        public bool MouseMove(MouseMoveEvent e)
        {
            if (base.OnMouseMove(e)) return true;
            if (DrawType == DrawingType.None) return false;

            var (x, y) = Canvas.ToImageFromScreen((int) LastMouseX, (int) LastMouseY);
            var (tx, ty) = Canvas.ToImageFromScreen((int) e.ScreenSpaceMousePosition.X, (int) e.ScreenSpaceMousePosition.Y);

            (LastMouseX, LastMouseY) = ((int) e.ScreenSpaceMousePosition.X, (int) e.ScreenSpaceMousePosition.Y);

            if (DrawType == DrawingType.Left) Canvas.CurrentTool.OnMove(x, y, tx, ty);
            else if (DrawType == DrawingType.Right) Canvas.CurrentTool.OnMoveRight(x, y, tx, ty);

            return false;
        }
    }
}