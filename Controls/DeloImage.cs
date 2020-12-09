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
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using Img = SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>;

namespace DeloP.Controls
{
    public class DeloImage : CompositeDrawable, IEnumerable<Img>
    {
        public event Action<ScrollEvent> ScrollAction = delegate { };

        public IBindableList<Layer> Layers => _Layers;
        readonly BindableList<Layer> _Layers = new();

        public Img ActiveImage => ActiveLayer.Image;
        public Layer ActiveLayer
        {
            get => Layers[ClampActiveLayer()];
            set => ActiveLayerIndex = _Layers.IndexOf(value);
        }
        public int ActiveLayerIndex { get => ActiveLayerBindable.Value; set => ActiveLayerBindable.Value = value; }
        public readonly Bindable<int> ActiveLayerBindable = new BindableInt(0);

        public Img this[int layer] => Layers[ClampLayer(layer)].Image;
        public Rgba32 this[int x, int y]
        {
            get => AsOne()[x, y]; // TODO: optimize
            set => ActiveImage[x, y] = value;
        }


        [BackgroundDependencyLoader]
        void Load() => Layers.CollectionChanged += (_, __) => ClampActiveLayer();

        protected override void Update()
        {
            foreach (var layer in Layers)
            {
                if (!layer.DoUpdate) continue;

                if (layer.UpdateBounds.HasValue) UpdateRect(layer.Sprite, layer.Upload, layer.UpdateBounds.Value);
                else UpdateRect(layer.Sprite, layer.Upload);

                layer.DoUpdate = false;
                layer.UpdateBounds = null;
            }

            base.Update();
        }
        protected override bool OnScroll(ScrollEvent e)
        {
            ScrollAction(e);
            return base.OnScroll(e);
        }

        public void AddLayer() => AddLayer(CreateImage());
        public void AddLayer(Color background) => AddLayer(CreateImage(background));
        public void AddLayer(Img image)
        {
            var upload = new CachedTextureUpload() { Image = image };
            var sprite = new Sprite() { RelativeSizeAxes = Axes.Both, Texture = new Texture(image.Width, image.Height, true, osuTK.Graphics.ES30.All.Nearest) };
            sprite.Texture.TextureGL.BypassTextureUploadQueueing = true;

            var layer = new Layer(sprite, image, upload);
            layer.IsVisible.ValueChanged += e => sprite.Alpha = e.NewValue ? 1 : 0;

            _Layers.Add(layer);
            AddInternal(sprite);
            UpdateLayer(Layers.Count - 1);

            ActiveLayerIndex = Layers.Count - 1;
        }

        public void DeleteActiveLayer() => DeleteLayer(ActiveLayerIndex);
        public void DeleteLayer(int index)
        {
            if (Layers.Count == 1) return;

            RemoveInternal(Layers[index].Sprite);
            _Layers.RemoveAt(index);
        }


        public void UpdateActiveLayer() => UpdateLayer(ActiveLayerIndex);
        public void UpdateActiveLayer(RectangleI bounds) => UpdateLayer(ActiveLayerIndex, bounds);
        public void UpdateLayer(int layer) => Layers[layer].ScheduleUpdate();
        public void UpdateLayer(int layer, RectangleI bounds) => Layers[layer].ScheduleUpdate(bounds);

        static void UpdateRect(Sprite sprite, ITextureUpload upload) => sprite.Texture.SetData(upload);
        static void UpdateRect(Sprite sprite, ITextureUpload upload, RectangleI bounds)
        {
            var b = upload.Bounds;
            upload.Bounds = bounds;
            sprite.Texture.SetData(upload);

            upload.Bounds = b;
        }

        public void Resize(int dx, int dy, int width, int height)
        {
            var newlayers = Layers.Select(x =>
            {
                var newimg = CreateImage();
                newimg.Mutate(ctx => ctx.DrawImage(x.Image, new Point(dx, dy), 1f));

                x.Sprite.Width = width;
                x.Sprite.Height = height;

                return new Layer(x.Sprite, newimg, new CachedTextureUpload() { Image = newimg });
            }).ToArray();

            _Layers.Clear();
            _Layers.AddRange(newlayers);

            Width = width;
            Height = height;
        }
        public void Clear()
        {
            _Layers.Clear();
            ClearInternal();

            AddLayer(Color.White);
        }

        public Img AsOne()
        {
            var output = CreateImage();
            output.Mutate(x =>
            {
                foreach (var image in this)
                    x.DrawImage(image, 1f);
            });

            return output;
        }

        Img CreateImage() => CreateImage(Color.Transparent);
        Img CreateImage(Color color) => new Img(SixLabors.ImageSharp.Configuration.Default, (int) Width, (int) Height, color);

        int ClampActiveLayer() => ActiveLayerIndex = ClampLayer(ActiveLayerIndex);
        int ClampLayer(int layer) => Math.Clamp(layer, 0, Layers.Count - 1);


        public IEnumerator<Img> GetEnumerator() => Layers.Select(x => x.Image).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}