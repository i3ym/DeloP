using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Framework.Layout;
using SkiaSharp;

namespace DeloP.Controls
{
    public class Canvas : CompositeDrawable
    {
        public event Action LayoutInvalidateEvent = delegate { };
        public event Action<ScrollEvent> ScrollEvent = delegate { };

        public SKColor MainColor { get => MainColorBindable.Value; set => MainColorBindable.Value = value; }
        public SKColor SecondaryColor { get => SecondaryColorBindable.Value; set => SecondaryColorBindable.Value = value; }
        public ITool CurrentTool { get => CurrentToolBindable.Value; set => CurrentToolBindable.Value = value; }

        public readonly Bindable<SKColor> MainColorBindable = new Bindable<SKColor>();
        public readonly Bindable<SKColor> SecondaryColorBindable = new Bindable<SKColor>();
        public readonly Bindable<ITool> CurrentToolBindable = new Bindable<ITool>();

        public readonly DeloImage Image = new DeloImage();
        public SKCanvas OverlayCanvas => Image.OverlayCanvas;
        public SKBitmap OverlayImage => Image.OverlayImage;

        [BackgroundDependencyLoader]
        void Load()
        {
            AutoSizeAxes = Axes.Both;
            InternalChild = Image;

            MainColor = new SKColor(0, 0, 0);
            SecondaryColor = new SKColor(255, 255, 255);

            Image.Resize(0, 0, 1000, 1000);
            Image.ActiveLayer.Canvas.Clear(SecondaryColor);
            Image.ScrollEvent += e => ScrollEvent(e);
        }

        public void ChangeSize(int width, int height) => ChangeSize(0, 0, width, height);
        public void ChangeSize(int dx, int dy, int width, int height) => Image.Resize(dx, dy, width, height);

        protected override bool OnInvalidate(Invalidation invalidation, InvalidationSource source)
        {
            if ((invalidation & Invalidation.Layout) != 0) LayoutInvalidateEvent();
            return base.OnInvalidate(invalidation, source);
        }

        public (int x, int y) ToImageFromScreen(int screenMousex, int screenMousey) =>
            (
                (int) ((screenMousex - ScreenSpaceDrawQuad.TopLeft.X) / Scale.X / Image.DrawWidth * Image.Width),
                (int) ((screenMousey - ScreenSpaceDrawQuad.TopLeft.Y) / Scale.Y / Image.DrawHeight * Image.Height)
            );
    }
}