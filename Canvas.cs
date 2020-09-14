using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Layout;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

namespace DeloP
{
    public class Canvas : CompositeDrawable
    {
        public event Action<ITool> OnSetTool = delegate { };
        public event Action<Rgba32> OnSetMainColor = delegate { };
        public event Action<Rgba32> OnSetSecondaryColor = delegate { };
        public event Action<Image<Rgba32>> OnImageReplace = delegate { };
        public event Action OnMove = delegate { };

        readonly Sprite Sprite, OverlaySprite;

        Image<Rgba32> _Image = null!;
        public Image<Rgba32> Image
        {
            get => _Image;
            private set
            {
                _Image = value;
                OverlayImage = new Image<Rgba32>(Configuration.Default, Image.Width, Image.Height, Color.Transparent);

                Sprite.Texture = new Texture(Image.Width, Image.Height, true, osuTK.Graphics.ES30.All.Nearest);
                OverlaySprite.Texture = new Texture(Image.Width, Image.Height, true, osuTK.Graphics.ES30.All.Nearest);

                OnImageReplace(value);
            }
        }
        public Image<Rgba32> OverlayImage { get; private set; } = null!;

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

            Image = new Image<Rgba32>(Configuration.Default, 1000, 1000, Color.White);
            UpdateImage();

            MainColor = Color.Black;
            SecondaryColor = Color.White;
        }

        public void ChangeSize(int width, int height) => ChangeSize(0, 0, width, height);
        public void ChangeSize(int dx, int dy, int width, int height)
        {
            var newimg = new Image<Rgba32>(Configuration.Default, width, height, Rgba32.White);
            newimg.Mutate(ctx => ctx.DrawImage(Image, new Point(dx, dy), 1f));
            Image = newimg;

            UpdateImage();
        }

        protected override bool OnInvalidate(Invalidation invalidation, InvalidationSource source)
        {
            OnMove();
            return base.OnInvalidate(invalidation, source);
        }

        public (int x, int y) ToImagePosition(int mousex, int mousey) =>
            (
                (int) (mousex / Scale.X / Sprite.DrawWidth * Image.Width) - (int) DrawPosition.X,
                (int) (mousey / Scale.Y / Sprite.DrawHeight * Image.Height) + 1 - (int) DrawPosition.Y
            );

        public void UpdateImage() => Sprite.Texture.SetData(new NoDisposeTextureUpload(Image));
        public void UpdateOverlay() => OverlaySprite.Texture.SetData(new NoDisposeTextureUpload(OverlayImage));
    }
}