using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using DeloP.Controls;
using DeloP.ToolSettings;

namespace DeloP.Containers
{
    public class ToolSettingsPanel : CompositeDrawable
    {
        readonly FillFlowContainer Container;

        public ToolSettingsPanel(Canvas canvas)
        {
            Container = new FillFlowContainer();
            Container.RelativeSizeAxes = Axes.Both;
            Container.Direction = FillDirection.Horizontal;
            Container.Padding = new MarginPadding() { Horizontal = 10, Vertical = 3 };
            Container.Spacing = new osuTK.Vector2(10, 0);

            var tools = Enumerable.Empty<Drawable>()
                .Append(new ZoomSetting(canvas))
                .Append(new ThicknessToolSetting(canvas))
                .Select(x => { x.RelativeSizeAxes = Axes.Y; return x; });

            Container.AddRange(tools);

            canvas.OnSetTool += t =>
            {
                foreach (var child in Container.Children.OfType<ToolSettingBase>())
                    child.Alpha = child.AppliesTo(t) ? 1 : 0;
            };

            InternalChildren = new Drawable[]
            {
                new Box() { Colour = Colors.Background, RelativeSizeAxes = Axes.Both },
                Container
            };
        }
    }
}