using System;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Textures;
using osuTK.Graphics.ES30;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;

namespace DeloP
{
    public class CachedTextureUpload : ITextureUpload
    {
        public ReadOnlySpan<Rgba32> Data => Image.GetPixelSpan();
        public int Level { get; } = 0;
        public RectangleI Bounds { get; set; }
        public PixelFormat Format => PixelFormat.Rgba;

        readonly Image<Rgba32> Image;

        public CachedTextureUpload(Image<Rgba32> image)
        {
            Image = image;
            Bounds = new RectangleI(0, 0, Image.Width, Image.Height);
        }

        public void Dispose() { }
    }
}