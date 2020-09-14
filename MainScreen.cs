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

            Canvas.OnImageReplace += img =>
            {
                Canvas.Width = CanvasContainer.Width = img.Width;
                Canvas.Height = CanvasContainer.Height = img.Height;
            };
            CanvasContainer.OnResize += e =>
            {
                Canvas.ChangeSize((int) -(e.EndPos.X - e.StartPos.X), (int) -(e.EndPos.Y - e.StartPos.Y), (int) e.EndSize.X, (int) e.EndSize.Y);
                Canvas.Position = e.EndPos;
            };
            Canvas.OnMove += () => CanvasContainer.Position = Canvas.Position;

            ToolPanel = new ToolPanel(Canvas) { RelativeSizeAxes = Axes.Y, Width = 68 };
            Canvas.Y = 2;
            Canvas.X = ToolPanel.Width + Canvas.Y;
        }
        protected override void LoadComplete()
        {
            base.LoadComplete();

            Window.Title = "DeloP";
            Window.WindowStateChanged += (obj, e) => Task.Run(async () => { await Task.Delay(5); Invalidate(); });

            Children = new Drawable[]
            {
                new Box() { Colour = Colors.DarkBackground, RelativeSizeAxes = Axes.Both },
                new Container()
                {
                    Children = new Drawable[]
                    {
                        new Container() { Children = new Drawable[] { Canvas, CanvasContainer }, RelativeSizeAxes = Axes.Both },
                        ToolPanel,
                    },
                    RelativeSizeAxes = Axes.Both
                }
            };
        }
    }
}