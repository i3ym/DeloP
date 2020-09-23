using DeloP.Controls;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Layout;

namespace DeloP.ToolSettings
{
    public class ThicknessToolSetting : ToolSettingBase
    {
        readonly NumericUpDown Input;

        public ThicknessToolSetting(Canvas canvas)
        {
            Input = new NumericUpDown();
            Input.RelativeSizeAxes = Axes.Both;
            Input.OnChangeValue += t =>
            {
                if (canvas.CurrentTool is IThicknessTool tt)
                    tt.Thickness = t;
            };

            canvas.OnSetTool += t =>
            {
                if (t is IThicknessTool tt)
                    Input.Value = (int) tt.Thickness;
            };

            AddInternal(new Box() { RelativeSizeAxes = Axes.Both, Colour = Colors.DarkBackground });
            AddInternal(Input);
        }

        public override bool AppliesTo(ITool tool) => tool is IThicknessTool;
        protected override bool OnInvalidate(Invalidation invalidation, InvalidationSource source)
        {
            Width = Input.Width;
            return base.OnInvalidate(invalidation, source);
        }
    }
}