using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Layout;

namespace DeloP
{
    public class ThicknessToolSettings : CompositeDrawable
    {
        readonly NumericUpDown Input;

        public ThicknessToolSettings(ITool tool)
        {
            Input = new NumericUpDown();
            Input.RelativeSizeAxes = Axes.Both;
            Input.OnChangeValue += t => tool.Thickness = t;

            AddInternal(new Box() { RelativeSizeAxes = Axes.Both, Colour = Colors.DarkBackground });
            AddInternal(Input);
        }

        protected override bool OnInvalidate(Invalidation invalidation, InvalidationSource source)
        {
            Width = Input.Width;
            return base.OnInvalidate(invalidation, source);
        }
    }
}