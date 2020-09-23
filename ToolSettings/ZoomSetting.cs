using DeloP.Controls;
using osu.Framework.Bindables;
using osu.Framework.Graphics;

namespace DeloP.ToolSettings
{
    public class ZoomSetting : ToolSettingBase
    {
        public ZoomSetting(Canvas canvas)
        {
            var slider = new HandleSlider();
            slider.Current = new BindableInt() { MinValue = 0, MaxValue = 100 };
            slider.Current.ValueChanged += e => canvas.Scale = new osuTK.Vector2(e.NewValue / 100f * 10 + 1);
            slider.Current.Value = 0;

            slider.RelativeSizeAxes = Axes.Both;

            InternalChild = slider;
            Width = 100;
        }
    }
}