using System.Collections.ObjectModel;

namespace Mapsui.Styles;

public class StyleCollection : BaseStyle
{
    public Collection<IStyle> Styles { get; set; } = [];
}
