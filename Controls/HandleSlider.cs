using DeloP.Containers;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;

namespace DeloP.Controls
{
    public class HandleSlider : SliderBar<int>
    {
        readonly Box HandleDrawable;
        readonly SpriteText Text;

        public HandleSlider()
        {
            HandleDrawable = new Box();
            HandleDrawable.RelativeSizeAxes = Axes.Y;
            HandleDrawable.Width = 6;
            HandleDrawable.Colour = Colors.DarkBackground;
            HandleDrawable.RelativePositionAxes = Axes.X;
            HandleDrawable.Origin = Anchor.TopCentre;

            Text = new SpriteText();
            Text.Colour = Colour4.White;
            Text.RelativeSizeAxes = Axes.Both;


            var border = new BorderContainer();
            border.RelativeSizeAxes = Axes.Both;
            border.BorderColour = Colour4.White;
            border.BackgroundColour = Colors.ToolSelection;
            border.BorderThickness = 2;
            border.Masking = true;

            border.Add(HandleDrawable);
            border.Add(Text);

            Add(border);
        }

        protected override void UpdateValue(float value)
        {
            HandleDrawable.X = value;
            Text.Text = (int) (value * 100) + "%";
        }
    }
}