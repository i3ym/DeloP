using System.Collections.Immutable;
using System.IO;
using System.Linq;
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
using SixLabors.ImageSharp.PixelFormats;
using SkiaSharp;

namespace DeloP
{
    public class MainScreen : osu.Framework.Game
    {
        const int ToolPanelWidth = 68, ToolSettingsHeight = 32, MenuHeight = 24;

        [Resolved] Game Game { get; set; } = null!;
        readonly FullCanvas FullCanvas = new FullCanvas() { RelativeSizeAxes = Axes.Both, Y = ToolSettingsHeight + 2, X = ToolPanelWidth + 2 };

        [BackgroundDependencyLoader]
        void Load()
        {
            Resources.AddStore(new DllResourceStore("DeloP.dll"));
            Fonts.AddStore(new GlyphStore(Resources, "Fonts/Ubuntu"));

            var tools = Enumerable.Empty<ITool>()
                .Append(new MoveTool(FullCanvas))
                .Append(new PencilTool(FullCanvas))
                .Append(new EraserTool(FullCanvas))
                .Append(new PipetteTool(FullCanvas))
                .Append(new LineTool(FullCanvas))
                .Append(new RectangleTool(FullCanvas))
                .Append(new TriangleTool(FullCanvas))
                .Append(new FillTool(FullCanvas))
                .ToImmutableArray();

            Window.Title = "DeloP";
            Children = new Drawable[]
            {
                new Box() { RelativeSizeAxes = Axes.Both, Colour = Colors.DarkBackground },
                new DeloMenu(Direction.Horizontal, true) { RelativeSizeAxes = Axes.X, Height = MenuHeight, Depth = -2, Items = CreateMenuItems() },
                new ToolSettingsPanel(FullCanvas) { RelativeSizeAxes = Axes.X, Y = MenuHeight, Height = ToolSettingsHeight, Depth = -1 },
                new ToolPanel(FullCanvas.Canvas, tools) { RelativeSizeAxes = Axes.Y, Y = ToolSettingsHeight + MenuHeight, Width = ToolPanelWidth, Depth = -1 },
                FullCanvas,
                new LayersWindow(FullCanvas) { Width = 300, Height = 600 },
            };

            FullCanvas.Canvas.CurrentTool = tools.First();
        }

        MenuItem[] CreateMenuItems()
        {
            string? openedPath = null;

            static void save(SKBitmap image, string file)
            {
                using var stream = File.OpenWrite(file);
                image.Encode(stream, SKEncodedImageFormat.Png, 9);
            }
            void clickNew()
            {
                FullCanvas.Canvas.Image.Clear();
                FullCanvas.Canvas.Image.UpdateActiveLayer();
            }
            void clickOpen()
            {
                var selector = new BackgroundDrawable<DeloFileSelector>(new DeloFileSelector()) { RelativeSizeAxes = Axes.Both, Depth = -99, Background = Colors.Background };
                selector.Child.CurrentFile.ValueChanged += e => Task.Run(() =>
                {
                    try
                    {
                        FullCanvas.Canvas.Image.Clear();
                        FullCanvas.Canvas.Image.AddLayer(SKBitmap.FromImage(SKImage.FromEncodedData(File.OpenWrite(e.NewValue.FullName))));
                        openedPath = e.NewValue.FullName;

                        Schedule(() => Remove(selector));
                    }
                    catch { }
                });

                Schedule(() => Add(selector));
            }
            void clickSave() => Task.Run(() =>
            {
                if (openedPath is null) clickSaveAs();
                else save(FullCanvas.Canvas.Image.AsOne(), openedPath);
            });
            void clickSaveAs()
            {
                var selector = new BackgroundDrawable<DeloFileSaveSelector>(new DeloFileSaveSelector()) { RelativeSizeAxes = Axes.Both, Depth = -99, Background = Colors.Background };
                selector.Child.OnSelect += p => Task.Run(() =>
                {
                    save(FullCanvas.Canvas.Image.AsOne(), openedPath = p);
                    Schedule(() => Remove(selector));
                });

                Schedule(() => Add(selector));
            }
            void clickExit() => Game.Exit();


            return new[]
            {
                new MenuItem("Файл", () => { })
                {
                    Items = new[]
                    {
                        new MenuItem("Новый", clickNew),
                        new MenuItem("Открыть", clickOpen),
                        new MenuItem("Сохранить", clickSave),
                        new MenuItem("Сохранить как...", clickSaveAs),
                        new MenuItem("Выйти", clickExit),
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