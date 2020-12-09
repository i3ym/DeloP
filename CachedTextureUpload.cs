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

        Image<Rgba32>? _Image;
        public Image<Rgba32>? Image
        {
            get => _Image;
            set
            {
                if (value is null) throw new NullReferenceException();

                _Image = value;
                Bounds = new RectangleI(0, 0, value.Width, value.Height);
            }
        }

        void IDisposable.Dispose() { }
    }
}