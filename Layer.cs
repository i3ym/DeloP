using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using SkiaSharp;

namespace DeloP
{
    public class Layer : CompositeDrawable
    {
        readonly Sprite Sprite;
        public SKCanvas Canvas { get; private set; }
        public SKBitmap Image { get; private set; }
        public ITextureUpload Upload { get; private set; }

        public readonly Bindable<bool> IsVisible = new Bindable<bool>(true);
        public new readonly Bindable<string> Name = new Bindable<string>(string.Empty);

        bool DoUpdate;
        RectangleI? UpdateBounds;

        public Layer(SKBitmap image)
        {
            RelativeSizeAxes = Axes.Both;
            InternalChild = Sprite = new Sprite() { RelativeSizeAxes = Axes.Both };

            IsVisible.ValueChanged += e => Alpha = e.NewValue ? 1 : 0;

            Image = null!; Canvas = null!; Upload = null!;
            SetImage(image);
        }

        public SKColor this[int x, int y] { get => Image.GetPixel(x, y); set => Image.SetPixel(x, y, value); }

        public void SetImage(SKBitmap image, bool update = true)
        {
            Image = image;
            Canvas = new SKCanvas(image);
            Upload = new SkiaTextureUpload(image);

            Sprite.Texture = new Texture(image.Width, image.Height, true, osuTK.Graphics.ES30.All.Nearest);
            Sprite.Texture.TextureGL.BypassTextureUploadQueueing = true;

            if (update) ForceUpdate();
        }

        protected override void Update()
        {
            if (DoUpdate) ForceUpdate(UpdateBounds);
            base.Update();
        }

        void ForceUpdate(RectangleI? bounds = null)
        {
            if (!bounds.HasValue) Sprite.Texture.SetData(Upload);
            else
            {
                var b = Upload.Bounds;
                Upload.Bounds = bounds.Value;
                Sprite.Texture.SetData(Upload);

                Upload.Bounds = b;
            }

            DoUpdate = false;
            UpdateBounds = null;
        }
        public void ScheduleUpdate(RectangleI? bounds = null)
        {
            DoUpdate = true;
            UpdateBounds = bounds;
        }
    }
}