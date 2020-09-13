using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;

namespace Painter
{
    public class MainScreen : osu.Framework.Game
    {
        ToolPanel ToolPanel = null!;
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
                new GridContainer()
                {
                    ColumnDimensions = new [] { new Dimension(GridSizeMode.Absolute, 100), new Dimension(GridSizeMode.AutoSize) },
                    Content = new Drawable[][]
                    {
                        new Drawable[]
                        {
                            ToolPanel = new ToolPanel() { RelativeSizeAxes = Axes.Both },
                            Canvas = new Canvas(Window)
                        },
                    },
                    RelativeSizeAxes = Axes.Both
                }
            };

            Canvas.Scale = new osuTK.Vector2(1000 / Canvas.Image.Width, 1000 / Canvas.Image.Height);
            ToolPanel.OnChoose += t => Canvas.CurrentTool = t;
            ToolPanel.OnChangeColor += c => Canvas.MainColor = c;
        }
    }
}