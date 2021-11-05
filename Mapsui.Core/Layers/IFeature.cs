using System.Collections.Generic;
using Mapsui.Styles;

namespace Mapsui.Layers
{
    public interface IFeature
    {
        ICollection<IStyle> Styles { get; }
        object? this[string key] { get; set; }
        IEnumerable<string> Fields { get; }
        MRect? Extent { get; }
    }
}
