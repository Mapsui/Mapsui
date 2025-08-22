using System;

namespace Mapsui.Styles.Thematics;

public class ThemeStyle : BaseStyle, IThemeStyle
{
    private readonly Func<IFeature, Viewport, IStyle?> _method;

    public ThemeStyle(Func<IFeature, IStyle?> method)
    {
        _method = (f, v) => GetStyle(method, f, v);
    }

    public ThemeStyle(Func<IFeature, Viewport, IStyle?> method)
    {
        _method = method;
    }

    public IStyle? GetStyle(IFeature attribute, Viewport viewport)
    {
        return _method(attribute, viewport);
    }

    public override string ToString()
    {
        return $"ThemeStyle: {GetType().Name}";
    }

    private static IStyle? GetStyle(Func<IFeature, IStyle?> method, IFeature feature, Viewport _) => method(feature);
}
