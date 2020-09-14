using System.Threading.Tasks;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;

namespace DeloP
{
    public class MainScreen : osu.Framework.Game
    {
        readonly ToolPanel ToolPanel;
        readonly Canvas Canvas;
        readonly ResizableContainer CanvasContainer;

        public MainScreen()
        {
            Canvas = new Canvas();
            CanvasContainer = new ResizableContainer();
            CanvasContainer.Top.Alpha = CanvasContainer.Left.Alpha = CanvasContainer.TopLeft.Alpha = 0;
            CanvasContainer.TopRight.Alpha = CanvasContainer.BottomLeft.Alpha = 0;

            Canvas.OnImageReplace += img =>
            {
                Canvas.Width = CanvasContainer.Width = img.Width;
                Canvas.Height = CanvasContainer.Height = img.Height;
            };
            CanvasContainer.OnResize += () =>
            {
                Canvas.ChangeSize((int) CanvasContainer.Width, (int) CanvasContainer.Height);
                Canvas.Position = CanvasContainer.Position;
            };

            ToolPanel = new ToolPanel(Canvas) { RelativeSizeAxes = Axes.Both };
        }
        protected override void LoadComplete()
        {
            base.LoadComplete();

            Window.Title = "DeloP";
            Window.WindowStateChanged += (obj, e) => Task.Run(async () => { await Task.Delay(5); Invalidate(); });

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
                            ToolPanel,
                            new Container() { Children = new Drawable[] { Canvas, CanvasContainer } }
                        },
                    },
                    RelativeSizeAxes = Axes.Both
                }
            };
        }
    }
}