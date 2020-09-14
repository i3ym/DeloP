using osu.Framework.Graphics.Textures;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace DeloP
{
    public class NoDisposeTextureUpload : TextureUpload
    {
        public NoDisposeTextureUpload(Image<Rgba32> image) : base(image) { }

        protected override void Dispose(bool _) { }
    }
}