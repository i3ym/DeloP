using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace DeloP
{
    public readonly struct Constrained
    {
        readonly Image<Rgba32> Image;

        public Constrained(Image<Rgba32> image) => Image = image;

        public Rgba32 this[int x, int y]
        {
            get
            {
                if (!CheckConstraints(x, y)) return Rgba32.Transparent;
                return Image[x, y];
            }
            set
            {
                if (!CheckConstraints(x, y)) return;
                Image[x, y] = value;
            }
        }

        public bool CheckConstraints(int x, int y) => x >= 0 && y >= 0 && x < Image.Width && y < Image.Height;
    }
}