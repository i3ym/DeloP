using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Events;
using osu.Framework.Layout;
using osuTK;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

namespace DeloP.Controls
{
    public class Canvas : CompositeDrawable
    {
        public event Action LayoutInvalidateEvent = delegate { };
        public event Action<ScrollEvent> ScrollEvent = delegate { };

        readonly ScrollSprite Sprite;
        readonly Sprite OverlaySprite;

        readonly CachedTextureUpload TextureImageUpload = new CachedTextureUpload();
        readonly CachedTextureUpload OverlayTextureUpload = new CachedTextureUpload();

        public Image<Rgba32> Image
        {
            get => ImageBindable.Value;
            set
            {
                ((Bindable<Image<Rgba32>>) ImageBindable).Value = value;
                ((Bindable<Image<Rgba32>>) OverlayImageBindable).Value = new Image<Rgba32>(SixLabors.ImageSharp.Configuration.Default, Image.Width, Image.Height, Color.Transparent);

                Sprite.Texture = new Texture(Image.Width, Image.Height, true, osuTK.Graphics.ES30.All.Nearest);
                Sprite.Texture.TextureGL.BypassTextureUploadQueueing = true;

                OverlaySprite.Texture = new Texture(Image.Width, Image.Height, true, osuTK.Graphics.ES30.All.Nearest);
                OverlaySprite.Texture.TextureGL.BypassTextureUploadQueueing = true;

                TextureImageUpload.Image = Image;
                OverlayTextureUpload.Image = OverlayImage;

                UpdateImage();
            }
        }
        public Image<Rgba32> OverlayImage => OverlayImageBindable.Value;
        public Rgba32 MainColor { get => MainColorBindable.Value; set => MainColorBindable.Value = value; }
        public Rgba32 SecondaryColor { get => SecondaryColorBindable.Value; set => SecondaryColorBindable.Value = value; }
        public ITool CurrentTool { get => CurrentToolBindable.Value; set => CurrentToolBindable.Value = value; }

        public readonly IBindable<Image<Rgba32>> ImageBindable = new Bindable<Image<Rgba32>>();
        public readonly IBindable<Image<Rgba32>> OverlayImageBindable = new Bindable<Image<Rgba32>>();
        public readonly Bindable<Rgba32> MainColorBindable = new Bindable<Rgba32>();
        public readonly Bindable<Rgba32> SecondaryColorBindable = new Bindable<Rgba32>();
        public readonly Bindable<ITool> CurrentToolBindable = new Bindable<ITool>();

        public Canvas()
        {
            Sprite = new ScrollSprite(e => ScrollEvent(e)) { RelativeSizeAxes = Axes.Both, Anchor = Anchor.Centre, Origin = Anchor.Centre };
            OverlaySprite = new Sprite() { RelativeSizeAxes = Axes.Both, Anchor = Anchor.Centre, Origin = Anchor.Centre };
        }

        [BackgroundDependencyLoader]
        void Load()
        {
            AddInternal(Sprite);
            AddInternal(OverlaySprite);

            Image = new Image<Rgba32>(SixLabors.ImageSharp.Configuration.Default, 1000, 1000, Color.White);
            UpdateImage();

            MainColor = Color.Black;
            SecondaryColor = Color.White;
        }


        int i = 0;
        protected override void Update()
        {
            i++;
            base.Update();

            if (DoUpdateImage)
            {
                DoUpdateImage = false;

                if (ImageUpdateRect.HasValue)
                {
                    var b = TextureImageUpload.Bounds;
                    TextureImageUpload.Bounds = ImageUpdateRect.Value;
                    Sprite.Texture.SetData(TextureImageUpload);

                    TextureImageUpload.Bounds = b;
                    ImageUpdateRect = null;
                }
                else Sprite.Texture.SetData(TextureImageUpload);
            }
            if (DoUpdateOverlay)
            {
                DoUpdateOverlay = false;

                if (OverlayUpdateRect.HasValue)
                {
                    var b = OverlayTextureUpload.Bounds;
                    OverlayTextureUpload.Bounds = OverlayUpdateRect.Value;
                    OverlaySprite.Texture.SetData(TextureImageUpload);

                    OverlayTextureUpload.Bounds = b;
                    OverlayUpdateRect = null;
                }
                else OverlaySprite.Texture.SetData(OverlayTextureUpload);
            }
        }

        public void ChangeSize(int width, int height) => ChangeSize(0, 0, width, height);
        public void ChangeSize(int dx, int dy, int width, int height)
        {
            var newimg = new Image<Rgba32>(SixLabors.ImageSharp.Configuration.Default, width, height, Rgba32.White);
            newimg.Mutate(ctx => ctx.DrawImage(Image, new Point(dx, dy), 1f));
            Image = newimg;

            UpdateImage();
        }

        protected override bool OnInvalidate(Invalidation invalidation, InvalidationSource source)
        {
            if ((invalidation & Invalidation.Layout) != 0) LayoutInvalidateEvent();
            return base.OnInvalidate(invalidation, source);
        }

        public (int x, int y) ToImageFromScreen(int screenMousex, int screenMousey) =>
            (
                (int) ((screenMousex - ScreenSpaceDrawQuad.TopLeft.X) / Scale.X / Sprite.DrawWidth * Image.Width),
                (int) ((screenMousey - ScreenSpaceDrawQuad.TopLeft.Y) / Scale.Y / Sprite.DrawHeight * Image.Height)
            );

        bool DoUpdateImage, DoUpdateOverlay;
        RectangleI? ImageUpdateRect, OverlayUpdateRect;


        public void UpdateImage() => DoUpdateImage = true;
        public void UpdateImage(RectangleI bounds)
        {
            DoUpdateImage = true;
            ImageUpdateRect = bounds;
        }
        public void UpdateOverlay() => DoUpdateOverlay = true;


        class ScrollSprite : Sprite
        {
            readonly Action<ScrollEvent> ScrollAction;

            public ScrollSprite(Action<ScrollEvent> scrollAction) => ScrollAction = scrollAction;

            protected override bool OnScroll(ScrollEvent e)
            {
                ScrollAction(e);
                return base.OnScroll(e);
            }
        }
    }
}