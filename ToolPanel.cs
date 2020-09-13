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

namespace Painter
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

                    OutBorderMultiplier = 2f;
                    BackgroundBox.Texture = Textures.SelectedTool.Value;
                    BackgroundBox.Colour = Colors.ToolSelection;
                    DarkBorderBox.Alpha = BackgroundBox.Alpha = 0;

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
                    catch { Console.WriteLine("res.sprites." + tool.SpriteName + ".png   was not found"); }

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
                     .Append(Colour4.White).Append(Colour4.Black).Append(Colour4.DarkGray).Append(Colour4.Gray)
                     .Append(Colour4.Red).Append(Colour4.Green).Append(Colour4.Blue)
                     .Select(x => new SetColorBox(canvas, x));

                GColors = new CubeGrid(canvas, colors);
                GColors.RelativeSizeAxes = Axes.X;
                AddInternal(GColors);
            }

            protected override bool OnInvalidate(Invalidation invalidation, InvalidationSource source)
            {
                CColors.Height = CColors.DrawWidth;
                GColors.Y = CColors.DrawHeight;
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

                    public ChooseColorBox(Canvas canvas, Colour4 color) : base(color) => Canvas = canvas;

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

                public ColorBox(Colour4 color) : base(Texture.WhitePixel) => Sprite.Colour = Color = color;
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
            protected float OutBorderMultiplier = 1f;
            protected float InBorderMultiplier = 2f;

            public SpriteBox(Texture texture)
            {
                AddInternal(BackgroundBox = new Sprite() { Texture = Texture.WhitePixel, Colour = Colors.Background, RelativeSizeAxes = Axes.Both });
                AddInternal(DarkBorderBox = new Sprite() { Texture = Texture.WhitePixel, Colour = Colors.DarkBackground });
                AddInternal(Sprite = new Sprite() { Texture = texture });

                BackgroundBox.X = BackgroundBox.Y = 0;
            }

            protected override bool OnInvalidate(Invalidation invalidation, InvalidationSource source)
            {
                var outb = (int) DrawWidth / (11 * OutBorderMultiplier);
                var inb = (int) DrawWidth / (11 * InBorderMultiplier);

                Sprite.X = Sprite.Y = outb + inb;
                DarkBorderBox.X = DarkBorderBox.Y = outb;

                DarkBorderBox.Width = DarkBorderBox.Height = BackgroundBox.DrawWidth - (outb * 2);
                Sprite.Width = Sprite.Height = DarkBorderBox.DrawWidth - (inb * 2);

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

            protected override bool OnInvalidate(Invalidation invalidation, InvalidationSource source)
            {
                foreach (var content in Grid.Content)
                    foreach (var child in content)
                        child.Width = child.Height = DrawWidth / 2;

                Height = Grid.Content.Length * DrawWidth / 2;
                return base.OnInvalidate(invalidation, source);
            }
        }
    }
}