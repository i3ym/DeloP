using System;
using System.IO;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osuTK;

namespace DeloP.Controls
{
    public class DeloFileSaveSelector : CompositeDrawable
    {
        public event Action<string> OnSelect = delegate { };

        readonly DeloDirectorySelector DirectorySelector;
        readonly ConfirmTextBox TextBox;

        public DeloFileSaveSelector()
        {
            DirectorySelector = new DeloDirectorySelector() { RelativeSizeAxes = Axes.Both };
            TextBox = new ConfirmTextBox() { RelativeSizeAxes = Axes.X, Anchor = Anchor.BottomLeft, Height = 40, Origin = Anchor.BottomLeft };
        }
        [BackgroundDependencyLoader]
        void Load()
        {
            TextBox.OnConfirm += t => OnSelect(Path.Combine(DirectorySelector.CurrentPath.Value.FullName, t));

            var grid = new GridContainer()
            {
                RelativeSizeAxes = Axes.Both,
                Content = new Drawable[][]
                {
                    new Drawable[] { DirectorySelector },
                    new Drawable[] { TextBox },
                }
            };

            AddInternal(grid);
        }


        class ConfirmTextBox : CompositeDrawable
        {
            public event Action<string> OnConfirm = delegate { };

            readonly DeloTextBox TextBox = new DeloTextBox() { RelativeSizeAxes = Axes.Both };
            readonly Button Button = new DeloButton() { RelativeSizeAxes = Axes.Y, AutoSizeAxes = Axes.X, Text = "Сохранить" };

            [BackgroundDependencyLoader]
            void Load()
            {
                Button.Action += () => OnConfirm(TextBox.Text);

                var grid = new GridContainer()
                {
                    RelativeSizeAxes = Axes.Both,
                    ColumnDimensions = new[] { new Dimension(), new Dimension(GridSizeMode.AutoSize), },
                    Content = new Drawable[][]
                    {
                        new Drawable[]
                        {
                            new BackgroundDrawable<TextBox>(TextBox) { RelativeSizeAxes = Axes.Both, Background = Colors.DarkBackground },
                            Button
                        }
                    }
                };

                AddInternal(grid);
            }
        }
        class DeloTextBox : TextBox
        {
            protected override Caret CreateCaret() => new DeloCaret();
            protected override SpriteText CreatePlaceholder() => new SpriteText();
            protected override void NotifyInputError() => this.FlashColour(Colour4.Red, 100);


            class DeloCaret : Caret
            {
                [BackgroundDependencyLoader]
                void Load()
                {
                    RelativeSizeAxes = Axes.Y;
                    AddInternal(new Box() { RelativeSizeAxes = Axes.Both, Colour = Colors.White });
                }

                public override void DisplayAt(Vector2 position, float? selectionWidth)
                {
                    this.MoveTo(position, 100, Easing.OutQuad);
                    this.ResizeWidthTo(selectionWidth ?? 2, 100, Easing.OutQuad);
                }
            }
        }
        class DeloButton : BasicButton
        {

        }
    }
}