using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
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
        public event Action<ITool> OnSetTool = delegate { };
        public event Action<Rgba32> OnSetMainColor = delegate { };
        public event Action<Rgba32> OnSetSecondaryColor = delegate { };
        public event Action<Image<Rgba32>> OnImageReplace = delegate { };
        public event Action LayoutInvalidateAction = delegate { };

        readonly Sprite Sprite, OverlaySprite;

        Image<Rgba32> _Image = null!;
        public Image<Rgba32> Image
        {
            get => _Image;
            private set
            {
                _Image = value;
                OverlayImage = new Image<Rgba32>(SixLabors.ImageSharp.Configuration.Default, Image.Width, Image.Height, Color.Transparent);

                Sprite.Texture = new Texture(Image.Width, Image.Height, true, osuTK.Graphics.ES30.All.Nearest);
                Sprite.Texture.TextureGL.BypassTextureUploadQueueing = true;
                OverlaySprite.Texture = new Texture(Image.Width, Image.Height, true, osuTK.Graphics.ES30.All.Nearest);
                OverlaySprite.Texture.TextureGL.BypassTextureUploadQueueing = true;

                TextureImageUpload = new CachedTextureUpload(Image);
                OverlayTextureUpload = new CachedTextureUpload(OverlayImage);

                OnImageReplace(value);
            }
        }
        public Image<Rgba32> OverlayImage { get; private set; } = null!;
        CachedTextureUpload TextureImageUpload = null!, OverlayTextureUpload = null!;

        float _Zoom = 1;
        public float Zoom
        {
            get => _Zoom;
            set
            {
                _Zoom = value;
                this.ResizeTo(new Vector2(Image.Width * value, Image.Height * value), 100, Easing.OutQuad);
            }
        }

        Rgba32 _MainColor = Color.Black;
        Rgba32 _SecondaryColor = Color.White;
        ITool _CurrentTool = new PencilTool();

        public Rgba32 MainColor
        {
            get => _MainColor;
            set
            {
                _MainColor = value;
                OnSetMainColor(value);
            }
        }
        public Rgba32 SecondaryColor
        {
            get => _SecondaryColor;
            set
            {
                _SecondaryColor = value;
                OnSetSecondaryColor(value);
            }
        }
        public ITool CurrentTool
        {
            get => _CurrentTool;
            set
            {
                _CurrentTool = value;
                OnSetTool(value);
            }
        }


        public Canvas()
        {
            Sprite = new Sprite();
            OverlaySprite = new Sprite();
            Sprite.RelativeSizeAxes = Axes.Both;
            OverlaySprite.RelativeSizeAxes = Axes.Both;
        }
        protected override void LoadComplete()
        {
            base.LoadComplete();

            InternalChildren = new Drawable[]
            {
                Sprite,
                OverlaySprite
            };

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
            if ((invalidation & Invalidation.Layout) != 0) LayoutInvalidateAction();
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
    }
}