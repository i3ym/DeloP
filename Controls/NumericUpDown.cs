using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Layout;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace DeloP.Controls
{
    public class NumericUpDown : CompositeDrawable
    {
        public event Action<int> OnChangeValue = delegate { };

        [Resolved] TextureStore TextureStore { get; set; } = null!;

        int _Value;
        public int Value
        {
            get => _Value;
            set
            {
                if (value < 1) value = 1;

                _Value = value;
                TextBox.Text = value.ToString();

                OnChangeValue(value);
            }
        }

        readonly NumberOnlyTextBox TextBox;
        readonly Button Up, Down;

        public NumericUpDown()
        {
            InternalChildren = new Drawable[]
            {
                TextBox = new NumberOnlyTextBox() { RelativeSizeAxes = Axes.Y, BorderColour = Colors.DarkBackground, BorderThickness = 1 },
                Up = new BasicButton(),
                Down = new BasicButton()
            };


            var upTex = new Sprite();
            var upimg = Image.Load<Rgba32>(GetType().Assembly.GetManifestResourceStream("DeloP.Textures.hide_show.png")!);
            var downimg = upimg.Clone();

            upimg.Mutate(ctx => ctx.Rotate(RotateMode.Rotate270));

            upTex.Texture = new Texture(upimg.Width, upimg.Height, true, osuTK.Graphics.ES30.All.Nearest);
            upTex.Texture.SetData(new TextureUpload(upimg));
            Up.Child = upTex;

            var downTex = new Sprite();
            downimg.Mutate(ctx => ctx.Rotate(RotateMode.Rotate90));

            downTex.Texture = new Texture(downimg.Width, downimg.Height, true, osuTK.Graphics.ES30.All.Nearest);
            downTex.Texture.SetData(new TextureUpload(downimg));
            Down.Child = downTex;


            TextBox.Width = 50;
            Up.Width = Down.Width = Up.Height = Down.Height = Down.Y = upTex.Width;
            Up.X = Down.X = TextBox.Width + 4;
            Width = Up.X + Up.Width;


            TextBox.OnTextChanged += () => Value = int.Parse(TextBox.Text);
            Up.Action += () => Value++;
            Down.Action += () => Value--;

            Value = 1;
        }

        protected override bool OnInvalidate(Invalidation invalidation, InvalidationSource source)
        {
            Up.Y = DrawHeight / 4 - Up.Height / 2;
            Down.Y = Up.Y + DrawHeight / 2;

            return base.OnInvalidate(invalidation, source);
        }
    }
}