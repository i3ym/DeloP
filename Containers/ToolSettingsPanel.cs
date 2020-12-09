using System.Linq;
using DeloP.Controls;
using DeloP.ToolSettings;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;

namespace DeloP.Containers
{
    public class ToolSettingsPanel : CompositeDrawable
    {
        readonly FillFlowContainer Container;

        public ToolSettingsPanel(FullCanvas fcanvas)
        {
            var canvas = fcanvas.Canvas;

            Container = new FillFlowContainer();
            Container.RelativeSizeAxes = Axes.Both;
            Container.Direction = FillDirection.Horizontal;
            Container.Padding = new MarginPadding() { Horizontal = 10, Vertical = 3 };
            Container.Spacing = new osuTK.Vector2(10, 0);

            var tools = Enumerable.Empty<Drawable>()
                .Append(new ZoomSetting(fcanvas))
                .Append(new ThicknessToolSetting(canvas))
                .Select(x => { x.RelativeSizeAxes = Axes.Y; return x; });

            Container.AddRange(tools);

            canvas.CurrentToolBindable.ValueChanged += e =>
            {
                foreach (var child in Container.Children.OfType<ToolSettingBase>())
                    child.Alpha = child.AppliesTo(e.NewValue) ? 1 : 0;
            };

            InternalChildren = new Drawable[]
            {
                new Box() { Colour = Colors.Background, RelativeSizeAxes = Axes.Both },
                Container
            };
        }

        protected override bool Handle(UIEvent e) => true;
    }
}