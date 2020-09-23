using osu.Framework.Graphics.Containers;

namespace DeloP.ToolSettings
{
    public abstract class ToolSettingBase : CompositeDrawable
    {
        public virtual bool AppliesTo(ITool tool) => true;
    }
}