using SkiaSharp;

namespace DeloP
{
    public readonly struct Constrained
    {
        readonly SKBitmap Image;

        public Constrained(SKBitmap image) => Image = image;

        public SKColor this[int x, int y]
        {
            get
            {
                if (!CheckConstraints(x, y)) return SKColor.Empty;
                return Image.GetPixel(x, y);
            }
            set
            {
                if (!CheckConstraints(x, y)) return;
                Image.SetPixel(x, y, value);
            }
        }

        public bool CheckConstraints(int x, int y) => x >= 0 && y >= 0 && x < Image.Width && y < Image.Height;
    }
}