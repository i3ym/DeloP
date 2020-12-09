using osu.Framework.Bindables;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using SixLabors.ImageSharp.PixelFormats;
using Img = SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>;

namespace DeloP
{
    public class Layer
    {
        public bool DoUpdate = false;
        public RectangleI? UpdateBounds;

        public readonly Sprite Sprite;
        public readonly Img Image;
        public readonly ITextureUpload Upload;
        public readonly Bindable<bool> IsVisible = new Bindable<bool>(true);
        public readonly Bindable<string> Name = new Bindable<string>(string.Empty);

        public Layer(Sprite sprite, Img image, ITextureUpload upload)
        {
            Sprite = sprite;
            Image = image;
            Upload = upload;
        }

        public Rgba32 this[int x, int y] { get => Image[x, y]; set => Image[x, y] = value; }

        public void ScheduleUpdate(RectangleI? bounds = null)
        {
            DoUpdate = true;
            UpdateBounds = bounds;
        }
    }
}