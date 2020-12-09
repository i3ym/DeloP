using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Layout;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace DeloP.Controls
{
    public class Canvas : CompositeDrawable
    {
        public event Action LayoutInvalidateEvent = delegate { };
        public event Action<ScrollEvent> ScrollEvent = delegate { };

        readonly Sprite OverlaySprite;
        readonly CachedTextureUpload OverlayTextureUpload = new CachedTextureUpload();
        Image<Rgba32> _OverlayImage = null!;
        public Image<Rgba32> OverlayImage
        {
            get => _OverlayImage;
            private set
            {
                _OverlayImage = value;
                OverlayTextureUpload.Image = value;
                UpdateOverlaySprite();
            }
        }

        public readonly DeloImage Image;
        public Rgba32 MainColor { get => MainColorBindable.Value; set => MainColorBindable.Value = value; }
        public Rgba32 SecondaryColor { get => SecondaryColorBindable.Value; set => SecondaryColorBindable.Value = value; }
        public ITool CurrentTool { get => CurrentToolBindable.Value; set => CurrentToolBindable.Value = value; }

        public readonly Bindable<Rgba32> MainColorBindable = new Bindable<Rgba32>();
        public readonly Bindable<Rgba32> SecondaryColorBindable = new Bindable<Rgba32>();
        public readonly Bindable<ITool> CurrentToolBindable = new Bindable<ITool>();

        public Canvas()
        {
            Image = new DeloImage() { Width = 1000, Height = 1000 };
            OverlaySprite = new Sprite() { RelativeSizeAxes = Axes.Both, Anchor = Anchor.Centre, Origin = Anchor.Centre };
            OverlayImage = new Image<Rgba32>(SixLabors.ImageSharp.Configuration.Default, (int) Image.Width, (int) Image.Height, Color.Transparent);
        }

        [BackgroundDependencyLoader]
        void Load()
        {
            AutoSizeAxes = Axes.Both;

            AddInternal(Image);
            AddInternal(OverlaySprite);

            MainColor = Color.Black;
            SecondaryColor = Color.White;

            Image.AddLayer(SecondaryColor);
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


        bool DoUpdateOverlay; // TODO: move into DeloImage
        RectangleI? OverlayUpdateRect;
        public void UpdateOverlaySprite() => DoUpdateOverlay = true;
    }
}