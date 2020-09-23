using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Layout;

namespace DeloP.Containers
{
    public class BorderContainer : CompositeDrawable
    {
        public new bool Masking
        {
            get => Container.Masking;
            set => Container.Masking = value;
        }
        public new Colour4 BorderColour
        {
            get => Border.Colour;
            set => Border.Colour = value;
        }
        public Colour4 BackgroundColour
        {
            get => Background.Colour;
            set => Background.Colour = value;
        }
        int _BorderThickness;
        public new int BorderThickness
        {
            get => _BorderThickness;
            set
            {
                _BorderThickness = value;
                Invalidate(Invalidation.Layout);
            }
        }


        readonly Box Border, Background;
        readonly Container Container;

        public BorderContainer()
        {
            Border = new Box();
            Border.RelativeSizeAxes = Axes.Both;

            Background = new Box();
            Background.RelativeSizeAxes = Axes.Both;
            Background.Colour = Colour4.Transparent;

            Container = new Container();

            AddInternal(Border);
            AddInternal(Background);
            AddInternal(Container);
        }

        public void Add(Drawable drawable) => Container.Add(drawable);
        public void AddRange(IEnumerable<Drawable> drawables) => Container.AddRange(drawables);
        public void Remove(Drawable drawable) => Container.Remove(drawable);

        protected override bool OnInvalidate(Invalidation invalidation, InvalidationSource source)
        {
            if ((invalidation & Invalidation.Layout) == 0) return base.OnInvalidate(invalidation, source);

            Container.X = Container.Y = BorderThickness;
            Container.Width = DrawWidth - BorderThickness * 2;
            Container.Height = DrawHeight - BorderThickness * 2;

            return base.OnInvalidate(invalidation, source);
        }
    }
}