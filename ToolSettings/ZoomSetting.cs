using DeloP.Controls;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;

namespace DeloP.ToolSettings
{
    public class ZoomSetting : ToolSettingBase
    {
        readonly FullCanvas Canvas;

        public ZoomSetting(FullCanvas canvas)
        {
            Canvas = canvas;

            var slider = new HandleSlider() { RelativeSizeAxes = Axes.Both };
            slider.Current = new BindableInt() { MinValue = 0, MaxValue = 100 };
            slider.Current.ValueChanged += HandleValueChanged;
            slider.Current.Value = 0;

            InternalChild = slider;
            Width = 100;

            canvas.Zoom.ValueChanged += e =>
            {
                slider.Current.ValueChanged -= HandleValueChanged;
                slider.Current.Value = (int) (e.NewValue * 10) - 10;
                slider.Current.ValueChanged += HandleValueChanged;
            };
        }

        void HandleValueChanged(ValueChangedEvent<int> e) => Canvas.SetZoom((e.NewValue + 10) / 10f);

        protected override bool Handle(UIEvent e) => true;
    }
}