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
                new Box() { Colour = Colors.Background, RelativeSizeAxes = Axes.Both },
                grid
            };
        }


        class ToolsContainer : CompositeDrawable
        {
            readonly GridContainer Grid;
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

                Grid = new GridContainer();
                Grid.RelativeSizeAxes = Axes.Both;
                Grid.Content = tools;

                InternalChild = Grid;

                Canvas.CurrentTool = tools.First().First().Tool;
            }

            protected override bool OnInvalidate(Invalidation invalidation, InvalidationSource source)
            {
                Height = Grid.Content.Length / 2 * Grid.DrawWidth;
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
            readonly CurrentColors CColors;
            readonly ColorsGrid GColors;
            readonly Canvas Canvas;

            public ColorsContainer(Canvas canvas)
            {
                AddInternal(new Box() { Colour = Colour4.Black, RelativeSizeAxes = Axes.Both }); // TODO: remove

                Canvas = canvas;

                CColors = new CurrentColors(canvas);
                CColors.RelativeSizeAxes = Axes.X;
                AddInternal(CColors);

                GColors = new ColorsGrid(canvas);
                GColors.RelativeSizeAxes = Axes.X;
                AddInternal(GColors);
            }

            protected override bool OnInvalidate(Invalidation invalidation, InvalidationSource source)
            {
                CColors.Height = CColors.DrawWidth;
                GColors.Y = CColors.DrawHeight;
                return base.OnInvalidate(invalidation, source);
            }


            class ColorsGrid : CompositeDrawable
            {
                readonly GridContainer Grid;
                readonly Canvas Canvas;

                public ColorsGrid(Canvas canvas)
                {
                    Canvas = canvas;

                    var colors = Enumerable.Empty<Colour4>()
                        .Append(Colour4.White).Append(Colour4.Black).Append(Colour4.DarkGray).Append(Colour4.Gray)
                        .Append(Colour4.Red).Append(Colour4.Green).Append(Colour4.Blue)

                        .Select(x => new SetColorBox(canvas, x))
                        .Select(x =>
                        {
                            x.Width = x.Height = 22;
                            return x;
                        })
                        .Select((x, i) => (x, i))
                        .GroupBy(x => x.i / 2)
                        .Select(x => x.Select(x => x.x).ToArray())
                        .ToArray();

                    Grid = new GridContainer();
                    Grid.RelativeSizeAxes = Axes.Both;
                    Grid.Content = colors;

                    AddInternal(Grid);
                }

                protected override bool OnInvalidate(Invalidation invalidation, InvalidationSource source)
                {
                    foreach (var content in Grid.Content)
                        foreach (var child in content)
                            child.Width = child.Height = DrawWidth / 2;

                    Height = Grid.Content.Length * DrawWidth / 2;
                    return base.OnInvalidate(invalidation, source);
                }
            }
            class CurrentColors : CompositeDrawable
            {
                public readonly ChooseColorBox Color1, Color2;
                readonly Container Container;

                public CurrentColors(Canvas canvas)
                {
                    Color1 = new ChooseColorBox(canvas, Colour4.Black);
                    canvas.OnSetMainColor += c => Color1.InternalColour = new Colour4(c.R, c.G, c.B, 255);
                    /* Color1.ClickEvent += _ =>
                     {
                         // TODO:
                     };*/

                    Color2 = new ChooseColorBox(canvas, Colour4.Black);
                    canvas.OnSetSecondaryColor += c => Color2.InternalColour = new Colour4(c.R, c.G, c.B, 255);
                    /* Color2.ClickEvent += _ =>
                     {
                         // TODO:
                     };*/


                    Container = new Container();
                    Container.Add(Color2);
                    Container.Add(Color1);

                    InternalChild = Container;
                }


                protected override bool OnInvalidate(Invalidation invalidation, InvalidationSource source)
                {
                    Color1.Width = Color1.Height = DrawWidth / 2;
                    Color2.Width = Color2.Height = Color1.Width;

                    Color1.X = DrawWidth / 8;
                    Color2.X = Color1.X + Color1.Width / 2;
                    Color2.Y = Color1.Y + Color1.Width / 2;

                    Container.Width = Container.Height = Color1.Y + Color1.Height - Color2.Y + Color2.Height;
                    return base.OnInvalidate(invalidation, source);
                }

                public class ChooseColorBox : ColorBox
                {
                    public Colour4 InternalColour { get => ColourBox.Colour; set => ColourBox.Colour = value; }

                    public ChooseColorBox(Canvas canvas, Colour4 color) : base(canvas, color) { }

                    protected override bool OnMouseDown(MouseDownEvent e)
                    {
                        // TODO color chooser
                        return base.OnMouseDown(e);
                    }
                }
            }

            abstract class ColorBox : CompositeDrawable
            {
                protected readonly Canvas Canvas;
                protected readonly Colour4 Color;
                protected readonly Box ColourBox, DarkBorderBox, BackgroundBox;

                public ColorBox(Canvas canvas, Colour4 color)
                {
                    Canvas = canvas;
                    Color = color;

                    AddInternal(BackgroundBox = new Box() { Colour = Colors.Background, RelativeSizeAxes = Axes.Both });
                    AddInternal(DarkBorderBox = new Box() { Colour = Colors.DarkBackground });
                    AddInternal(ColourBox = new Box() { Colour = color });

                    BackgroundBox.X = BackgroundBox.Y = 0;
                }

                protected override bool OnInvalidate(Invalidation invalidation, InvalidationSource source)
                {
                    var outb = (int) DrawWidth / 11;
                    var inb = (int) DrawWidth / 22;

                    ColourBox.X = ColourBox.Y = outb + inb;
                    DarkBorderBox.X = DarkBorderBox.Y = outb;

                    DarkBorderBox.Width = DarkBorderBox.Height = BackgroundBox.DrawWidth - (outb * 2);
                    ColourBox.Width = ColourBox.Height = DarkBorderBox.DrawWidth - (inb * 2);

                    return base.OnInvalidate(invalidation, source);
                }
            }
            class SetColorBox : ColorBox
            {
                public SetColorBox(Canvas canvas, Colour4 color) : base(canvas, color) { }

                protected override bool OnMouseDown(MouseDownEvent e)
                {
                    if (base.OnMouseDown(e)) return true;

                    if (e.Button == MouseButton.Left) Canvas.MainColor = new Rgba32(Color.R, Color.G, Color.B);
                    else if (e.Button == MouseButton.Right) Canvas.SecondaryColor = new Rgba32(Color.R, Color.G, Color.B);

                    return false;
                }
            }
        }
    }
}