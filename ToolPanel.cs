using System.Collections.Generic;
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

namespace DeloP
{
    public class ToolPanel : CompositeDrawable
    {
        readonly Canvas Canvas;
        readonly Container Container;
        readonly ToolsContainer CTools;
        readonly ColorsContainer CColors;

        public ToolPanel(Canvas canvas)
        {
            Canvas = canvas;

            Container = new Container();
            CTools = new ToolsContainer(Canvas) { RelativeSizeAxes = Axes.X };
            CColors = new ColorsContainer(Canvas) { RelativeSizeAxes = Axes.X };
        }
        protected override void LoadComplete()
        {
            base.LoadComplete();

            Container.Padding = new MarginPadding(2);
            Container.RelativeSizeAxes = Axes.Both;
            Container.Add(CTools);
            Container.Add(CColors);

            InternalChildren = new Drawable[]
            {
                new Box() { Colour = Colors.Background, RelativeSizeAxes = Axes.Both },
                Container
            };
        }

        protected override bool OnInvalidate(Invalidation invalidation, InvalidationSource source)
        {
            CColors.Y = CTools.DrawHeight + 10;
            return base.OnInvalidate(invalidation, source);
        }


        class ToolsContainer : CompositeDrawable
        {
            public ToolsContainer(Canvas canvas)
            {
                var tools = Enumerable.Empty<ITool>()
                    .Append(new MoveTool())
                    .Append(new PencilTool())
                    .Append(new EraserTool())
                    .Append(new PipetteTool())
                    .Append(new LineTool())
                    .Append(new RectangleTool())
                    .Append(new TriangleTool())
                    .Append(new FillTool())
                    .Select(x => { x.Thickness = 5f; return x; })
                    .Select(x => new ToolDrawable(canvas, x));


                var grid = new CubeGrid(canvas, tools);
                grid.RelativeSizeAxes = Axes.X;
                InternalChild = grid;

                canvas.CurrentTool = tools.First().Tool;
            }

            protected override bool OnInvalidate(Invalidation invalidation, InvalidationSource source)
            {
                Height = InternalChild.DrawHeight;
                return base.OnInvalidate(invalidation, source);
            }


            class ToolDrawable : SpriteBox
            {
                readonly Canvas Canvas;
                public readonly ITool Tool;
                bool IsSelected = false;

                public ToolDrawable(Canvas canvas, ITool tool) : base(LoadTexture(tool))
                {
                    Canvas = canvas;
                    Tool = tool;

                    BackgroundBox.Texture = Textures.SelectedTool.Value;
                    BackgroundBox.Colour = Colors.ToolSelection;
                    DarkBorderBox.Alpha = BackgroundBox.Alpha = 0;

                    OutBorder = 1;
                    InBorder = 5;
                    Sprite.Width = Sprite.Height = 16;

                    canvas.OnSetTool += t =>
                    {
                        IsSelected = t == Tool;
                        DarkBorderBox.Alpha = BackgroundBox.Alpha = IsSelected ? 1 : 0;
                    };
                }

                static Texture LoadTexture(ITool tool)
                {
                    try
                    {
                        var img = SpriteStore.Load("res.sprites." + tool.SpriteName + ".png");
                        var texture = new Texture(img.Width, img.Height, true, osuTK.Graphics.ES30.All.Nearest);
                        texture.SetData(new TextureUpload(img));

                        return texture;
                    }
                    catch { Console.WriteLine("Warning: res.sprites." + tool.SpriteName + ".png was not found"); }

                    return Texture.WhitePixel;
                }

