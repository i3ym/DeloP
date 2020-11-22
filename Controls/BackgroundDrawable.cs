using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;

namespace DeloP.Controls
{
    public static class BackgroundDrawable
    {
        public static BackgroundDrawable<T> Create<T>(T child) where T : Drawable => new BackgroundDrawable<T>(child);
        public static BackgroundDrawable<T> Create<T>(T child, Colour4 background) where T : Drawable => new BackgroundDrawable<T>(child) { Background = background };
    }
    public class BackgroundDrawable<T> : CompositeDrawable where T : Drawable
    {
        public Colour4 Background { get => Box.Colour; set => Box.Colour = value; }

        public readonly T Child;
        readonly Box Box = new Box() { RelativeSizeAxes = Axes.Both };

        public BackgroundDrawable(T child)
        {
            AddInternal(Box);
            AddInternal(Child = child);

            child.RelativeSizeAxes = Axes.Both;
        }
    }
}