using System;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Painter
{
    public static class SpriteStore
    {
        public static Image<Rgba32> Load(string path) => Image.Load<Rgba32>(LoadStream(path));
        public static Stream LoadStream(string path) =>
            typeof(SpriteStore).Assembly.GetManifestResourceStream("Painter." + path) ?? throw new NullReferenceException();
    }
}