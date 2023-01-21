using System;

namespace Mapsui.Styles.Thematics;

public class ThemeStyle : Style, IThemeStyle
{
    private readonly Func<IFeature, IStyle?> _method;

    public ThemeStyle(Func<IFeature, IStyle?> method)
    {
        _method = method;
    }

    public IStyle? GetStyle(IFeature attribute)
    {
        return _method(attribute);
    }
}
