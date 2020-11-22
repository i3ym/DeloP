using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;

namespace DeloP.Controls
{
    public class DeloMenu : BasicMenu
    {
        public DeloMenu(Direction direction, bool topLevelMenu = false) : base(direction, topLevelMenu) { }

        [BackgroundDependencyLoader]
        void Load() => BackgroundColour = Colors.Background.Darken(.1f);


        protected override DrawableMenuItem CreateDrawableMenuItem(MenuItem item) => new DeloMenuItem(item);
        protected override Menu CreateSubMenu() => new DeloMenu(Direction.Vertical) { Anchor = Direction == Direction.Horizontal ? Anchor.BottomLeft : Anchor.TopRight };


        public class DeloMenuItem : DrawableMenuItem
        {
            public DeloMenuItem(MenuItem item) : base(item) { }

            [BackgroundDependencyLoader]
            void Load()
            {
                BackgroundColour = Colors.Background;
                BackgroundColourHover = Colors.DarkBackground;
            }

            protected override Drawable CreateContent() => new SpriteText()
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                Padding = new MarginPadding(2),
                Font = FontUsage.Default.With(family: "Ubuntu"),
                Text = Item.Text.Value,
            };
        }
    }
}