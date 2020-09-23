using System.Linq;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using DeloP.Controls;

namespace DeloP.Containers
{
    public class ToolSettingsPanel : CompositeDrawable
    {
        IReadOnlyList<Drawable> Children
        {
            get => Container.Children;
            set => Container.Children = value;
        }
        readonly FillFlowContainer Container;

        public ToolSettingsPanel(Canvas canvas)
        {
            canvas.OnSetTool += t =>
            {
                Children = t.GetSettings(canvas).Select(x => { x.RelativeSizeAxes = Axes.Y; return x; }).ToArray();
            };


            InternalChildren = new Drawable[]
            {
                new Box() { Colour = Colors.Background, RelativeSizeAxes = Axes.Both },
                Container = new FillFlowContainer()
                {
                    RelativeSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Padding = new MarginPadding() { Horizontal = 10, Vertical = 3 },
                    Spacing = new osuTK.Vector2(10, 0)
                }
            };
        }
    }
}