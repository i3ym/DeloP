using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Events;
using SkiaSharp;

namespace DeloP.Controls
{
    public class DeloImage : CompositeDrawable, IEnumerable<SKBitmap>
    {
        public event Action<ScrollEvent> ScrollEvent = delegate { };

        public IBindableList<Layer> Layers => _Layers;
        readonly BindableList<Layer> _Layers = new();

        public SKCanvas ActiveCanvas => ActiveLayer.Canvas;
        public SKBitmap ActiveImage => ActiveLayer.Image;
        public Layer ActiveLayer
        {
            get => Layers[ClampActiveLayer()];
            set => ActiveLayerIndex = _Layers.IndexOf(value);
        }
        public int ActiveLayerIndex { get => ActiveLayerBindable.Value; set => ActiveLayerBindable.Value = value; }
        public readonly Bindable<int> ActiveLayerBindable = new BindableInt(0);

        public SKCanvas OverlayCanvas => OverlayLayer.Canvas;
        public SKBitmap OverlayImage => OverlayLayer.Image;
        readonly Layer OverlayLayer = new Layer(new SKBitmap(1, 1)) { Depth = -1 };


        [BackgroundDependencyLoader]
        void Load()
        {
            Layers.CollectionChanged += (_, __) => { if (Layers.Count != 0) ClampActiveLayer(); };
            AddInternal(OverlayLayer);
            AddLayer();
        }

        public SKBitmap this[int layer] => Layers[ClampLayer(layer)].Image;
        public SKColor this[int x, int y]
        {
            get => AsOne().GetPixel(x, y); // TODO: optimize
            set => ActiveImage.SetPixel(x, y, value);
        }

        protected override bool OnScroll(ScrollEvent e)
        {
            ScrollEvent(e);
            return base.OnScroll(e);
        }

        public void AddLayer() => AddLayer(CreateImage());
        public void AddLayer(SKColor background) => AddLayer(CreateImage(background));
        public void AddLayer(SKBitmap image)
        {
            var layer = new Layer(image);

            _Layers.Add(layer);
            AddInternal(layer);
            UpdateLayer(Layers.Count - 1);

            ActiveLayerIndex = Layers.Count - 1;
        }

        public void DeleteActiveLayer() => DeleteLayer(ActiveLayerIndex);
        public void DeleteLayer(int index)
        {
            if (Layers.Count == 1) return;

            RemoveInternal(Layers[index]);
            _Layers.RemoveAt(index);
        }


        public void UpdateActiveLayer(RectangleI? bounds = null) => UpdateLayer(ActiveLayerIndex, bounds);
        public void UpdateLayer(int layer, RectangleI? bounds = null) => Layers[layer].ScheduleUpdate(bounds);
        public void UpdateOverlay(RectangleI? bounds = null) => OverlayLayer.ScheduleUpdate(bounds);

        public void Resize(int dx, int dy, int width, int height)
        {
            Width = width;
            Height = height;

            foreach (var layer in InternalChildren.Cast<Layer>())
            {
                var oldimg = layer.Image;

                layer.SetImage(CreateImage(), false);
                layer.Canvas.DrawBitmap(oldimg, SKRect.Create(dx, dy, oldimg.Width, oldimg.Height));

                layer.ScheduleUpdate();
            }
        }
        public void Clear()
        {
            _Layers.Clear();
            ClearInternal();

            AddLayer(new SKColor(255, 255, 255));
        }

        public SKBitmap AsOne()
        {
            var output = CreateImage();
            using var canvas = new SKCanvas(output);

            foreach (var image in this)
                canvas.DrawBitmap(image, 0, 0);

            return output;
        }

        SKBitmap CreateImage() => new SKBitmap(Math.Max(1, (int) Width), Math.Max(1, (int) Height), SKColorType.Rgba8888, SKAlphaType.Unpremul);
        SKBitmap CreateImage(SKColor color)
        {
            var bitmap = CreateImage();
            using (var canvas = new SKCanvas(bitmap))
                canvas.Clear(color);

            return bitmap;
        }

        int ClampActiveLayer() => ActiveLayerIndex = ClampLayer(ActiveLayerIndex);
        int ClampLayer(int layer) => Math.Clamp(layer, 0, Layers.Count - 1);


        public IEnumerator<SKBitmap> GetEnumerator() => Layers.Select(x => x.Image).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}