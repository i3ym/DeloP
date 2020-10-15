using System.Collections.Generic;
using System.Threading.Tasks;
using DeloP.Containers;
using DeloP.Controls;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;

namespace DeloP
{
    public class MainScreen : osu.Framework.Game
    {
        readonly FullCanvas FullCanvas;
        readonly ToolPanel ToolPanel;
        readonly ToolSettingsPanel ToolSettingsPanel;

        public MainScreen()
        {
            FullCanvas = new FullCanvas();

            ToolSettingsPanel = new ToolSettingsPanel(FullCanvas.Canvas);
            ToolSettingsPanel.Height = 32;
            ToolSettingsPanel.RelativeSizeAxes = Axes.X;

            ToolPanel = new ToolPanel(FullCanvas.Canvas);
            ToolPanel.RelativeSizeAxes = Axes.Y;
            ToolPanel.Y = ToolSettingsPanel.Height;
            ToolPanel.Width = 68;

            FullCanvas.Y = ToolSettingsPanel.Height + 2;
            FullCanvas.X = ToolPanel.Width + 2;
        }

        [BackgroundDependencyLoader]
        void Load()
        {
            Window.Title = "DeloP";
            Window.WindowStateChanged += (obj, e) => Task.Run(async () => { await Task.Delay(5); Invalidate(); });

            Children = new Drawable[]
            {
                new Box() { Colour = Colors.DarkBackground, RelativeSizeAxes = Axes.Both },
                FullCanvas,
                ToolPanel,
                ToolSettingsPanel,
            };
        }


        protected override bool OnMouseDown(MouseDownEvent e) => FullCanvas.MouseDown(e);
        protected override void OnMouseUp(MouseUpEvent e) => FullCanvas.MouseUp(e);
        protected override bool OnMouseMove(MouseMoveEvent e) => FullCanvas.MouseMove(e);
    }
}