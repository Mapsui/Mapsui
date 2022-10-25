using Mapsui.Styles;

namespace Mapsui.Rendering
{
    public interface ILabelCache
    {
        object GetOrCreateTypeface(Font font);
    }
}