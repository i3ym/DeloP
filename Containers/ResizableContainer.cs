using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osuTK;

namespace DeloP.Containers
{
    public class ResizableContainer : CompositeDrawable
    {
        const float BoxSize = 10f;

        public event Action<ResizeEvent> OnResize = delegate { };

        public readonly DraggableBox TopRight, BottomRight, TopLeft, BottomLeft;
        public readonly DraggableBox Right, Left, Top, Bottom;

        Vector2 StartPos, StartSize;

        public ResizableContainer()
        {
            var borderColor = Colour4.Black;
            float borderSize = 2f;
            AddInternal(new Box() { RelativeSizeAxes = Axes.Y, Width = borderSize, Colour = borderColor, Anchor = Anchor.TopLeft, Origin = Anchor.TopLeft });
            AddInternal(new Box() { RelativeSizeAxes = Axes.Y, Width = borderSize, Colour = borderColor, Anchor = Anchor.TopRight, Origin = Anchor.TopRight });
            AddInternal(new Box() { RelativeSizeAxes = Axes.X, Height = borderSize, Colour = borderColor, Anchor = Anchor.TopLeft, Origin = Anchor.TopLeft });
            AddInternal(new Box() { RelativeSizeAxes = Axes.X, Height = borderSize, Colour = borderColor, Anchor = Anchor.BottomLeft, Origin = Anchor.BottomLeft });

            AddInternal(TopRight = CreateBox(Anchor.TopRight));
            AddInternal(BottomRight = CreateBox(Anchor.BottomRight));

            AddInternal(Right = CreateBox(Anchor.CentreRight));
            AddInternal(Left = CreateBox(Anchor.CentreLeft));

            AddInternal(Top = CreateBox(Anchor.TopCentre));
            AddInternal(Bottom = CreateBox(Anchor.BottomCentre));

            AddInternal(TopLeft = CreateBox(Anchor.TopLeft));
            AddInternal(BottomLeft = CreateBox(Anchor.BottomLeft));
        }

        DraggableBox CreateBox(Anchor anchor)
        {
            var box = CreateSelectionBox();
            box.Anchor = box.Origin = anchor;
            box.Width = box.Height = BoxSize;

            box.DragEvent += e =>
            {
                if ((e.Target.Anchor & Anchor.y0) == Anchor.y0) // top
                {
                    Y += e.Delta.Y;
                    Height -= e.Delta.Y;
                }
                if ((e.Target.Anchor & Anchor.y2) == Anchor.y2) // bottom
                    Height += e.Delta.Y;

                if ((e.Target.Anchor & Anchor.x0) == Anchor.x0) // left
                {
                    X += e.Delta.X;
                    Width -= e.Delta.X;
                }
                if ((e.Target.Anchor & Anchor.x2) == Anchor.x2) // right
                    Width += e.Delta.X;
            };
            box.DragStartEvent += _ =>
            {
                StartPos = DrawPosition;
                StartSize = DrawSize;
            };
            box.DragEndEvent += _ => OnResize(new ResizeEvent(StartPos, StartSize, DrawPosition, DrawSize));

            return box;
        }
        DraggableBox CreateSelectionBox() => new DraggableBox(CreateSelectionDrawable());
        protected virtual Drawable CreateSelectionDrawable() => new Box() { Colour = Colors.Background };


        public class DraggableBox : CompositeDrawable
        {
            public event Action<DragStartEvent> DragStartEvent = delegate { };
            public event Action<DragEvent> DragEvent = delegate { };
            public event Action<DragEndEvent> DragEndEvent = delegate { };

            public DraggableBox(Drawable child)
            {
                child.RelativeSizeAxes = Axes.Both;

                Masking = true;
                BorderColour = Colour4.Black;
                BorderThickness = 2f;
                InternalChild = child;
            }

            protected override bool OnDragStart(DragStartEvent e)
            {
                DragStartEvent(e);
                return true;
            }
            protected override void OnDrag(DragEvent e)
            {
                DragEvent(e);
                base.OnDrag(e);
            }
            protected override void OnDragEnd(DragEndEvent e)
            {
                DragEndEvent(e);
                base.OnDragEnd(e);
            }

            protected override bool OnMouseDown(MouseDownEvent e) => true;
        }
        public readonly struct ResizeEvent
        {
            public readonly Vector2 StartPos, StartSize, EndPos, EndSize;

            public ResizeEvent(Vector2 startPos, Vector2 startSize, Vector2 endPos, Vector2 endSize)
            {
                StartPos = startPos;
                StartSize = startSize;
                EndPos = endPos;
                EndSize = endSize;
            }
        }
    }
}