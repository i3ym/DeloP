using System.Threading.Tasks;
using DeloP.Containers;
using DeloP.Controls;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.IO.Stores;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;

namespace DeloP
{
    public class MainScreen : osu.Framework.Game
    {
        const int ToolPanelWidth = 68, ToolSettingsHeight = 32, MenuHeight = 24;

        [Resolved] Game Game { get; set; } = null!;
        readonly FullCanvas FullCanvas = new FullCanvas() { Y = ToolSettingsHeight + 2, X = ToolPanelWidth + 2 };

        [BackgroundDependencyLoader]
        void Load()
        {
            Resources.AddStore(new DllResourceStore("DeloP.dll"));
            Fonts.AddStore(new GlyphStore(Resources, "Fonts/Ubuntu"));

            Window.Title = "DeloP";
            Children = new Drawable[]
            {
                new Box() { RelativeSizeAxes = Axes.Both, Colour = Colors.DarkBackground },
                new DeloMenu(Direction.Horizontal, true) { RelativeSizeAxes = Axes.X, Height = MenuHeight, Depth = -2, Items = CreateMenuItems() },
                new ToolSettingsPanel(FullCanvas.Canvas) { RelativeSizeAxes = Axes.X, Y = MenuHeight, Height = ToolSettingsHeight, Depth = -1 },
                new ToolPanel(FullCanvas.Canvas) { RelativeSizeAxes = Axes.Y, Y = ToolSettingsHeight + MenuHeight, Width = ToolPanelWidth, Depth = -1 },
                FullCanvas,
            };
        }

        MenuItem[] CreateMenuItems()
        {
            void clickNew()
            {
                FullCanvas.Canvas.Image.GetPixelSpan().Fill(Color.White);
                FullCanvas.Canvas.UpdateImage();
            }


            return new[]
            {
                new MenuItem("Файл", () => { })
                {
                    Items = new[]
                    {
                        new MenuItem("Новый", clickNew),
                        new MenuItem("Коровый", () => System.Console.WriteLine("чё смотриш пёс")),
                        new MenuItem("Выйти", Game.Exit),
                    }
                },
                new MenuItem("Нефайл", () => { })
                {
                    Items = new[]
                    {
                        new MenuItem("хуй", () => {}),
                    }
                }
            };
        }


        protected override bool OnMouseDown(MouseDownEvent e) => FullCanvas.MouseDown(e);
        protected override void OnMouseUp(MouseUpEvent e) => FullCanvas.MouseUp(e);
        protected override bool OnMouseMove(MouseMoveEvent e) => FullCanvas.MouseMove(e);
    }
}