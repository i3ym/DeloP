using System;
using osu.Framework.Graphics.Textures;

namespace Painter
{
    public static class Textures
    {
        public static readonly Lazy<Texture> SelectedTool = Tex("tool_selected");


        static Lazy<Texture> Tex(string name) =>
            new Lazy<Texture>(() =>
            {
                var img = SpriteStore.Load("res.sprites." + name + ".png");
                var texture = new Texture(img.Width, img.Height, false, osuTK.Graphics.ES30.All.Nearest);
                texture.SetData(new TextureUpload(img));

                return texture;
            });
    }
}