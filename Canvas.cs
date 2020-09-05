using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Events;
using osu.Framework.Platform;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Painter
{
    public class Canvas : CompositeDrawable
    {
        readonly IWindow Window;
        readonly Sprite Sprite, OverlaySprite;
        public Image<Rgba32> Image { get; private set; }
        public Image<Rgba32> OverlayImage { get; private set; }
        bool IsDrawing = false;

        public Rgba32 CurrentColor;
        public ITool CurrentTool = new PencilTool();

        public Canvas(IWindow window)
        {
            Window = window;
            Image = new Image<Rgba32>(Configuration.Default, 100, 100, Color.White);
            OverlayImage = new Image<Rgba32>(Configuration.Default, 100, 100, Color.Transparent);

            Height = Image.Height;
            Width = Image.Width;

            Sprite = new Sprite();
            Sprite.RelativeSizeAxes = Axes.Both;
            Sprite.Texture = new Texture(Image.Width, Image.Height, true, osuTK.Graphics.ES30.All.Nearest);

            OverlaySprite = new Sprite();
            OverlaySprite.RelativeSizeAxes = Axes.Both;
            OverlaySprite.Texture = new Texture(OverlayImage.Width, OverlayImage.Height, true, osuTK.Graphics.ES30.All.Nearest);

            CurrentColor = Color.Black;
        }
        protected override void LoadComplete()
        {
            base.LoadComplete();

            InternalChildren = new Drawable[]
            {
                Sprite,
                OverlaySprite
            };

            UpdateImage();
        }

        #region move listener

        int LastMouseX, LastMouseY;
        protected override bool OnMouseDown(MouseDownEvent e)
        {
            if (base.OnMouseDown(e)) return true;

            IsDrawing = true;
            (LastMouseX, LastMouseY) = ((int) e.MouseDownPosition.X, (int) e.MouseDownPosition.Y);

            var (x, y) = ToImagePosition((int) e.MouseDownPosition.X, (int) e.MouseDownPosition.Y);
            CurrentTool.OnStart(x, y, this);

            return false;
        }
        protected override void OnMouseUp(MouseUpEvent e)
        {
            base.OnMouseUp(e);
            IsDrawing = false;

            var (sx, sy) = ToImagePosition((int) e.MouseDownPosition.X, (int) e.MouseDownPosition.Y);
            var (ex, ey) = ToImagePosition((int) e.MousePosition.X, (int) e.MousePosition.Y);
            CurrentTool.OnEnd(sx, sy, ex, ey, this);
        }
        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            if (base.OnMouseMove(e)) return true;
            if (!IsDrawing) return false;

            var (x, y) = ToImagePosition((int) LastMouseX, (int) LastMouseY);
            var (tx, ty) = ToImagePosition((int) e.MousePosition.X, (int) e.MousePosition.Y);

            (LastMouseX, LastMouseY) = ((int) e.MousePosition.X, (int) e.MousePosition.Y);
            CurrentTool.OnMove(x, y, tx, ty, this);
            return false;
        }

        #endregion

        (int x, int y) ToImagePosition(int mousex, int mousey) =>
            (
                (int) (mousex / Scale.X / Sprite.DrawWidth * Image.Width),
                (int) (mousey / Scale.Y / Sprite.DrawHeight * Image.Height) + 1
            );
        public bool DrawPixelWithoutUpdate(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Image.Width || y >= Image.Height) return false;
            if (Image[x, y] == CurrentColor) return false;

            Image[x, y] = CurrentColor;
            return true;
        }
        public void DrawPixel(int x, int y)
        {
            if (DrawPixelWithoutUpdate(x, y))
                UpdateImage();
        }

        public void UpdateImage() => Sprite.Texture.SetData(new TextureUpload(Image.Clone()));
        public void UpdateOverlay() => OverlaySprite.Texture.SetData(new TextureUpload(OverlayImage.Clone()));
    }
}