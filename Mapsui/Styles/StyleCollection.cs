using System.Collections.ObjectModel;

namespace Mapsui.Styles;

public class StyleCollection : Style
{
    public Collection<IStyle> Styles { get; set; } = [];
}
