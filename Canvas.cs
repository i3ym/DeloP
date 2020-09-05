using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Events;
using osu.Framework.Platform;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.Primitives;

namespace Painter
{
    public class Canvas : CompositeDrawable
    {
        readonly IWindow Window;
        readonly Sprite Sprite;
        Image<Rgba32> Image;
        Texture Texture;
        bool IsDrawing = false;

        readonly GraphicsOptions Options = new GraphicsOptions(false);

        Rgba32 _CurrentColor;
        Rgba32 CurrentColor
        {
            get => _CurrentColor;
            set
            {
                _CurrentColor = value;
                Pen = new Pen(value, Pen.StrokeWidth);
            }
        }
        Pen Pen;

        public Canvas(IWindow window)
        {
            Window = window;
            Image = new Image<Rgba32>(Configuration.Default, 100, 100, Color.White);

            Height = Image.Height;
            Width = Image.Width;

            Sprite = new Sprite();
            Sprite.RelativeSizeAxes = Axes.Both;
            Sprite.Texture = Texture = new Texture(Image.Width, Image.Height, true, osuTK.Graphics.ES30.All.Nearest);

            Pen = new Pen(Color.Black, 1);
        }
        protected override void LoadComplete()
        {
            base.LoadComplete();

            InternalChild = Sprite;
            UpdateImage();
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            if (base.OnMouseDown(e)) return true;

            IsDrawing = true;

            var (x, y) = ToImagePosition((int) e.MouseDownPosition.X, (int) e.MouseDownPosition.Y);
            DrawLine(x, y, x, y);

            return false;
        }
        protected override void OnMouseUp(MouseUpEvent e)
        {
            base.OnMouseUp(e);
            IsDrawing = false;
        }
        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            if (base.OnMouseMove(e)) return true;
            if (!IsDrawing) return false;

            var (x, y) = ToImagePosition((int) e.LastMousePosition.X, (int) e.LastMousePosition.Y);
            var (tx, ty) = ToImagePosition((int) e.MousePosition.X, (int) e.MousePosition.Y);

            DrawLine(x, y, tx, ty);
            return false;
        }


        (int x, int y) ToImagePosition(int mousex, int mousey) =>
            (
                (int) (mousex / Scale.X / Sprite.DrawWidth * Image.Width),
                (int) (mousey / Scale.Y / Sprite.DrawHeight * Image.Height) + 1
            );
        bool DrawPixelWithoutUpdate(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Image.Width || y >= Image.Height) return false;
            if (Image[x, y] == CurrentColor) return false;

            Image[x, y] = CurrentColor;
            return true;
        }
        void DrawPixel(int x, int y)
        {
            if (DrawPixelWithoutUpdate(x, y))
                UpdateImage();
        }
        void DrawLine(int x, int y, int tx, int ty)
        {
            var points = new[] { new PointF(x, y), new PointF(tx, ty) };
            Image.Mutate(ctx => ctx.DrawLines(Options, Pen, points));

            UpdateImage();
        }
        void UpdateImage() => Texture.SetData(new TextureUpload(Image.Clone()));
    }
}