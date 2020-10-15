using System.Threading.Tasks;
using DeloP.Containers;
using DeloP.Controls;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.IO.Stores;

namespace DeloP
{
    public class MainScreen : osu.Framework.Game
    {
        const int ToolPanelWidth = 68, ToolSettingsHeight = 32, MenuHeight = 24;

        readonly FullCanvas FullCanvas = new FullCanvas() { Y = ToolSettingsHeight + 2, X = ToolPanelWidth + 2 };

        [BackgroundDependencyLoader]
        void Load()
        {
            Resources.AddStore(new DllResourceStore("DeloP.dll"));
            Fonts.AddStore(new GlyphStore(Resources, "Fonts/Ubuntu"));

            Window.Title = "DeloP";
            Window.WindowStateChanged += (obj, e) => Task.Run(async () => { await Task.Delay(5); Invalidate(); });

            Children = new Drawable[]
            {
                new Box() { RelativeSizeAxes = Axes.Both, Colour = Colors.DarkBackground },
                new Menu(FullCanvas.Canvas) { RelativeSizeAxes = Axes.X, Height = MenuHeight, Depth = -2 },
                new ToolSettingsPanel(FullCanvas.Canvas) { RelativeSizeAxes = Axes.X, Y = MenuHeight, Height = ToolSettingsHeight, Depth = -1 },
                new ToolPanel(FullCanvas.Canvas) { RelativeSizeAxes = Axes.Y, Y = ToolSettingsHeight + MenuHeight, Width = ToolPanelWidth, Depth = -1 },
                FullCanvas,
            };
        }


        protected override bool OnMouseDown(MouseDownEvent e) => FullCanvas.MouseDown(e);
        protected override void OnMouseUp(MouseUpEvent e) => FullCanvas.MouseUp(e);
        protected override bool OnMouseMove(MouseMoveEvent e) => FullCanvas.MouseMove(e);
    }
}