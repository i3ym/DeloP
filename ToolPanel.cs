using System.Linq;
using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;

namespace Painter
{
    public class ToolPanel : CompositeDrawable
    {
        public event Action<ITool> OnChoose = delegate { };

        protected override void LoadComplete()
        {
            base.LoadComplete();

            var tools = Enumerable.Empty<ITool>()
                .Append(new PencilTool())
                .Append(new BrushTool())
                .Select(x => new ToolDrawable(x) { RelativeSizeAxes = Axes.Both })
                .ToArray();

            foreach (var tool in tools)
                tool.OnChoose += e => OnChoose(e);

            var toolsGrid = tools
                .Select((x, i) => (x, i))
                .GroupBy(x => x.i % 2)
                .Select(x => x.Select(x => x.x).ToArray())
                .ToArray();

            InternalChildren = new Drawable[]
            {
                new Box() { Colour = Colour4.Black, RelativeSizeAxes = Axes.Both },
                new GridContainer() { Content = toolsGrid, RelativeSizeAxes = Axes.Both }
            };
        }



        class ToolDrawable : CompositeDrawable
        {
            public event Action<ITool> OnChoose = delegate { };
            readonly ITool Tool;

            public ToolDrawable(ITool tool)
            {
                Tool = tool;

                var button = new ToolButton(e => OnChoose(Tool));
                button.RelativeSizeAxes = Axes.Both;

                button.Text = tool.GetType().Name;

                InternalChild = button;
            }


            class ToolButton : BasicButton
            {
                readonly Action<ClickEvent> ClickAction;

                public ToolButton(Action<ClickEvent> onClick)
                {
                    ClickAction = onClick;
                    BackgroundColour = Colour4.Green;
                }

                protected override bool OnClick(ClickEvent e)
                {
                    ClickAction(e);
                    return base.OnClick(e);
                }
            }
        }
    }
}