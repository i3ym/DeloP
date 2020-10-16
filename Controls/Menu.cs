using System.Linq;
using System;
using System.Collections.Generic;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Layout;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;

namespace DeloP.Controls
{
    public class Menu : CompositeDrawable
    {
        [Resolved] Game Game { get; set; } = null!;

        readonly Canvas Canvas;

        public Menu(Canvas canvas) => Canvas = canvas;

        [BackgroundDependencyLoader]
        private void Load()
        {
            AddInternal(new Box() { RelativeSizeAxes = Axes.Both, Colour = Colors.Background });
            AddInternal(new FillFlowContainer()
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    new TopMenuEntry("Файл",
                        ("Новый", ClickNew),
                        ("Коровый", () => Console.WriteLine("чё смотриш пёс")),
                        default,
                        ("Выйти", Game.Exit)),

                    new TopMenuEntry("Правка")
                }
            });
        }

        void ClickNew()
        {
            Canvas.Image.GetPixelSpan().Fill(Color.White);
            Canvas.UpdateImage();
        }


        class HoverAnimationCompositeDrawable : CompositeDrawable
        {
            protected const int AnimationTime = 70;

            protected readonly Box Background = new Box() { RelativeSizeAxes = Axes.Both, Colour = new Colour4(1f, 1f, 1f, .2f), Alpha = 0 };

            [BackgroundDependencyLoader]
            private void Load() => AddInternal(Background);

            protected override bool OnHover(HoverEvent e)
            {
                Background.FadeInFromZero(AnimationTime);
                return base.OnHover(e);
            }
            protected override void OnHoverLost(HoverLostEvent e)
            {
                Background.FadeOutFromOne(AnimationTime);
                base.OnHoverLost(e);
            }
            protected override bool OnMouseDown(MouseDownEvent e)
            {
                Background.FadeColour(new Colour4(0f, 0f, 0f, .2f), AnimationTime);
                return base.OnMouseDown(e);
            }
            protected override void OnMouseUp(MouseUpEvent e)
            {
                Background.FadeColour(new Colour4(1f, 1f, 1f, .2f), AnimationTime);
                base.OnMouseUp(e);
            }

            protected override bool Handle(UIEvent e) => true;
        }
        class TopMenuEntry : HoverAnimationCompositeDrawable
        {
            readonly Bindable<bool> IsOpenBindable = new Bindable<bool>(false);
            public bool IsOpen { get => IsOpenBindable.Value; set => IsOpenBindable.Value = value; }

            readonly string Text;
            readonly IReadOnlyList<(string? text, Action? onClick)> Items;
            readonly BackgroundPanel Panel = new BackgroundPanel(Colors.Background) { BypassAutoSizeAxes = Axes.Both, Anchor = Anchor.BottomLeft, Depth = -3, Scale = new osuTK.Vector2(1, 0) };

            public TopMenuEntry(string text, params (string? text, Action? onClick)[] items)
            {
                Text = text;
                Items = items;

                Panel.Container.Direction = FillDirection.Vertical;
            }

            [BackgroundDependencyLoader]
            void Load()
            {
                IsOpenBindable.ValueChanged += v => Panel.ScaleTo(v.NewValue ? 1 : 0, AnimationTime, Easing.OutQuad);

                RelativeSizeAxes = Axes.Y;
                AutoSizeAxes = Axes.X;

                AddInternal(new SpriteText()
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Text = Text,
                    Font = FontUsage.Default.With(family: "Ubuntu", size: 18),
                    Margin = new MarginPadding() { Horizontal = 20 }
                });

                foreach (var (text, onClick) in Items)
                    if (text is { } && onClick is { })
                        Panel.Container.Add(new MenuEntry(this, text, onClick));
                    else { } // TODO: divider

                AddInternal(Panel);
            }

            protected override bool OnClick(ClickEvent e)
            {
                IsOpen = !IsOpen;
                return base.OnClick(e);
            }
            protected override bool OnDoubleClick(DoubleClickEvent e)
            {
                IsOpen = !IsOpen;
                return base.OnDoubleClick(e);
            }


            class BackgroundPanel : CompositeDrawable
            {
                public readonly FillFlowContainer Container = new FillFlowContainer() { AutoSizeAxes = Axes.Both };
                readonly Colour4 BackgroundColour;

                public BackgroundPanel(Colour4 background) => BackgroundColour = background;

                [BackgroundDependencyLoader]
                private void Load()
                {
                    AutoSizeAxes = Axes.Both;

                    AddInternal(new Box() { RelativeSizeAxes = Axes.Both, Colour = BackgroundColour });
                    AddInternal(Container);
                }
            }


            class MenuEntry : HoverAnimationCompositeDrawable
            {
                readonly TopMenuEntry Entry;
                readonly string Text;
                readonly Action ClickEvent;
                readonly SpriteText SpriteText;

                public MenuEntry(TopMenuEntry entry, string text, Action onClick)
                {
                    Entry = entry;
                    Text = text;
                    ClickEvent = onClick;

                    SpriteText = new SpriteText()
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Text = Text,
                        Font = FontUsage.Default.With(family: "Ubuntu", size: 18),
                        Margin = new MarginPadding() { Horizontal = 20 }
                    };
                }

                [BackgroundDependencyLoader]
                void Load()
                {
                    Height = 24;

                    AddInternal(new Box() { RelativeSizeAxes = Axes.Both, Colour = Colors.Background, Depth = 2 });
                    AddInternal(SpriteText);
                }

                protected override bool OnInvalidate(Invalidation invalidation, InvalidationSource source)
                {
                    if ((invalidation & Invalidation.Layout) != 0)
                        Width = Entry.Panel.Container.Children.OfType<MenuEntry>().Max(x => x.SpriteText.Width + x.SpriteText.Margin.Left + x.SpriteText.Margin.Right);

                    return base.OnInvalidate(invalidation, source);
                }

                protected override bool OnClick(ClickEvent e)
                {
                    ClickEvent();
                    Entry.IsOpen = false;

                    return base.OnClick(e);
                }
            }
        }
    }
}