                protected override bool OnHover(HoverEvent e)
                {
                    BackgroundBox.Alpha = 1;
                    return base.OnHover(e);
                }
                protected override void OnHoverLost(HoverLostEvent e)
                {
                    if (!IsSelected) BackgroundBox.Alpha = 0;
                    base.OnHoverLost(e);
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
            readonly CubeGrid GColors;
            readonly Canvas Canvas;

            public ColorsContainer(Canvas canvas)
            {
                Canvas = canvas;

                CColors = new CurrentColors(canvas);
                CColors.RelativeSizeAxes = Axes.X;
                AddInternal(CColors);


                var colors = Enumerable.Empty<Colour4>()
                    .Append(Colour4.White).Append(Colour4.Black)
                    .Append(new Colour4(195, 195, 195, 255))
                    .Append(new Colour4(127, 127, 127, 255))
                    .Append(new Colour4(185, 122, 87, 255))
                    .Append(new Colour4(136, 0, 21, 255))
                    .Append(new Colour4(255, 174, 201, 255))
                    .Append(new Colour4(237, 28, 36, 255))
                    .Append(new Colour4(255, 201, 14, 255))
                    .Append(new Colour4(255, 127, 39, 255))
                    .Append(new Colour4(239, 228, 176, 255))
                    .Append(new Colour4(255, 242, 0, 255))
                    .Append(new Colour4(181, 230, 29, 255))
                    .Append(new Colour4(34, 177, 76, 255))
                    .Append(new Colour4(153, 217, 234, 255))
                    .Append(new Colour4(0, 162, 232, 255))
                    .Append(new Colour4(112, 146, 190, 255))
                    .Append(new Colour4(63, 72, 204, 255))
                    .Append(new Colour4(200, 191, 231, 255))
                    .Append(new Colour4(163, 73, 164, 255))
                    .Select(x => new SetColorBox(canvas, x));

                GColors = new CubeGrid(canvas, colors, new MarginPadding(6));
                GColors.RelativeSizeAxes = Axes.X;
                AddInternal(GColors);
            }

            protected override bool OnInvalidate(Invalidation invalidation, InvalidationSource source)
            {
                CColors.Height = CColors.DrawWidth;
                GColors.Y = CColors.DrawHeight - 15;
                return base.OnInvalidate(invalidation, source);
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
                    public Colour4 InternalColour { get => Sprite.Colour; set => Sprite.Colour = value; }

                    protected readonly Canvas Canvas;

                    public ChooseColorBox(Canvas canvas, Colour4 color) : base(color)
                    {
                        Canvas = canvas;

                        OutBorder = 3;
                        InBorder = 1;
                        Sprite.Width = Sprite.Height = 26;
                    }

                    protected override bool OnMouseDown(MouseDownEvent e)
                    {
                        // TODO color chooser
                        return base.OnMouseDown(e);
                    }
                }
            }

            abstract class ColorBox : SpriteBox
            {
                protected readonly Colour4 Color;

                public ColorBox(Colour4 color) : base(Texture.WhitePixel)
                {
                    Sprite.Colour = Color = color;

                    OutBorder = 0;
                    InBorder = 1;
                    Sprite.Width = Sprite.Height = 18;
                }
            }
            class SetColorBox : ColorBox
            {
                protected readonly Canvas Canvas;

                public SetColorBox(Canvas canvas, Colour4 color) : base(color) => Canvas = canvas;

                protected override bool OnMouseDown(MouseDownEvent e)
                {
                    if (base.OnMouseDown(e)) return true;

                    if (e.Button == MouseButton.Left) Canvas.MainColor = new Rgba32(Color.R, Color.G, Color.B);
                    else if (e.Button == MouseButton.Right) Canvas.SecondaryColor = new Rgba32(Color.R, Color.G, Color.B);

                    return false;
                }
            }
        }

        abstract class SpriteBox : CompositeDrawable
        {
            protected readonly Sprite Sprite, DarkBorderBox, BackgroundBox;
            protected int InBorder = 1, OutBorder = 1;

            public SpriteBox(Texture texture)
            {
                AddInternal(BackgroundBox = new Sprite() { Texture = Texture.WhitePixel, Colour = Colors.Background, RelativeSizeAxes = Axes.Both });
                AddInternal(DarkBorderBox = new Sprite() { Texture = Texture.WhitePixel, Colour = Colors.DarkBackground });
                AddInternal(Sprite = new Sprite() { Texture = texture });

                BackgroundBox.X = BackgroundBox.Y = 0;
            }

            protected override bool OnInvalidate(Invalidation invalidation, InvalidationSource source)
            {
                Sprite.X = Sprite.Y = /*outb + inb +*/ DrawWidth / 2 - Sprite.Width / 2;
                DarkBorderBox.X = DarkBorderBox.Y = OutBorder;

                DarkBorderBox.Width = DarkBorderBox.Height = BackgroundBox.DrawWidth - (OutBorder * 2);
                // Sprite.Width = Sprite.Height = DarkBorderBox.DrawWidth - (inb * 2);
                Width = Height = Sprite.Width + (OutBorder + InBorder) * 2;

                return base.OnInvalidate(invalidation, source);
            }
        }
        class CubeGrid : CompositeDrawable
        {
            readonly GridContainer Grid;
            readonly Canvas Canvas;

            public CubeGrid(Canvas canvas, IEnumerable<Drawable> children)
            {
                Canvas = canvas;

                var content = children
                    .Select((x, i) => (x, i))
                    .GroupBy(x => x.i / 2)
                    .Select(x => x.Select(x => x.x).ToArray())
                    .ToArray();

                Grid = new GridContainer();
                Grid.RelativeSizeAxes = Axes.Both;
                Grid.Content = content;

                InternalChild = Grid;
            }
            public CubeGrid(Canvas canvas, IEnumerable<Drawable> children, MarginPadding padding) : this(canvas, children) =>
                Padding = padding;


            protected override bool OnInvalidate(Invalidation invalidation, InvalidationSource source)
            {
                foreach (var content in Grid.Content)
                    foreach (var child in content)
                        child.X = child.Y = DrawWidth / 2 / 2 - child.Width / 2 - (int) Padding.Left / 2;

                Height = Grid.Content.Length * DrawWidth / 2;
                return base.OnInvalidate(invalidation, source);
            }
        }
    }
}