using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Events;
using osu.Framework.Platform;
using osuTK.Input;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Painter
{
    public class Canvas : CompositeDrawable
    {
        public event Action<ITool> OnSetTool = delegate { };
        public event Action<Rgba32> OnSetMainColor = delegate { };
        public event Action<Rgba32> OnSetSecondaryColor = delegate { };

        readonly Sprite Sprite, OverlaySprite;
        public Image<Rgba32> Image { get; private set; }
        public Image<Rgba32> OverlayImage { get; private set; }

        Rgba32 _MainColor = Color.Black;
        Rgba32 _SecondaryColor = Color.White;
        ITool _CurrentTool = new PencilTool();

        public Rgba32 MainColor
        {
            get => _MainColor;
            set
            {
                _MainColor = value;
                OnSetMainColor(value);
            }
        }
        public Rgba32 SecondaryColor
        {
            get => _SecondaryColor;
            set
            {
                _SecondaryColor = value;
                OnSetSecondaryColor(value);
            }
        }
        public ITool CurrentTool
        {
            get => _CurrentTool;
            set
            {
                _CurrentTool = value;
                OnSetTool(value);
            }
        }


        public Canvas()
        {
            Image = new Image<Rgba32>(Configuration.Default, 1000, 1000, Color.White);
            OverlayImage = new Image<Rgba32>(Configuration.Default, Image.Height, Image.Width, Color.Transparent);

            Height = Image.Height;
            Width = Image.Width;

            Sprite = new Sprite();
            Sprite.RelativeSizeAxes = Axes.Both;
            Sprite.Texture = new Texture(Image.Width, Image.Height, true, osuTK.Graphics.ES30.All.Nearest);

            OverlaySprite = new Sprite();
            OverlaySprite.RelativeSizeAxes = Axes.Both;
            OverlaySprite.Texture = new Texture(OverlayImage.Width, OverlayImage.Height, true, osuTK.Graphics.ES30.All.Nearest);
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

            MainColor = Color.Black;
            SecondaryColor = Color.White;
        }


        #region move listener

        enum DrawingType : byte { None, Left, Right }
        DrawingType DrawType = DrawingType.None;
        int LastMouseX, LastMouseY;

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            if (base.OnMouseDown(e)) return true;

            if (e.Button == MouseButton.Left) DrawType = DrawingType.Left;
            else if (e.Button == MouseButton.Right) DrawType = DrawingType.Right;
            else return false;


            (LastMouseX, LastMouseY) = ((int) e.MouseDownPosition.X, (int) e.MouseDownPosition.Y);

            var (x, y) = ToImagePosition((int) e.MouseDownPosition.X, (int) e.MouseDownPosition.Y);
            if (e.Button == MouseButton.Left) CurrentTool.OnStart(x, y, this);
            else if (e.Button == MouseButton.Right) CurrentTool.OnStartRight(x, y, this);

            return false;
        }
        protected override void OnMouseUp(MouseUpEvent e)
        {
            base.OnMouseUp(e);
            DrawType = DrawingType.None;

            var (sx, sy) = ToImagePosition((int) e.MouseDownPosition.X, (int) e.MouseDownPosition.Y);
            var (ex, ey) = ToImagePosition((int) e.MousePosition.X, (int) e.MousePosition.Y);

            if (e.Button == MouseButton.Left) CurrentTool.OnEnd(sx, sy, ex, ey, this);
            else if (e.Button == MouseButton.Right) CurrentTool.OnEndRight(sx, sy, ex, ey, this);
        }
        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            if (base.OnMouseMove(e)) return true;
            if (DrawType == DrawingType.None) return false;

            var (x, y) = ToImagePosition((int) LastMouseX, (int) LastMouseY);
            var (tx, ty) = ToImagePosition((int) e.MousePosition.X, (int) e.MousePosition.Y);

            (LastMouseX, LastMouseY) = ((int) e.MousePosition.X, (int) e.MousePosition.Y);

            if (DrawType == DrawingType.Left) CurrentTool.OnMove(x, y, tx, ty, this);
            else if (DrawType == DrawingType.Right) CurrentTool.OnMoveRight(x, y, tx, ty, this);

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
            if (Image[x, y] == MainColor) return false;

            Image[x, y] = MainColor;
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