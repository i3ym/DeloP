using System.IO;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;

namespace DeloP.Controls
{
    public class DeloFileSelector : BasicFileSelector { }
    public class DeloFileSelector2 : FileSelector
    {
        protected override DirectorySelectorBreadcrumbDisplay CreateBreadcrumb() => new DeloDirectorySelectorBreadcrumbDisplay();
        protected override DirectorySelectorDirectory CreateDirectoryItem(DirectoryInfo directory, string? displayName = null) => new DeloDirectorySelectorDirectory(directory, displayName);
        protected override DirectoryListingFile CreateFileItem(FileInfo file) => new DeloDirectoryListingFile(file);
        protected override DirectorySelectorDirectory CreateParentDirectoryItem(DirectoryInfo directory) => new DeloDirectorySelectorDirectory(directory, "..");
        protected override ScrollContainer<Drawable> CreateScrollContainer() => new BasicScrollContainer();


        protected class DeloDirectorySelectorBreadcrumbDisplay : DirectorySelectorBreadcrumbDisplay
        {
            protected override DirectorySelectorDirectory CreateDirectoryItem(DirectoryInfo directory, string? displayName = null) => new DeloDirectorySelectorDirectory(directory, displayName);
            protected override DirectorySelectorDirectory CreateRootDirectoryItem() => new DeloDirectorySelectorDirectory(new DirectoryInfo(".."), ".");
        }
        protected class DeloDirectorySelectorDirectory : DirectorySelectorDirectory
        {
            protected override IconUsage? Icon => FontAwesome.Solid.Folder;

            public DeloDirectorySelectorDirectory(DirectoryInfo directory, string? displayName = null) : base(directory, displayName) { }
        }
        protected class DeloDirectoryListingFile : DirectoryListingFile
        {
            protected override IconUsage? Icon => FontAwesome.Solid.File;

            public DeloDirectoryListingFile(FileInfo file) : base(file) { }
        }
    }
}