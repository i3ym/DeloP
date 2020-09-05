using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;

namespace Painter
{
    public class MainScreen : osu.Framework.Game
    {
        Canvas Canvas = null!;

        public MainScreen()
        {
            Name = "Painter";
        }
        protected override void LoadComplete()
        {
            base.LoadComplete();

            Children = new Drawable[]
            {
                new Box() { Colour = Colour4.DarkGray, RelativeSizeAxes = Axes.Both },
                Canvas = new Canvas(Window)
            };

            Canvas.Scale = new osuTK.Vector2(10, 10);
        }
    }
}