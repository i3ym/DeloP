using System.Linq;
using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Layout;
using SixLabors.ImageSharp.PixelFormats;

namespace Painter
{
    public class ToolPanel : CompositeDrawable
    {
        public event Action<ITool> OnChoose = delegate { };
        public event Action<Rgba32> OnChangeColor = delegate { };

        protected override void LoadComplete()
        {
            base.LoadComplete();

            var tools = Enumerable.Empty<ITool>()
                .Append(new PencilTool())
                .Append(new LineTool())
                .Append(new RectangeTool())
                .Append(new TriangleTool())
                .Append(new FillTool())
                .Select(x => { x.Thickness = 5f; return x; })

                .Select(x => new ToolDrawable(x) { RelativeSizeAxes = Axes.Both })
                .Select(x => { x.OnChoose += t => OnChoose(t); return x; })

                .Select((x, i) => (x, i))
                .GroupBy(x => x.i / 2)
                .Select(x => x.Select(x => x.x).ToArray())
                .ToArray();


            var colorsc = new ColorsContainer() { RelativeSizeAxes = Axes.X };
            colorsc.OnChangeColor += clr => OnChangeColor(clr);

            var grid = new GridContainer();
            grid.RelativeSizeAxes = Axes.Both;
            grid.Content = new Drawable[][]
            {
                new Drawable[] { new GridContainer() { Content = tools, RelativeSizeAxes = Axes.Both } },
                new Drawable[] { colorsc },
            };


            InternalChildren = new Drawable[]
            {
                new Box() { Colour = Colour4.Black, RelativeSizeAxes = Axes.Both },
                grid
            };

            OnChoose(tools.First().First().Tool);
        }



        class ToolDrawable : CompositeDrawable
        {
            public event Action<ITool> OnChoose = delegate { };
            public readonly ITool Tool;

            public ToolDrawable(ITool tool)
            {
                Tool = tool;

                var button = new ToolButton(e => OnChoose(Tool));
                button.RelativeSizeAxes = Axes.Both;

                button.Text = tool.GetType().Name.Replace("Tool", "");

                InternalChild = button;
            }


            class ToolButton : BasicButton
            {
                readonly Action<ClickEvent> ClickEvent;

                public ToolButton(Action<ClickEvent> onClick) => ClickEvent = onClick;

                protected override bool OnClick(ClickEvent e)
                {
                    ClickEvent(e);
                    return base.OnClick(e);
                }
            }
        }

        class ColorsContainer : CompositeDrawable
        {
            public event Action<Rgba32> OnChangeColor = delegate { };

            readonly ColorBox Color1, Color2;
            readonly GridContainer Grid;

            public ColorsContainer()
            {
                Color1 = new ColorBox(Colour4.Black);
                Color1.ClickEvent += _ =>
                {
                    // TODO:
                };

                Color2 = new ColorBox(Colour4.Black);
                Color2.ClickEvent += _ =>
                {
                    // TODO:
                };


                var colors = new[]
                {
                    Colour4.White,
                    Colour4.Black,
                    Colour4.DarkGray,
                    Colour4.Gray,

                    Colour4.Red,
                    Colour4.Green,
                    Colour4.Blue,
                };

                var colorsd = colors
                    .Select(x => new ColorBox(x) { RelativeSizeAxes = Axes.Both })
                    .Select(x =>
                    {
                        x.ClickEvent += clr => OnChangeColor(clr);
                        return x;
                    })
                    .Select((x, i) => (x, i))
                    .GroupBy(x => x.i / 2)
                    .Select(x => x.Select(x => x.x).ToArray())
                    .Prepend(new[] { Color1, Color2 })
                    .ToArray();

                Grid = new GridContainer();
                Grid.RelativeSizeAxes = Axes.Both;
                Grid.Content = colorsd;

                InternalChild = Grid;
            }

            protected override bool OnInvalidate(Invalidation invalidation, InvalidationSource source)
            {
                Height = Grid.Content.SelectMany(x => x).Count() * Grid.DrawWidth / 2f / 2f;
                return base.OnInvalidate(invalidation, source);
            }


            class ColorBox : CompositeDrawable
            {
                public event Action<Rgba32> ClickEvent = delegate { };
                readonly Colour4 Color;

                public ColorBox(Colour4 color)
                {
                    Color = color;

                    Masking = true;
                    CornerRadius = 1f;
                    BorderThickness = 2f;
                    BorderColour = new Colour4(56, 56, 56, 255);

                    InternalChild = new Box() { Colour = color, RelativeSizeAxes = Axes.Both };
                }

                protected override bool OnClick(ClickEvent e)
                {
                    if (base.OnClick(e)) return true;

                    ClickEvent(new Rgba32(Color.R, Color.G, Color.B));
                    return false;
                }
            }
        }
    }
}