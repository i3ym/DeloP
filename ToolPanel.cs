using System;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Layout;
using SixLabors.ImageSharp.PixelFormats;
using osuTK.Input;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;

namespace Painter
{
    public class ToolPanel : CompositeDrawable
    {
        readonly Canvas Canvas;

        public ToolPanel(Canvas canvas) => Canvas = canvas;
        protected override void LoadComplete()
        {
            base.LoadComplete();

            var grid = new GridContainer();
            grid.RelativeSizeAxes = Axes.Both;
            grid.Content = new Drawable[][]
            {
                new Drawable[] { new ToolsContainer(Canvas) { RelativeSizeAxes = Axes.X } },
                new Drawable[] { new ColorsContainer(Canvas) { RelativeSizeAxes = Axes.X } },
            };

            InternalChildren = new Drawable[]
            {
                new Box() { Colour = Colour4.Black, RelativeSizeAxes = Axes.Both },
                grid
            };
        }


        class SpacedGrid : GridContainer
        {
            // TODO:
        }

        class ToolsContainer : CompositeDrawable
        {
            readonly SpacedGrid Grid;
            readonly Canvas Canvas;

            public ToolsContainer(Canvas canvas)
            {
                Canvas = canvas;

                var tools = Enumerable.Empty<ITool>()
                    .Append(new PencilTool())
                    .Append(new EraserTool())
                    .Append(new PipetteTool())
                    .Append(new LineTool())
                    .Append(new RectangleTool())
                    .Append(new TriangleTool())
                    .Append(new FillTool())
                    .Select(x => { x.Thickness = 5f; return x; })

                    .Select(x => new ToolDrawable(Canvas, x) { RelativeSizeAxes = Axes.Both })
                    .Select((x, i) => (x, i))
                    .GroupBy(x => x.i / 2)
                    .Select(x => x.Select(x => x.x).ToArray())
                    .ToArray();

                Grid = new SpacedGrid();
                Grid.RelativeSizeAxes = Axes.Both;
                Grid.Content = tools;

                InternalChild = Grid;

                Canvas.CurrentTool = tools.First().First().Tool;
            }

            protected override bool OnInvalidate(Invalidation invalidation, InvalidationSource source)
            {
                Height = Grid.Content.SelectMany(x => x).Count() * Grid.DrawWidth / 2f / 2f;
                return base.OnInvalidate(invalidation, source);
            }


            class ToolDrawable : CompositeDrawable
            {
                readonly Canvas Canvas;
                public readonly ITool Tool;

                public ToolDrawable(Canvas canvas, ITool tool)
                {
                    Canvas = canvas;
                    Tool = tool;

                    var sprite = new Sprite();
                    sprite.RelativeSizeAxes = Axes.Both;

                    try
                    {
                        var img = SpriteStore.Load("res.sprites." + tool.SpriteName + ".png");
                        sprite.Texture = new Texture(img.Width, img.Height, true, osuTK.Graphics.ES30.All.Nearest);
                        sprite.Texture.SetData(new TextureUpload(img));
                    }
                    catch { Console.WriteLine("res.sprites." + tool.SpriteName + ".png   was not found"); }

                    InternalChild = sprite;

                    /* var button = new ToolButton(canvas);
                    button.RelativeSizeAxes = Axes.Both;
                    button.Text = tool.GetType().Name.Replace("Tool", "");

                    InternalChild = button;*/
                }

                protected override bool OnClick(ClickEvent e)
                {
                    Canvas.CurrentTool = Tool;
                    return base.OnClick(e);
                }
            }
        }
        class ColorsContainer : CompositeDrawable
        {
            readonly ColorBox Color1, Color2;
            readonly SpacedGrid Grid;
            readonly Canvas Canvas;

            public ColorsContainer(Canvas canvas)
            {
                Canvas = canvas;

                Color1 = new ChooseColorBox(canvas, Colour4.Black);
                /* Color1.ClickEvent += _ =>
                 {
                     // TODO:
                 };*/

                Color2 = new ChooseColorBox(canvas, Colour4.Black);
                /* Color2.ClickEvent += _ =>
                 {
                     // TODO:
                 };*/


                var colors = Enumerable.Empty<Colour4>()
                    .Append(Colour4.White).Append(Colour4.Black).Append(Colour4.DarkGray).Append(Colour4.Gray)
                    .Append(Colour4.Red).Append(Colour4.Green).Append(Colour4.Blue)

                    .Select(x => new SetColorBox(canvas, x) { RelativeSizeAxes = Axes.Both })
                    .Select((x, i) => (x, i))
                    .GroupBy(x => x.i / 2)
                    .Select(x => x.Select(x => x.x).ToArray())
                    .Prepend(new[] { Color1, Color2 })
                    .ToArray();

                Grid = new SpacedGrid();
                Grid.RelativeSizeAxes = Axes.Both;
                Grid.Content = colors;

                InternalChild = Grid;
            }

            protected override bool OnInvalidate(Invalidation invalidation, InvalidationSource source)
            {
                Height = Grid.Content.SelectMany(x => x).Count() * Grid.DrawWidth / 2f / 2f;
                return base.OnInvalidate(invalidation, source);
            }


            abstract class ColorBox : CompositeDrawable
            {
                protected readonly Canvas Canvas;
                protected readonly Colour4 Color;

                public ColorBox(Canvas canvas, Colour4 color)
                {
                    Canvas = canvas;
                    Color = color;

                    Masking = true;
                    CornerRadius = 1f;
                    BorderThickness = 2f;
                    BorderColour = new Colour4(56, 56, 56, 255);

                    InternalChild = new Box() { Colour = color, RelativeSizeAxes = Axes.Both };
                }
            }
            class SetColorBox : ColorBox
            {
                public SetColorBox(Canvas canvas, Colour4 color) : base(canvas, color) { }

                protected override bool OnClick(ClickEvent e)
                {
                    if (base.OnClick(e)) return true;

                    if (e.Button == MouseButton.Left) Canvas.MainColor = new Rgba32(Color.R, Color.G, Color.B);
                    else if (e.Button == MouseButton.Right) Canvas.SecondaryColor = new Rgba32(Color.R, Color.G, Color.B);

                    return false;
                }
            }
            class ChooseColorBox : ColorBox
            {
                public ChooseColorBox(Canvas canvas, Colour4 color) : base(canvas, color) { }

                protected override bool OnClick(ClickEvent e)
                {
                    // TODO color chooser
                    return base.OnClick(e);
                }
            }
        }
    }
}