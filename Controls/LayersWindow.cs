using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;

namespace DeloP.Controls
{
    public class LayersWindow : CompositeDrawable
    {
        readonly FullCanvas Canvas;

        public LayersWindow(FullCanvas canvas) => Canvas = canvas;

        [BackgroundDependencyLoader]
        void Load()
        {
            var layers = new FillFlowContainer<LayerControl>()
            {
                RelativeSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
                Spacing = new osuTK.Vector2(2),
            };
            Canvas.Canvas.Image.ActiveLayerBindable.ValueChanged += e =>
            {
                foreach (var child in layers)
                    child.Background.Colour = Colour4.Transparent;

                if (layers.Count > e.NewValue)
                    layers.Children[e.NewValue].Background.Colour = Colors.DarkBackground;
            };

            var grid = new GridContainer()
            {
                RelativeSizeAxes = Axes.Both,
                RowDimensions = new Dimension[]
                {
                    new Dimension(GridSizeMode.Absolute, 50),
                    new Dimension(GridSizeMode.Distributed),
                    new Dimension(GridSizeMode.Absolute, 50),
                },
                Content = new Drawable?[][]
                {
                    new Drawable?[] { null },
                    new Drawable?[] { layers },
                    new Drawable?[] { new LayerWindowButtons(Canvas) { RelativeSizeAxes = Axes.Both } },
                },
            };

            AddInternal(new Box() { RelativeSizeAxes = Axes.Both, Colour = Colors.Background });
            AddInternal(grid);


            Canvas.Canvas.Image.Layers.BindCollectionChanged((_, e) =>
            {
                layers.Clear();
                layers.AddRange(Canvas.Canvas.Image.Layers.Select(x =>
                {
                    var control = new LayerControl(x) { RelativeSizeAxes = Axes.X, Height = 50 };
                    control.ClickEvent += () => Canvas.Canvas.Image.ActiveLayer = x;

                    return control;
                }));

                Canvas.Canvas.Image.ActiveLayerBindable.TriggerChange();
            }, true);
        }

        protected override bool Handle(UIEvent e) => true;
        protected override void OnDrag(DragEvent e)
        {
            Position += e.Delta;
            base.OnDrag(e);
        }


        class LayerWindowButtons : CompositeDrawable
        {
            readonly FullCanvas Canvas;

            public LayerWindowButtons(FullCanvas canvas) => Canvas = canvas;

            [BackgroundDependencyLoader]
            void Load()
            {
                InternalChild = new FillFlowContainer()
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    Direction = FillDirection.Horizontal,
                    Children = new Drawable[]
                    {
                        new IconButton() { Icon = FontAwesome.Solid.File, Action = Canvas.Canvas.Image.AddLayer },
                        new IconButton() { Icon = FontAwesome.Solid.Trash, Action = Canvas.Canvas.Image.DeleteActiveLayer },
                    },
                };
            }


            /*class IconButton : Button
            {
                public IconUsage Icon{set=>Sprite.}
                readonly Sprite Sprite;

                public IconButton()
                {
                    FontAwesome.Solid.Ad.Icon
                    Sprite = new() { RelativeSizeAxes = Axes.Both };
                }
            }*/
            class IconButton : BasicButton
            {
                public IconUsage Icon { set => Text = value.Icon.ToString(); }

                [BackgroundDependencyLoader]
                void Load() => AutoSizeAxes = Axes.Both;
            }
        }
        class LayerControl : CompositeDrawable
        {
            public event Action ClickEvent = delegate { };

            public readonly Box Background = new Box() { RelativeSizeAxes = Axes.Both };
            readonly Layer Layer;

            public LayerControl(Layer layer) => Layer = layer;

            [BackgroundDependencyLoader]
            void Load()
            {
                AddInternal(Background);
                AddInternal(new GridContainer()
                {
                    RelativeSizeAxes = Axes.Both,
                    ColumnDimensions = new Dimension[]
                    {
                        new Dimension(),
                        new Dimension(),
                    },
                    Content = new Drawable[][]
                    {
                        new Drawable[]
                        {
                            new DeloCheckbox() { Anchor = Anchor.CentreLeft, Origin = Anchor.CentreLeft }.With(x => { x.Current.Value = Layer.IsVisible.Value; x.Current.BindTo(Layer.IsVisible); }),
                            new DeloTextBox() { RelativeSizeAxes = Axes.Both, Anchor = Anchor.CentreLeft, Origin = Anchor.CentreLeft, Text = "New layer" }.With(x => x.Current.BindTo(Layer.Name)),
                        }
                    },
                });
            }

            protected override bool OnMouseDown(MouseDownEvent e)
            {
                ClickEvent();
                return base.OnMouseDown(e);
            }
        }
    }
}