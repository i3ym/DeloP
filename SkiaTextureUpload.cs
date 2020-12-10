using System;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Textures;
using osuTK.Graphics.ES30;
using SixLabors.ImageSharp.PixelFormats;
using SkiaSharp;

namespace DeloP
{
    public class SkiaTextureUpload : ITextureUpload
    {
        public unsafe ReadOnlySpan<Rgba32> Data
        {
            get
            {
                var ptr = Image.GetPixels(out var length);
                return new Span<Rgba32>(ptr.ToPointer(), length.ToInt32());
            }
        }
        public int Level => 0;

        public RectangleI Bounds { get; set; }
        public PixelFormat Format => PixelFormat.Rgba;

        readonly SKBitmap Image;

        public SkiaTextureUpload(SKBitmap image)
        {
            Image = image;
            Bounds = new RectangleI(0, 0, image.Width, image.Height);
        }

        public void Dispose() { }
    }
}