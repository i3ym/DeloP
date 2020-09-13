using System.Threading.Tasks;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;

namespace Painter
{
    public class MainScreen : osu.Framework.Game
    {
        readonly ToolPanel ToolPanel;
        readonly Canvas Canvas;

        public MainScreen()
        {
            Canvas = new Canvas();
            Canvas.Scale = new osuTK.Vector2(1000 / Canvas.Image.Width, 1000 / Canvas.Image.Height);

            ToolPanel = new ToolPanel(Canvas) { RelativeSizeAxes = Axes.Both };
        }
        protected override void LoadComplete()
        {
            base.LoadComplete();

            Window.Title = "Painter";
            Window.WindowStateChanged += (obj, e) => Task.Run(async () => { await Task.Delay(5); Invalidate(); });

            Children = new Drawable[]
            {
                new Box() { Colour = Colour4.DarkGray, RelativeSizeAxes = Axes.Both },
                new GridContainer()
                {
                    ColumnDimensions = new [] { new Dimension(GridSizeMode.Absolute, 55), new Dimension(GridSizeMode.AutoSize) },
                    Content = new Drawable[][] { new Drawable[] { ToolPanel, Canvas }, },
                    RelativeSizeAxes = Axes.Both
                }
            };
        }
    }
}